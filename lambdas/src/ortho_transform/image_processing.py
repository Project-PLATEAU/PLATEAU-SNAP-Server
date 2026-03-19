from os import environ
from typing import Final, TYPE_CHECKING

from ..shared.decorators import ApiError
from .geometry_processing import CoordList, Dimensions
from .geometry_processing import (
    _geometry_to_y_down_plane_coords,
    _safe_corr,
    _apply_homography_to_points, 
    _compute_axis_aligned_size_2d
)
from ..shared.lazy_imports import get_numpy, get_cv2

if TYPE_CHECKING:
    import numpy as np

BUFFER_PIXELS: Final[int] = int(environ.get("BUFFER_PIXELS", "200"))
MAX_DIMENSION: Final[int] = 4096

from ..shared.logger import get_logger
logger = get_logger(__name__)



def rectify_facade(image, pts, geometry_pts, facade_buffer_px=BUFFER_PIXELS):
    np = get_numpy()
    cv2 = get_cv2()
    
    if len(pts) < 3 or len(geometry_pts) < 3:
        logger.warning("rectify_facade: insufficient correspondences")
        raise ApiError(400, "Need ≥3 correspondences")

    plane2d = np.asarray(_geometry_to_y_down_plane_coords(geometry_pts), np.float32)
    img2d   = np.asarray(pts, np.float32)

    if len(plane2d) != len(img2d):
        raise ApiError(400, f"Point count mismatch: {len(plane2d)} vs {len(img2d)}")

    # Resolve point ordering ambiguities (starting index?) by testing cyclic shifts
    H_g2i = _find_homography_cyclic(plane2d, img2d)

    if H_g2i is None:
        logger.error(f"Homography failed. Plane: {plane2d.tolist()} Img: {img2d.tolist()}")
        raise ApiError(500, "findHomography failed: Points are degenerate or mismatched.")

    W, H = _compute_output_canvas_from_aspect_and_bbox_area(plane2d.tolist(), pts)
    m = max(50, min(W, H) // 20)

    pmin = plane2d.min(axis=0)
    pmax = plane2d.max(axis=0)
    pw, ph = pmax[0] - pmin[0], pmax[1] - pmin[1]

    iw, ih = float(W - 2*m), float(H - 2*m)
    bx, by = 0.0, 0.0

    # Calculate buffer relative to the target canvas size
    if iw > 1e-6 and ih > 1e-6 and facade_buffer_px > 0:
        bx = min((facade_buffer_px * pw) / iw, 0.25 * pw)
        by = min((facade_buffer_px * ph) / ih, 0.25 * ph)

    src_rect = np.array([
        [pmin[0] - bx, pmin[1] - by],
        [pmax[0] + bx, pmin[1] - by],
        [pmax[0] + bx, pmax[1] + by],
        [pmin[0] - bx, pmax[1] + by]
    ], dtype=np.float32)

    dst_rect = np.array([
        [m, m], [W - m, m], [W - m, H - m], [m, H - m]
    ], dtype=np.float32)

    H_g2d = cv2.getPerspectiveTransform(src_rect, dst_rect)
    try:
        H_g2i_inv = np.linalg.inv(H_g2i)
        H_i2d = H_g2d @ H_g2i_inv
    except np.linalg.LinAlgError:
        raise ApiError(500, "Failed to invert homography matrix")

    # Ensure the image isn't flipped (mirror effect) or rotated 180 degrees unnecessarily
    H_i2d = _fix_axis_flips_by_src_dst_correlation(H_i2d, H_g2i, H_g2d, src_rect, W, H)

    warped = cv2.warpPerspective(image, H_i2d, (W, H))
    warped_pts = _apply_homography_to_points(pts, H_i2d)

    return warped, [(float(x), float(y)) for x,y in warped_pts]


def _find_homography_cyclic(plane_pts: "np.ndarray", img_pts: "np.ndarray") -> "np.ndarray | None":
    """
    Calculates homography by testing all cyclic point shifts.
    Resolves rectangular ambiguity (0 vs 90 degree flips) by preferring
    transformations that align 'Geometry Down' with 'Image Down'.
    """
    np = get_numpy()
    cv2 = get_cv2()
    
    n = len(plane_pts)

    if n == 3:
        H = cv2.getAffineTransform(plane_pts, img_pts)
        return np.vstack([H, [0, 0, 1]])

    if n > 8:
        H, _ = cv2.findHomography(plane_pts, img_pts, cv2.RANSAC, 10.0, maxIters=2000)
        return H

    best_H = None
    best_metric = (float('inf'), -float('inf'))

    for shift in range(n):
        shifted_img = np.roll(img_pts, shift, axis=0)
        H, mask = cv2.findHomography(plane_pts, shifted_img, cv2.RANSAC, 10.0, maxIters=1000)
        if H is not None and abs(np.linalg.det(H)) > 1e-8:
            pts_h = np.hstack([plane_pts, np.ones((n, 1))])
            projected = (H @ pts_h.T).T
            projected = projected[:, :2] / projected[:, 2:3]
            error = np.mean(np.linalg.norm(projected - shifted_img, axis=1))
            vec_pts = np.array([[0.0, 0.0], [0.0, 1.0]], dtype=np.float32)
            vec_pts_h = np.hstack([vec_pts, np.ones((2, 1))])
            vec_proj = (H @ vec_pts_h.T).T
            vec_proj = vec_proj[:, :2] / vec_proj[:, 2:3]
            img_down_vec = vec_proj[1] - vec_proj[0]
            norm = np.linalg.norm(img_down_vec)
            alignment_score = (img_down_vec[1] / norm) if norm > 1e-6 else 0

            current_best_error = best_metric[0]
            is_better = False
            if error < current_best_error - 5.0:
                is_better = True
            elif abs(error - current_best_error) <= 5.0:
                if alignment_score > best_metric[1] + 0.1:
                    is_better = True
            if is_better:
                best_metric = (error, alignment_score)
                best_H = H

    return best_H


def _compute_output_canvas_from_aspect_and_bbox_area(geometry_pts: CoordList, coordinates_pts: CoordList) -> Dimensions:
    """
    Calculate output dimensions. Combines 3D Geometry aspect ratio with a 2D check.
    """
    np = get_numpy()
    cv2 = get_cv2()
    
    real_width, real_height = _compute_axis_aligned_size_2d(geometry_pts)
    if real_width <= 1e-3 or real_height <= 1e-3:
        target_aspect_ratio = 1.0
    else:
        target_aspect_ratio = real_width / real_height
    vis_ratio = None
    visual_max_dim = 0.0

    try:
        img_array = np.array(coordinates_pts, dtype=np.float32)
        _, (vis_w, vis_h), _ = cv2.minAreaRect(img_array)
        vis_w = max(vis_w, 1.0)
        vis_h = max(vis_h, 1.0)
        visual_max_dim = max(vis_w, vis_h)

        if target_aspect_ratio < 1.0:
            vis_ratio = min(vis_w, vis_h) / max(vis_w, vis_h)
        else:
            vis_ratio = max(vis_w, vis_h) / min(vis_w, vis_h)
    except Exception as e:
        logger.warning(f"Failed to compute visual aspect ratio: {e}")
        visual_max_dim = 512.0

    if vis_ratio is not None:
        if vis_ratio > target_aspect_ratio * 1.2:
            clamped_ratio = vis_ratio * 0.9
            logger.info(
                f"Geometry ratio ({target_aspect_ratio:.3f}) is suspiciously thin compared to "
                f"visual ratio ({vis_ratio:.3f}). Clamping to {clamped_ratio:.3f}."
            )
            target_aspect_ratio = max(target_aspect_ratio, clamped_ratio)

    if target_aspect_ratio > 1.0:
        calculated_width = int(visual_max_dim)
        calculated_height = int(calculated_width / target_aspect_ratio)
    else:
        calculated_height = int(visual_max_dim)
        calculated_width = int(calculated_height * target_aspect_ratio)

    if calculated_width > MAX_DIMENSION or calculated_height > MAX_DIMENSION:
        scale = MAX_DIMENSION / max(calculated_width, calculated_height)
        calculated_width = int(calculated_width * scale)
        calculated_height = int(calculated_height * scale)

    MIN_DIMENSION = 64
    if calculated_width < MIN_DIMENSION or calculated_height < MIN_DIMENSION:
        if calculated_width > 0 and calculated_height > 0:
            scale = MIN_DIMENSION / min(calculated_width, calculated_height)
            calculated_width = int(calculated_width * scale)
            calculated_height = int(calculated_height * scale)
        else:
            calculated_width, calculated_height = MIN_DIMENSION, MIN_DIMENSION

    return max(1, calculated_width), max(1, calculated_height)


def _fix_axis_flips_by_src_dst_correlation(H_i2d, H_g2i, H_g2d, plane_quad, W, H):
    """Apply horizontal and vertical flip corrections if needed."""
    np = get_numpy()
    
    src_x = (H_g2i @ np.column_stack([plane_quad, np.ones(4)]).T)
    src_x = (src_x[0] / src_x[2]).ravel()
    dst_x = (H_g2d @ np.column_stack([plane_quad, np.ones(4)]).T)
    dst_x = (dst_x[0] / dst_x[2]).ravel()
    corr_x = _safe_corr(src_x, dst_x)
    if corr_x < 0:
        Fh = np.array([[-1,0,W-1],[0,1,0],[0,0,1]], dtype=np.float32)
        H_i2d = Fh @ H_i2d
        logger.info("Applied HORIZONTAL flip (corr_x=%.4f)", float(corr_x))

    src_y = (H_g2i @ np.column_stack([plane_quad, np.ones(4)]).T)
    src_y = (src_y[1] / src_y[2]).ravel()
    dst_y = (H_g2d @ np.column_stack([plane_quad, np.ones(4)]).T)
    dst_y = (dst_y[1] / dst_y[2]).ravel()
    corr_y = _safe_corr(src_y, dst_y)
    if corr_y < 0:
        Fv = np.array([[1,0,0],[0,-1,H-1],[0,0,1]], dtype=np.float32)
        H_i2d = Fv @ H_i2d
        logger.info("Applied VERTICAL flip (corr_y=%.4f)", float(corr_y))
    return H_i2d
