using PLATEAU.Snap.Models.Client;

namespace PLATEAU.Snap.Server.Services;

public interface IJobService
{
    Task<Job> GetByIdAsync(long jobId);
}
