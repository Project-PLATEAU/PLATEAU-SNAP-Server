using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

namespace PLATEAU.Snap.Server.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseS3Repositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IStorageRepository, StorageRepository>()
            .AddAWSService<IAmazonS3>();
    }
}
