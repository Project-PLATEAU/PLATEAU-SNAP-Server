using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Services;

internal static class MeshUtil
{
    /// <summary>
    /// 浮動小数点数の比較における許容誤差
    /// </summary>
    private const double AreaComparisonTolerance = 1e-10;

    /// <summary>
    /// 3次メッシュコードから Envelope を返します。
    /// </summary>
    /// <param name="meshCode">3次メッシュコード（8桁）</param>
    /// <returns>3次メッシュコードのEnvelope</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Envelope GetEnvelopeFromThirdMeshCode(string meshCode)
    {
        if (meshCode is null || meshCode.Length != 8)
        {
            throw new ArgumentException("3次メッシュコードは8桁で指定してください。", nameof(meshCode));
        }

        // 1次メッシュ
        int p = int.Parse(meshCode.Substring(0, 2)); // 緯度部分
        int a = int.Parse(meshCode.Substring(2, 2)); // 経度部分

        // 2次メッシュ
        int q = int.Parse(meshCode.Substring(4, 1)); // 南北方向
        int r = int.Parse(meshCode.Substring(5, 1)); // 東西方向

        // 3次メッシュ
        int s = int.Parse(meshCode.Substring(6, 1)); // 南北方向
        int t = int.Parse(meshCode.Substring(7, 1)); // 東西方向

        // 緯度の計算
        double lat = p / 1.5; // 1次メッシュの南端緯度
        lat += q * (5.0 / 60.0); // 2次メッシュ分
        lat += s * (30.0 / 3600.0); // 3次メッシュ分

        // 経度の計算
        double lon = a + 100; // 1次メッシュの西端経度
        lon += r * (7.5 / 60.0); // 2次メッシュ分
        lon += t * (45.0 / 3600.0); // 3次メッシュ分

        // 3次メッシュのサイズ
        double latSize = 30.0 / 3600.0;
        double lonSize = 45.0 / 3600.0;

        return new Envelope(lon, lon + lonSize, lat, lat + latSize);
    }

    /// <summary>
    /// ジオメトリを受け取って3次メッシュコードを返します。
    /// ジオメトリの座標は経度, 緯度の順でX, Yに入っています。
    /// ジオメトリがメッシュの境界に位置している場合、以下の仕様でメッシュを決定します：
    /// 1. ジオメトリをメッシュで分割したときに面積が最も大きいメッシュ
    /// 2. 面積が同じ場合はメッシュコードが小さい方を選択
    /// </summary>
    /// <param name="geometry">ジオメトリ（経度, 緯度の順でX, Yに格納）</param>
    /// <returns>3次メッシュコード（8桁）</returns>
    public static string GetThirdMeshCode(Geometry geometry)
    {
        if (geometry == null)
        {
            throw new ArgumentNullException(nameof(geometry));
        }

        // ジオメトリの重心を計算
        var centroid = geometry.Centroid;
        var longitude = centroid.X;
        var latitude = centroid.Y;

        // 基本の3次メッシュコードを取得
        var baseMeshCode = CalculateThirdMeshCode(longitude, latitude);

        // ジオメトリが複数のメッシュにまたがる可能性がある場合の処理
        var candidateMeshCodes = GetCandidateMeshCodes(geometry);

        if (candidateMeshCodes.Count <= 1)
        {
            return baseMeshCode;
        }

        // 各メッシュでジオメトリと重複する部分の面積を計算
        var meshAreas = new Dictionary<string, double>();

        foreach (var meshCode in candidateMeshCodes)
        {
            var meshPolygon = CreateMeshPolygon(meshCode);
            var intersection = geometry.Intersection(meshPolygon);

            if (intersection != null && !intersection.IsEmpty)
            {
                meshAreas[meshCode] = intersection.Area;
            }
        }

        if (meshAreas.Count == 0)
        {
            return baseMeshCode;
        }

        // 面積が最大のメッシュを選択
        var maxArea = meshAreas.Values.Max();
        var maxAreaMeshCodes = meshAreas.Where(kvp => Math.Abs(kvp.Value - maxArea) < AreaComparisonTolerance)
                                      .Select(kvp => kvp.Key)
                                      .ToList();

        // 面積が同じ場合はメッシュコードが小さい方を選択
        return maxAreaMeshCodes.Min()!;
    }

