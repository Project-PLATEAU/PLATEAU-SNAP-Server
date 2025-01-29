using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models.Extensions.Geometries;
using PLATEAU.Snap.Server.Entities;
using System.Data;

namespace PLATEAU.Snap.Server.Repositories;

internal class CityBoundaryRepository : BaseRepository, ICityBoundaryRepository
{
    public CityBoundaryRepository(CitydbV4DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<int> GetSrid(Coordinate coordinate)
    {
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT system_number FROM city_boundary
            WHERE ST_Within(ST_SetSrid(ST_GeomFromText(@point), 4326), geom)";
        command.Parameters.Add(command.CreateParameter("@point", coordinate.ToWkt2D()));

        var number = Convert.ToInt32(await command.ExecuteScalarAsync());
        if (number < 0 || number > 19)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "The system number is out of range.");
        }

        // I系(6669) ～ XIX系(6687)
        return 6668 + number;
    }
}
