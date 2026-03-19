using Microsoft.EntityFrameworkCore.Storage;
using PLATEAU.Snap.Server.Entities;

namespace PLATEAU.Snap.Server.Repositories;

internal class BaseRepository
{
    private readonly CitydbV4DbContext dbContext;

    protected CitydbV4DbContext Context => dbContext;

    public BaseRepository(CitydbV4DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await this.dbContext.Database.BeginTransactionAsync();
    }
}
