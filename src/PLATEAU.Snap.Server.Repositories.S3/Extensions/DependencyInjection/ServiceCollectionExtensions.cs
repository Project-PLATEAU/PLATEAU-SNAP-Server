using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PLATEAU.Snap.Server.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseS3Repositories(this IServiceCollection services, ConfigurationManager configuration)
    {
        return services
            .AddScoped<IStorageRepository, StorageRepository>()
            .AddDefaultAWSOptions(configuration.GetAWSOptions())
            .AddAWSService<IAmazonS3>();
    }
}
