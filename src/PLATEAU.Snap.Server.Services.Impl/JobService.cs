using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Services;

internal class JobService : IJobService
{
    private readonly IJobRepository jobRepository;

    private readonly IStorageRepository storageRepository;

    public JobService(IJobRepository jobRepository, IStorageRepository storageRepository)
    {
        this.jobRepository = jobRepository;
        this.storageRepository = storageRepository;
    }

    public async Task<Job> GetByIdAsync(long jobId)
    {
        var job = await this.jobRepository.GetAsync(jobId);
        if (job == null)
        {
            throw new NotFoundException($"Job with ID {jobId} does not exist.");
        }

        return job.ToClientModelResolvePath(storageRepository.GeneratePreSignedURLAsync);
    }
}
