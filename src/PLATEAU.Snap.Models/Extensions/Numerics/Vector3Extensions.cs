using System.Numerics;

namespace PLATEAU.Snap.Models.Extensions.Numerics;

public static class Vector3Extensions
{
    public static double Angle(this Vector3 from, Vector3 to)
    {
        var dot = Vector3.Dot(from, to);
        var angle = Math.Acos(dot / (from.Length() * to.Length()));
        return angle;
    }

    public static double Degrees(this Vector3 from, Vector3 to)
    {
        return Angle(from, to) * 180 / Math.PI;
    }
}
