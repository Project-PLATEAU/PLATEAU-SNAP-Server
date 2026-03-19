using PLATEAU.Snap.Server.Entities.Models;

namespace PLATEAU.Snap.Server.Repositories;

public interface IJobRepository : IBaseRepository
{
    Task<Job?> GetAsync(long jobId);

    Task<Job> AddAsync(Job entity);

    Task<Job> UpdateAsync(Job entity);
}
