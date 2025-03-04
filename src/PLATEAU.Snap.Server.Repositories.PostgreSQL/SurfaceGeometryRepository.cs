using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models.Extensions.Geometries;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Entities;
using System.Data;

namespace PLATEAU.Snap.Server.Repositories;

internal class SurfaceGeometryRepository : BaseRepository, ISurfaceGeometryRepository
{
    public SurfaceGeometryRepository(CitydbV4DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<PolygonInfo>> GetPolygonInfoAsync(VisibleSurfacesRequest request, int srid)
    {
        // 一定距離にあり、カメラの視野角内に入る面を平面直角に変換して取得
        // CityGML importerでインポートすると経度緯度が逆に登録されるようなので、ST_FlipCoordinates で入れ替え
        // カメラからの距離をメートル単位で指定するために平面直角に変換している (このためにsridの指定が必要になる)
        // カメラの視野角に入る面のみ絞り込む
        // カメラからの距離を計算して近い順にソート
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        // 事前に surface_centroid に lod1 の surface のみ登録しているため、このクエリで lod1 の面を取得できる
        using var command = connection.CreateCommand();
        command.CommandText = @"
            WITH t1 AS (
              SELECT
                sg.id,
                sg.gmlid,
                ST_Transform(ST_FlipCoordinates(geometry), 4326) AS geom,
                ST_Transform(ST_FlipCoordinates(geometry), @srid) AS plane_geom,
                sc.center,
                ST_Distance(center::geography, ST_SetSRID(ST_GeomFromText(@from_point_2d), 4326)::geography) AS distance
              FROM surface_centroid AS sc
              JOIN surface_geometry AS sg ON sc.id=sg.id
              WHERE ST_Distance(center::geography, ST_SetSRID(ST_GeomFromText(@from_point_2d), 4326)::geography) BETWEEN @min_distance AND @max_distance
            ),
            t2 AS (
              SELECT
                id,
                gmlid,
                geom,
                plane_geom,
                distance,
                abs(
                  degrees(ST_Azimuth(ST_SetSRID(ST_GeomFromText(@from_point_2d), 4326), center)) -
                  degrees(ST_Azimuth(ST_SetSRID(ST_GeomFromText(@from_point_2d), 4326), ST_SetSRID(ST_GeomFromText(@to_point_2d), 4326)))
                ) AS degrees
              FROM t1
            )
            SELECT id, gmlid, geom, plane_geom FROM t2
            WHERE degrees <= @field_of_view
            ORDER BY distance";
        command.Parameters.Add(command.CreateParameter("@srid", srid));
        command.Parameters.Add(command.CreateParameter("@from_point_2d", request.From.ToWkt2D()));
        command.Parameters.Add(command.CreateParameter("@to_point_2d", request.To.ToWkt2D()));
        command.Parameters.Add(command.CreateParameter("@min_distance", request.MinDistance));
        command.Parameters.Add(command.CreateParameter("@max_distance", request.MaxDistance));
        command.Parameters.Add(command.CreateParameter("@field_of_view", request.FieldOfView / 2));

        var list = new List<PolygonInfo>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);
            var gmlId = reader.GetString(1);
            var polygon = await reader.GetFieldValueAsync<Polygon>(2);
            var planePolygon = await reader.GetFieldValueAsync<Polygon>(3);
            list.Add(new PolygonInfo(id, gmlId, polygon, planePolygon));
        }

        return list;
    }

    public async Task<CameraInfo> GetCameraInfoAsync(VisibleSurfacesRequest request, int srid)
    {
        // カメラのfrom-toを平面直角に変換して取得
        // ここの変換はローカルでもできるが、DBでやることで一貫性を保っている
        // 検証して同じ結果が得られるならローカルでやってもいい
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
              ST_Transform(ST_SetSRID(ST_GeomFromText(@from), 6697), @srid), 
              ST_Transform(ST_SetSRID(ST_GeomFromText(@to), 6697), @srid)";
        command.Parameters.Add(command.CreateParameter("@srid", srid));
        command.Parameters.Add(command.CreateParameter("@from", request.From.ToWkt()));
        command.Parameters.Add(command.CreateParameter("@to", request.To.ToWkt()));

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var from = await reader.GetFieldValueAsync<Point>(0);
        var to = await reader.GetFieldValueAsync<Point>(1);

        return new CameraInfo(from.Coordinate, to.Coordinate);
    }
}
