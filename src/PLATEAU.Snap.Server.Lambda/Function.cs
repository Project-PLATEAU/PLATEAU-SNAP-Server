using Amazon;
using Amazon.Lambda.Core;
using Amazon.RDS.Util;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Lambda;
using PLATEAU.Snap.Models.Settings;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Repositories;
using PLATEAU.Snap.Server.Services;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PLATEAU.Snap.Server.Lambda;

public class Function
{
    private static readonly ServiceProvider RootProvider = BuildServices();

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var isDevelopment = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                  ?? "Production").Equals("Development");

        if (!isDevelopment)
        {
            var secretName = config["SECRET_NAME"];
            ArgumentNullException.ThrowIfNull(secretName);

            var response = new AmazonSecretsManagerClient().GetSecretValueAsync(new GetSecretValueRequest() { SecretId = secretName }).Result;
            
            // 環境変数形式(Database__Port)を階層構造に変換
            var secretDict = JsonSerializer.Deserialize<Dictionary<string, string?>>(response.SecretString)!;
            var convertedDict = secretDict.ToDictionary(
                kvp => kvp.Key.Replace("__", ":"),
                kvp => kvp.Value
            );
            
            var secretConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(convertedDict)
                .Build();

            var databaseSettings = secretConfig.GetSection("Database").Get<DatabaseSettings>();
            ArgumentNullException.ThrowIfNull(databaseSettings);
            var s3Settings = secretConfig.GetSection("S3").Get<S3Settings>();
            ArgumentNullException.ThrowIfNull(s3Settings);
            var appSettings = secretConfig.GetSection("App").Get<AppSettings>();
            ArgumentNullException.ThrowIfNull(appSettings);

            services.AddDbContextFactory<CitydbV4DbContext>((sp, options) =>
            {
                var builder = new NpgsqlConnectionStringBuilder();
                builder.Host = databaseSettings.Host;
                builder.Port = databaseSettings.Port;
                builder.Username = databaseSettings.Username;
                builder.Password = databaseSettings.Password;
                builder.Database = databaseSettings.Database;
                builder.SslMode = SslMode.Require;
                options.UseNpgsql(builder.ConnectionString, o => o.UseNetTopologySuite());
                options.UseSnakeCaseNamingConvention();
            });

            services
                .AddSingleton<IConfiguration>(config)
                .AddSingleton(databaseSettings)
                .AddSingleton(s3Settings)
                .AddSingleton(appSettings);
        }
        else
        {

        }

        services
            .AddLogging(b => b.AddSimpleConsole())
            .UsePostgreSQLRepositoriesForLambda()
            .UseS3Repositories()
            .UseImporterExporter()
            .AddScoped<FunctionHandlerImpl>();

        return services.BuildServiceProvider();
    }

    public async Task<ExportBuildingResultParam> ExportBuilding(LambdaExportBuildingRequest input, ILambdaContext context)
    {
        using var scope = RootProvider.CreateScope();
        var impl = scope.ServiceProvider.GetRequiredService<FunctionHandlerImpl>();
        return await impl.ExportBuildingAsync(input, context);
    }

    public async Task<ExportMeshResultParam> ExportMesh(LambdaExportMeshRequest input, ILambdaContext context)
    {
        using var scope = RootProvider.CreateScope();
        var impl = scope.ServiceProvider.GetRequiredService<FunctionHandlerImpl>();
        return await impl.ExportMeshAsync(input, context);
    }
}
