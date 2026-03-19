using NetTopologySuite.Geometries;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Test.Fakes.Repositories;

internal class FakeCityBoundaryRepository : ICityBoundaryRepository
{
    public Task<int> GetSrid(Coordinate coordinate)
    {
        throw new NotImplementedException();
    }
}
