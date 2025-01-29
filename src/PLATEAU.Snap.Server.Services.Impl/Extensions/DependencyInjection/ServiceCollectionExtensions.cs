using Microsoft.Extensions.DependencyInjection;

namespace PLATEAU.Snap.Server.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseDefaultServices(this IServiceCollection services)
    {
        return services
            .AddScoped<ISurfaceGeometryService, SurfaceGeometryService>()
            .AddScoped<IImageService, ImageService>();
    }
}
