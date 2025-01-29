using NetTopologySuite.Geometries;
using System.Numerics;

namespace PLATEAU.Snap.Models.Extensions.Geometries;

public static class CoordinateExtensions
{
    public static string ToWkt(this Coordinate coordinate)
    {
        return $"POINT Z ({coordinate.X} {coordinate.Y} {coordinate.Z})";
    }

    public static string ToWkt2D(this Coordinate coordinate)
    {
        return $"POINT ({coordinate.X} {coordinate.Y})";
    }

    public static Vector3 ToVector3(this Coordinate coordinate)
    {
        return new Vector3((float)coordinate.X, (float)coordinate.Y, (float)coordinate.Z);
    }
}
