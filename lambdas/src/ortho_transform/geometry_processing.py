from typing import List, Tuple, TYPE_CHECKING
from ..shared.lazy_imports import get_numpy, get_shapely_wkt

from ..shared.logger import get_logger
logger = get_logger(__name__)

if TYPE_CHECKING:
    import numpy as np

# Coordinate
Coord = Tuple[float, float]
CoordList = List[Coord]

# Geometry
Dimensions = Tuple[int, int]


def wkt_polygon_to_coords(wkt_string: str) -> CoordList:
    """Parse WKT polygon string to extract coordinates."""
    wkt = get_shapely_wkt()
    polygon = wkt.loads(wkt_string)
    coords = list(polygon.exterior.coords)
    return coords[:-1]  # Remove duplicate closing coordinate


def _compute_axis_aligned_size_2d(plane_geometry_pts: CoordList) -> Tuple[float, float]:
    """
    Calculate dimensions using percentiles. This ignores outliers
    """
    np = get_numpy()
    
    if not plane_geometry_pts:
        return 0.0, 0.0

    try:
        coords = np.array(plane_geometry_pts)
        # 2nd and 98th percentiles instead of min/max to clip outliers
        xs = coords[:, 0]
        ys = coords[:, 1]
        x_min, x_max = np.percentile(xs, [2, 98])
        y_min, y_max = np.percentile(ys, [2, 98])
        if x_max - x_min < 1e-4: x_min, x_max = np.min(xs), np.max(xs)
        if y_max - y_min < 1e-4: y_min, y_max = np.min(ys), np.max(ys)

        width = x_max - x_min
        height = y_max - y_min
        return (width, height) if width > 0 and height > 0 else (0.0, 0.0)

    except (IndexError, TypeError, ValueError) as e:
        logger.warning(f"Failed to parse geometry: {e}")
        return 0.0, 0.0


def _geometry_to_y_down_plane_coords(geometry_pts: CoordList) -> List[List[float]]:
    """Process geometry coordinates, handling 3D projection if needed."""
    np = get_numpy()
    
    if len(geometry_pts) > 0 and len(geometry_pts[0]) >= 3:
        points_3d = np.array(geometry_pts, dtype=np.float32)
        normalized_coords = _project_facade_3d_to_2d(points_3d)
        max_y = np.max(normalized_coords[:, 1])
        geometry = [[pt[0], max_y - pt[1]] for pt in normalized_coords]
    else:
        geometry_coords = np.array(geometry_pts)
        max_y = geometry_coords[:, 1].max()
        geometry = [[point[0], max_y - point[1]] for point in geometry_coords]
    return geometry


def _project_facade_3d_to_2d(points_3d: "np.ndarray") -> "np.ndarray":
    """
    Projects 3D facade points onto a 2D plane.
    Uses SVD to find the best-fit plane, then projects using:
    - World Z as vertical reference (for vertical facades)
    - Primary variance direction for horizontal axis (4+ points)
    - Edge-based heuristics for triangles (3 points)
    """
    np = get_numpy()
    
    points_3d = np.asarray(points_3d, dtype=np.float64)
    n_points = len(points_3d)
    if n_points < 3:
        logger.warning("Need at least 3 points for 3D projection, got %d", n_points)
        if n_points == 0:
            return np.zeros((0, 2), dtype=np.float32)
        # Fallback for lines/points: just drop the Z axis
        result = points_3d[:, [0, 2]] if points_3d.shape[1] >= 3 else points_3d[:, :2]
        return (result - result.min(axis=0)).astype(np.float32)
    centroid = np.mean(points_3d, axis=0)
    centered = points_3d - centroid
    try:
        _, s, vh = np.linalg.svd(centered, full_matrices=False)
    except np.linalg.LinAlgError:
        logger.error("SVD failed in 3D projection")
        return np.zeros((n_points, 2), dtype=np.float32)
    # Sanity checks for degenerate geometry (points all in one spot, or a straight line)
    tolerance = 1e-6 * s[0] if s[0] > 0 else 1e-6
    if s[0] < tolerance:
        logger.warning("All 3D points are coincident")
        return np.zeros((n_points, 2), dtype=np.float32)
    if s[1] < tolerance:
        logger.warning("3D points are collinear, projecting onto line")
        line_dir = vh[0]
        t_values = centered @ line_dir
        t_values -= t_values.min()
        return np.column_stack([t_values, np.zeros(n_points)]).astype(np.float32)
    normal = vh[2]  # Smallest variance direction
    # Heuristic: Is this a wall or a floor/roof?
    z_coords = points_3d[:, 2]
    height_variation = z_coords.max() - z_coords.min()
    vertical_component = abs(normal[2])
    is_vertical_facade = height_variation > 1.0 and vertical_component < 0.7
    logger.debug(
        "Facade analysis: height_var=%.2f, normal_z=%.3f, is_vertical=%s, n_points=%d",
        height_variation, vertical_component, is_vertical_facade, n_points
    )
    if is_vertical_facade:
        # Use world Z as vertical
        v_axis = np.array([0.0, 0.0, 1.0])
        if n_points == 3:
            u_axis = _find_horizontal_axis_from_triangle(points_3d, normal)
        else:
            u_axis = _find_horizontal_axis_from_variance(vh, normal)
        # Force u_axis to be perfectly perpendicular to Z.
        u_axis = u_axis - np.dot(u_axis, v_axis) * v_axis
        u_norm = np.linalg.norm(u_axis)
        if u_norm > 1e-9:
            u_axis = u_axis / u_norm
        else:
            u_axis = np.cross(v_axis, normal)
            u_axis = u_axis / np.linalg.norm(u_axis)
    else:
        # use pca directly
        u_axis = vh[0]
        v_axis = vh[1]
        if np.dot(np.cross(u_axis, v_axis), normal) < 0:
            u_axis = -u_axis
    projected_2d = np.column_stack([
        centered @ u_axis,
        centered @ v_axis
    ])
    min_vals = projected_2d.min(axis=0)
    projected_2d -= min_vals
    logger.debug(
        "3D→2D projection: normal=%s, u_axis=%s, v_axis=%s",
        np.round(normal, 4), np.round(u_axis, 4), np.round(v_axis, 4)
    )
    logger.debug(
        "Projected 2D bounds: x=[%.2f, %.2f], y=[%.2f, %.2f]",
        projected_2d[:, 0].min(), projected_2d[:, 0].max(),
        projected_2d[:, 1].min(), projected_2d[:, 1].max()
    )
    return projected_2d.astype(np.float32)


