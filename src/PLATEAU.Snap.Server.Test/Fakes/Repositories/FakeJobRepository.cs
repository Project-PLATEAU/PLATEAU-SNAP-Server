using Microsoft.EntityFrameworkCore.Storage;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Server.Entities.Models;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Test.Fakes.Repositories;

internal class FakeJobRepository : IJobRepository
{
    public async Task<Job> AddAsync(Job entity)
    {
        await Task.CompletedTask;
        return entity;
    }

    public async Task<Job?> GetAsync(long jobId)
    {
        return await Task.FromResult(new Job
        {
            Id = jobId,
            Status = JobStatusType.completed.ToString(),
        });
    }

    public async Task<Job> UpdateAsync(Job entity)
    {
        await Task.CompletedTask;
        return entity;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        throw new NotImplementedException();
    }
}
