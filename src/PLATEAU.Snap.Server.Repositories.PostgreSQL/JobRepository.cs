using Microsoft.EntityFrameworkCore;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Entities.Models;

namespace PLATEAU.Snap.Server.Repositories;

internal class JobRepository : BaseRepository, IJobRepository
{
    public JobRepository(CitydbV4DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Job?> GetAsync(long jobId)
    {
        return await Context.Jobs.FirstOrDefaultAsync(x => x.Id == jobId);
    }

    public async Task<Job> AddAsync(Job entity)
    {
        await Context.Jobs.AddAsync(entity);
        await Context.SaveChangesAsync();

        return entity;
    }

    public async Task<Job> UpdateAsync(Job entity)
    {
        Context.Jobs.Update(entity);
        await Context.SaveChangesAsync();

        return entity;
    }
}
