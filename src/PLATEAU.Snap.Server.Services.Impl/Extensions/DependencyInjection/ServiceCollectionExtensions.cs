using Amazon.Lambda;
using Microsoft.Extensions.DependencyInjection;

namespace PLATEAU.Snap.Server.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseDefaultServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IAppService, AppService>()
            .AddScoped<IImporterExporterService, ImporterExporterService>()
            .AddScoped<IInvokerService, InvokerService>()
            .AddScoped<IJobService, JobService>()
            .AddScoped<ITextureService, TextureService>()
            .AddAWSService<IAmazonLambda>();
    }

    public static IServiceCollection UseImporterExporter(this IServiceCollection services)
    {
        return services
            .AddScoped<IImporterExporterService, ImporterExporterService>();
    }
}
