using Microsoft.Extensions.DependencyInjection;

namespace PLATEAU.Snap.Server.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UsePostgreSQLRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<ISurfaceGeometryRepository, SurfaceGeometryRepository>()
            .AddScoped<IImageRepository, ImageRepository>()
            .AddScoped<ICityBoundaryRepository, CityBoundaryRepository>();
    }
}
