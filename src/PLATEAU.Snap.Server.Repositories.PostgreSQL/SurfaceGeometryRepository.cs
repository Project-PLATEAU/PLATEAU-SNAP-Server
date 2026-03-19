using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Extensions.Geometries;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Entities.Models;
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

        // 事前に surface_centroid に lod2 の surface のみ登録しているため、このクエリで lod2 の面を取得できる
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
        command.Parameters.Add(command.CreateParameter("srid", srid));
        command.Parameters.Add(command.CreateParameter("from_point_2d", request.From.ToWkt2D()));
        command.Parameters.Add(command.CreateParameter("to_point_2d", request.To.ToWkt2D()));
        command.Parameters.Add(command.CreateParameter("min_distance", request.MinDistance));
        command.Parameters.Add(command.CreateParameter("max_distance", request.MaxDistance));
        command.Parameters.Add(command.CreateParameter("field_of_view", request.FieldOfView / 2));

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
        command.Parameters.Add(command.CreateParameter("srid", srid));
        command.Parameters.Add(command.CreateParameter("from", request.From.ToWkt()));
        command.Parameters.Add(command.CreateParameter("to", request.To.ToWkt()));

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var from = await reader.GetFieldValueAsync<Point>(0);
        var to = await reader.GetFieldValueAsync<Point>(1);

        return new CameraInfo(from.Coordinate, to.Coordinate);
    }

    public async Task<PageList<BuildingImage>> GetBuildingsAsync(SortType sortType, int pageNumber, int pageSize)
    {
        var query = Context.Database.SqlQueryRaw<BuildingImage>(@"
            SELECT
              DISTINCT ON (building_id)
              building_id AS id,
              gmlid,
              thumbnail AS thumbnail_bytes,
              CASE WHEN ken_name IS NOT NULL THEN ken_name ELSE '' END ||
              CASE WHEN sityo_name IS NOT NULL THEN sityo_name ELSE '' END ||
              CASE WHEN gst_name IS NOT NULL THEN gst_name ELSE '' END ||
              CASE WHEN css_name IS NOT NULL THEN css_name ELSE '' END ||
              CASE WHEN moji IS NOT NULL THEN moji ELSE '' END AS address
            FROM surface_images
            LEFT OUTER JOIN town_boundary AS tb ON ST_Intersects(center, tb.geom)");

        query = sortType switch
        {
            SortType.id_asc => query.OrderBy(x => x.Id),
            SortType.id_desc => query.OrderByDescending(x => x.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
        };

        return await PageList<BuildingImage>.ToPageListAsync(query, pageNumber, pageSize);
    }

    public async Task<PageList<BuildingFace>> GetFacesAsync(int buildingId, SortType sortType, int pageNumber, int pageSize)
    {
        var param = new NpgsqlParameter("buildingId", buildingId);
        var query = Context.Database.SqlQueryRaw<BuildingFace>(@"
            WITH ranked AS (
              SELECT
                bf.*,
                ROW_NUMBER() OVER (
                  PARTITION BY building_id, face_id
                  ORDER BY timestamp DESC NULLS LAST
                ) AS rn
              FROM building_faces bf
            ),
            t AS(
              SELECT * FROM ranked WHERE rn = 1
            )
            SELECT building_id, face_id, image_id, gmlid, is_ortho, thumbnail, coordinates, timestamp FROM t
            WHERE building_id = @buildingId", param);

        query = sortType switch
        {
            SortType.id_asc => query.OrderBy(x => x.IsOrtho).ThenBy(x => x.FaceId),
            SortType.id_desc => query.OrderBy(x => x.IsOrtho).ThenByDescending(x => x.FaceId),
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
        };

        return await PageList<BuildingFace>.ToPageListAsync(query, pageNumber, pageSize);
    }

    public async Task<PageList<BuildingFace>> GetFaceImagesAsync(int buildingId, int faceId, SortType sortType, int pageNumber, int pageSize)
    {
        var query = Context.BuildingFaces.Where(x => x.BuildingId == buildingId && x.FaceId == faceId);
        query = sortType switch
        {
            SortType.id_asc => query.OrderBy(x => x.ImageId),
            SortType.id_desc => query.OrderByDescending(x => x.ImageId),
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
        };

        return await PageList<BuildingFace>.ToPageListAsync(query, pageNumber, pageSize);
    }

    public async Task<string?> GetFaceWktAsync(int faceId)
    {
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ST_AsText(ST_Transform(ST_FlipCoordinates(g.geometry), 6668 + cast(system_number as integer))) AS geom FROM surface_geometry AS g
            JOIN city_boundary AS sn ON ST_Within(ST_Centroid(ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326)), sn.geom)
            WHERE id =@id";
        command.Parameters.Add(command.CreateParameter("id", faceId));

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() && reader["geom"] != DBNull.Value ? reader["geom"].ToString() : null;
    }

    public async Task<SurfaceImage?> GetSurfaceImageAsync(int buildingId, int faceId, long imageId)
    {
        return await Context.SurfaceImages
            .Where(x => x.BuildingId == buildingId && x.FaceId == faceId && x.ImageId == imageId)
            .FirstOrDefaultAsync();
    }

    public async Task<RoofSurface?> GetRoofSurfaceAsync(int buildingId, int faceId)
            {
        return await Context.RoofSurfaces
            .Where(x => x.BuildingId == buildingId && x.FaceId == faceId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(int buildingId)
    {
        return await Context.SurfaceImages.AnyAsync(x => x.BuildingId == buildingId);
    }

    public async Task<bool> ExistsAsync(int buildingId, int faceId)
    {
        return await Context.SurfaceImages.Where(x => x.BuildingId == buildingId && x.FaceId == faceId).AnyAsync();
    }

    public async Task<Geometry?> GetEnvelopeGeometryAsync(int buildingId)
    {
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = @"
            WITH extent AS(
              SELECT ST_3DExtent(ST_Transform(ST_FlipCoordinates(geometry), 4326)) AS box FROM surface_geometry
              WHERE id IN (
                SELECT id FROM surface_centroid WHERE building_id = @buildingId
              )
            )
            SELECT ST_MakeLine(ARRAY[
              ST_MakePoint(xmin, ymin, zmin),
              ST_MakePoint(xmax, ymax, zmax)
            ]) AS geom
            FROM (
              SELECT
                ST_XMin(box) AS xmin, ST_YMin(box) AS ymin, ST_ZMin(box) AS zmin,
                ST_XMax(box) AS xmax, ST_YMax(box) AS ymax, ST_ZMax(box) AS zmax
              FROM extent
            )";
        command.Parameters.Add(command.CreateParameter("buildingId", buildingId));

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? await reader.GetFieldValueAsync<Geometry>(0) : null;
    }

    public async Task<Geometry?> GetRoofprintAsync(int buildingId)
    {
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326) AS roofprint FROM building AS b
            JOIN surface_geometry AS sg ON b.lod0_roofprint_id=sg.root_id
            WHERE b.id=@buildingId AND parent_id IS NOT NULL;";
        command.Parameters.Add(command.CreateParameter("buildingId", buildingId));

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? await reader.GetFieldValueAsync<Geometry>(0) : null;
    }

    public async Task<List<int>> GetIntersectionsAsync(Polygon envelope)
    {
        envelope.SRID = 4326;
        var param = new NpgsqlParameter("envelope", envelope)
        {
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Geometry
        };

        var query = Context.Database.SqlQueryRaw<int>(@"
            WITH t AS(
              SELECT b.id AS building_id,ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326) AS roofprint FROM building AS b
              JOIN surface_geometry AS sg ON b.lod0_roofprint_id=sg.root_id
              WHERE parent_id IS NOT NULL
            )
            SELECT building_id FROM t
            WHERE ST_Intersects(@envelope, roofprint);", param);

        return await query.ToListAsync();
    }

    public async Task<List<GeometryInfo>> GetNotContainsAsync(Polygon envelope, int[] includeIds)
    {
        var idListParam = new NpgsqlParameter("idList", includeIds)
        {
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer
        };
        envelope.SRID = 4326;
        var envParam = new NpgsqlParameter("envelope", envelope)
        {
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Geometry
        };

        var query = Context.Database.SqlQueryRaw<GeometryInfo>(@"
            WITH t AS(
              SELECT b.id AS building_id,ST_Transform(ST_FlipCoordinates(ST_Force2D(geometry)), 4326) AS roofprint FROM building AS b
              JOIN surface_geometry AS sg ON b.lod0_roofprint_id=sg.root_id
              WHERE parent_id IS NOT NULL
            )
            SELECT building_id AS id,roofprint AS geom FROM t
            WHERE building_id = ANY (@idList) AND NOT ST_Contains(@envelope, roofprint);", idListParam, envParam);

        return await query.ToListAsync();
    }
}
