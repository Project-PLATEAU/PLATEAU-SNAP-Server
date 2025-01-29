using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Repositories;

public interface ICityBoundaryRepository
{
    Task<int> GetSrid(Coordinate coordinate);
}