def _find_horizontal_axis_from_triangle(points_3d: "np.ndarray", normal: "np.ndarray") -> "np.ndarray":
    """
    For a triangle, find the horizontal axis by identifying the most horizontal edge.
    This handles cases like gable ends where two points share the same Z (base)
    and one point is at a different Z (apex).
    """
    np = get_numpy()
    
    n = len(points_3d)
    best_edge = None
    best_horizontalness = -1.0
    for i in range(n):
        j = (i + 1) % n
        edge = points_3d[j] - points_3d[i]
        edge_len = np.linalg.norm(edge)
        if edge_len < 1e-6:
            continue
        edge_normalized = edge / edge_len
        # We want edges that are long AND flat (low Z-component).
        horizontalness = 1.0 - abs(edge_normalized[2])
        score = horizontalness * edge_len
        if score > best_horizontalness:
            best_horizontalness = score
            best_edge = edge_normalized
    if best_edge is None:
        # Fallback: use cross product of normal with Z
        fallback = np.cross(np.array([0, 0, 1]), normal)
        norm = np.linalg.norm(fallback)
        if norm > 1e-9:
            return fallback / norm
        return np.array([1.0, 0.0, 0.0])
    # Project the best edge onto XY plane to get pure horizontal
    horizontal = np.array([best_edge[0], best_edge[1], 0.0])
    h_norm = np.linalg.norm(horizontal)
    if h_norm < 1e-6:
        # Edge was purely vertical. Rotate 90 deg in XY plane.
        perp = np.array([-best_edge[1], best_edge[0], 0.0])
        p_norm = np.linalg.norm(perp)
        if p_norm > 1e-9:
            return perp / p_norm
        return np.array([1.0, 0.0, 0.0])
    return horizontal / h_norm


def _find_horizontal_axis_from_variance(vh: "np.ndarray", normal: "np.ndarray") -> "np.ndarray":
    """
    Finds horizontal axis for 4+ points.
    Why this exists: For tall facades, the primary variance is vertical (height).
    We need to check if the primary axis is actually horizontal before using it.
    """
    np = get_numpy()
    
    primary_direction = vh[0]
    horizontal = np.array([primary_direction[0], primary_direction[1], 0.0])
    h_norm = np.linalg.norm(horizontal)
    # If primary direction is vertical (like a tower), switch to secondary direction (width)
    if h_norm < 1e-6:
        secondary_direction = vh[1]
        horizontal = np.array([secondary_direction[0], secondary_direction[1], 0.0])
        h_norm = np.linalg.norm(horizontal)
        if h_norm < 1e-6:
            fallback = np.cross(np.array([0, 0, 1]), normal)
            f_norm = np.linalg.norm(fallback)
            if f_norm > 1e-9:
                return fallback / f_norm
            return np.array([1.0, 0.0, 0.0])
    return horizontal / h_norm


def _apply_homography_to_points(points: CoordList, matrix: "np.ndarray") -> CoordList:
    """Transform a list of points using the given transformation matrix."""
    np = get_numpy()
    
    points_array = np.array(points, dtype=np.float32)
    ones = np.ones((len(points_array), 1), dtype=np.float32)
    homogeneous_points = np.hstack([points_array, ones])
    transformed_homogeneous = matrix @ homogeneous_points.T
    w = transformed_homogeneous[2]
    w[np.abs(w) < 1e-9] = 1e-9
    transformed_points = (transformed_homogeneous[:2] / w).T
    return [(float(x), float(y)) for x, y in transformed_points]


def _safe_corr(a: "np.ndarray", b: "np.ndarray") -> float:
    """Pearson corr; returns 0 if either is (near) constant to avoid NaNs."""
    np = get_numpy()
    
    a = np.asarray(a, dtype=np.float32).ravel()
    b = np.asarray(b, dtype=np.float32).ravel()
    if a.size != b.size or a.size < 2:
        return 0.0
    if a.std() < 1e-6 or b.std() < 1e-6:
        return 0.0
    return float(np.corrcoef(a, b)[0, 1])
