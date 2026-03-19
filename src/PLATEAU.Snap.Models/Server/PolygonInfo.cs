using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models.Extensions.Geometries;
using System.Numerics;

namespace PLATEAU.Snap.Models.Server;

public class PolygonInfo
{
    //private Polygon? _projectionPolygon = null;

    public int Id { get; set; }

    public string Gmlid { get; set; }

    public Polygon Polygon { get; set; }

    public Polygon PlanePolygon { get; set; }

    public Plane Plane { get; set; }

    public Vector3 FirstVector => PlanePolygon.Coordinates.First().ToVector3();

    //public Polygon ProjectionPolygon
    //{
    //    get
    //    {
    //        return _projectionPolygon != null ? _projectionPolygon : throw new InvalidOperationException();
    //    }
    //    set
    //    {
    //        _projectionPolygon = value;
    //    }
    //}

    public Vector3[] Vectors => PlanePolygon.Coordinates.Select(c => c.ToVector3()).ToArray();

    public PolygonInfo(int id, string gmlId, Polygon polygon, Polygon planePolygon)
    {
        Id = id;
        Gmlid = gmlId;
        Polygon = polygon;
        PlanePolygon = planePolygon;

        Plane = CreatePlaneFromPolygon(planePolygon);
    }

    /// <summary>
    /// ポリゴンから平面を計算します。
    /// 最も面積の大きい三角形を見つけて、その平面を返します。
    /// </summary>
    /// <param name="polygon">平面を計算するポリゴン</param>
    /// <param name="areaEpsilon">面積の最小閾値の2乗(デフォルト: 1e-6、約0.001平方メートルの三角形に相当)</param>
    /// <param name="maxVertices">パフォーマンス最適化のための最大頂点数(デフォルト: 50)</param>
    /// <param name="sufficientAreaThreshold">早期終了のための十分な面積の閾値(デフォルト: 1.0)</param>
    /// <returns>計算された平面</returns>
    /// <remarks>
    /// このメソッドは O(n³) の計算量を持ちますが、以下の最適化により実用的なパフォーマンスを実現します:
    /// - 大きなポリゴン(50頂点以上)の場合、最初の50頂点のみを使用
    /// - 十分な面積の三角形が見つかった時点で早期終了
    /// 建物ポリゴンは通常4-20頂点程度なので、ほとんどの場合で高速に動作します。
    /// 
    /// 座標系: 平面直角座標系(メートル単位)を想定しています。
    /// areaEpsilonは外積の大きさの2乗と比較されます。デフォルト値1e-6は、
    /// 外積の大きさ約0.001(三角形の面積約0.0005平方メートル)に相当します。
    /// </remarks>
    private static Plane CreatePlaneFromPolygon(
        Polygon polygon, 
        float areaEpsilon = 1e-6f,
        int maxVertices = 50,
        float sufficientAreaThreshold = 1.0f)
    {
        // 頂点を Vector3 に変換（重複座標はここではあえて残しておいてOK）
        var points = polygon.Coordinates
            .Select(c => c.ToVector3())
            .ToArray();

        if (points.Length < 3)
        {
            throw new InvalidOperationException("At least three points are required to define a plane.");
        }

        // パフォーマンス最適化: 大きなポリゴンの場合は最初のN頂点のみを使用
        var searchLimit = Math.Min(points.Length, maxVertices);

        bool found = false;
        bool searchComplete = false;
        float bestArea2 = 0f;      // 外積の長さの2乗（= 三角形の面積の2乗に比例）
        Plane bestPlane = default;

        for (int i = 0; i < searchLimit - 2 && !searchComplete; i++)
        {
            for (int j = i + 1; j < searchLimit - 1 && !searchComplete; j++)
            {
                var p0 = points[i];
                var p1 = points[j];

                var v1 = p1 - p0;
                if (v1.LengthSquared() < float.Epsilon)
                {
                    continue; // 同一点はスキップ
                }

                for (int k = j + 1; k < searchLimit && !searchComplete; k++)
                {
                    var p2 = points[k];
                    var v2 = p2 - p0;
                    if (v2.LengthSquared() < float.Epsilon)
                    {
                        continue;
                    }

                    var cross = Vector3.Cross(v1, v2);
                    var area2 = cross.LengthSquared(); // 面積の2乗に相当

                    // 一番「面積が大きい三角形」を採用
                    if (area2 > bestArea2)
                    {
                        bestArea2 = area2;
                        bestPlane = Plane.CreateFromVertices(p0, p1, p2);
                        found = true;

                        // 早期終了: 十分に大きな三角形が見つかった場合は探索を打ち切る
                        if (bestArea2 > sufficientAreaThreshold)
                        {
                            searchComplete = true;
                        }
                    }
                }
            }
        }
        // 全部ほぼ共線だった場合のチェック
        if (!found || bestArea2 < areaEpsilon)
        {
            throw new InvalidOperationException("All vertices of the polygon lie on (or very close to) the same straight line, so a plane cannot be defined.");
        }

        return bestPlane;
    }
}
