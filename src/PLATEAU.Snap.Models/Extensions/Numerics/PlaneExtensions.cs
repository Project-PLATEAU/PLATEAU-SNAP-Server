using System.Numerics;

namespace PLATEAU.Snap.Models.Extensions.Numerics;

public static class PlaneExtensions
{
    public static Matrix4x4 GetTransformationMatrixToXYPlane(this Plane plane)
    {
        // 平面の法線ベクトル
        var normal = plane.Normal;

        // XY平面の法線ベクトル
        var xyNormal = Vector3.UnitZ;

        // 回転軸を計算
        var rotationAxis = Vector3.Cross(normal, xyNormal);
        var rotationAngle = Math.Acos(Vector3.Dot(normal, xyNormal));

        // 回転行列を計算
        var rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotationAxis, (float)rotationAngle);

        // 平面上の点を計算
        var pointOnPlane = -plane.D * normal;

        // 平面上の点をXY平面に投影
        var projectedPoint = new Vector3(pointOnPlane.X, pointOnPlane.Y, 0);

        // 平行移動行列を計算
        var translationMatrix = Matrix4x4.CreateTranslation(projectedPoint - pointOnPlane);

        // 変換行列を計算
        var transformationMatrix = rotationMatrix * translationMatrix;

        return transformationMatrix;
    }
}