    /// <summary>
    /// 経度・緯度から3次メッシュコードを計算します
    /// </summary>
    private static string CalculateThirdMeshCode(double longitude, double latitude)
    {
        // 1次メッシュ（20万分の1地勢図）の計算
        // 緯度方向: 40分（2/3度）間隔、経度方向: 1度間隔
        int latCode1 = (int)Math.Floor(latitude * 1.5); // 緯度×1.5の整数部
        int lonCode1 = (int)Math.Floor(longitude) - 100; // 経度-100の整数部

        // 1次メッシュコード（4桁）
        string firstMesh = $"{latCode1:D2}{lonCode1:D2}";

        // 2次メッシュ（2万5千分の1地勢図）の計算
        // 1次メッシュを縦横8等分
        double latRemainder1 = latitude * 1.5 - latCode1;
        double lonRemainder1 = longitude - (lonCode1 + 100);

        int latCode2 = (int)Math.Floor(latRemainder1 * 8);
        int lonCode2 = (int)Math.Floor(lonRemainder1 * 8);

        // 2次メッシュコード（6桁）
        string secondMesh = $"{firstMesh}{latCode2}{lonCode2}";

        // 3次メッシュ（基準地域メッシュ）の計算
        // 2次メッシュを縦横10等分
        double latRemainder2 = (latRemainder1 * 8) - latCode2;
        double lonRemainder2 = (lonRemainder1 * 8) - lonCode2;

        int latCode3 = (int)Math.Floor(latRemainder2 * 10);
        int lonCode3 = (int)Math.Floor(lonRemainder2 * 10);

        // 3次メッシュコード（8桁）
        string thirdMesh = $"{secondMesh}{latCode3}{lonCode3}";

        return thirdMesh;
    }

    /// <summary>
    /// ジオメトリに関連する候補メッシュコードを取得します
    /// </summary>
    private static List<string> GetCandidateMeshCodes(Geometry polygon)
    {
        var envelope = polygon.EnvelopeInternal;
        var meshCodes = new HashSet<string>();

        // エンベロープの各頂点とその周辺のメッシュコードを取得
        var points = new[]
        {
            new { X = envelope.MinX, Y = envelope.MinY },
            new { X = envelope.MaxX, Y = envelope.MinY },
            new { X = envelope.MinX, Y = envelope.MaxY },
            new { X = envelope.MaxX, Y = envelope.MaxY },
            new { X = (envelope.MinX + envelope.MaxX) / 2, Y = (envelope.MinY + envelope.MaxY) / 2 }
        };

        foreach (var point in points)
        {
            meshCodes.Add(CalculateThirdMeshCode(point.X, point.Y));

            // 周辺のメッシュも確認（境界付近の場合）
            var delta = 0.00001; // 約1m程度のオフセット
            meshCodes.Add(CalculateThirdMeshCode(point.X - delta, point.Y - delta));
            meshCodes.Add(CalculateThirdMeshCode(point.X + delta, point.Y - delta));
            meshCodes.Add(CalculateThirdMeshCode(point.X - delta, point.Y + delta));
            meshCodes.Add(CalculateThirdMeshCode(point.X + delta, point.Y + delta));
        }

        return meshCodes.ToList();
    }

    /// <summary>
    /// 3次メッシュコードからメッシュ範囲のポリゴンを作成します
    /// </summary>
    private static Polygon CreateMeshPolygon(string meshCode)
    {
        if (string.IsNullOrEmpty(meshCode) || meshCode.Length != 8)
        {
            throw new ArgumentException("Invalid mesh code format", nameof(meshCode));
        }

        // メッシュコードを分解
        int latCode1 = int.Parse(meshCode.Substring(0, 2));
        int lonCode1 = int.Parse(meshCode.Substring(2, 2));
        int latCode2 = int.Parse(meshCode.Substring(4, 1));
        int lonCode2 = int.Parse(meshCode.Substring(5, 1));
        int latCode3 = int.Parse(meshCode.Substring(6, 1));
        int lonCode3 = int.Parse(meshCode.Substring(7, 1));

        // 1次メッシュの基準点
        double baseLat = latCode1 / 1.5;
        double baseLon = lonCode1 + 100;

        // 2次メッシュのサイズ
        double lat2Size = (2.0 / 3.0) / 8.0; // 5分 = 1/12度
        double lon2Size = 1.0 / 8.0; // 7.5分 = 1/8度

        // 3次メッシュのサイズ
        double lat3Size = lat2Size / 10.0; // 30秒 = 1/120度
        double lon3Size = lon2Size / 10.0; // 45秒 = 1/80度

        // 3次メッシュの南西角
        double swLat = baseLat + latCode2 * lat2Size + latCode3 * lat3Size;
        double swLon = baseLon + lonCode2 * lon2Size + lonCode3 * lon3Size;

        // 3次メッシュの北東角
        double neLat = swLat + lat3Size;
        double neLon = swLon + lon3Size;

        // ポリゴンを作成
        var coordinates = new[]
        {
            new Coordinate(swLon, swLat),
            new Coordinate(neLon, swLat),
            new Coordinate(neLon, neLat),
            new Coordinate(swLon, neLat),
            new Coordinate(swLon, swLat)
        };

        var factory = new GeometryFactory();
        return factory.CreatePolygon(coordinates);
    }
}
