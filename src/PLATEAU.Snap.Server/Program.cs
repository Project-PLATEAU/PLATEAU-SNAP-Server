using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using PLATEAU.Snap.Models.Json;
using PLATEAU.Snap.Models.Settings;
using PLATEAU.Snap.Server;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Extensions.DependencyInjection;
using PLATEAU.Snap.Server.Filters;
using PLATEAU.Snap.Server.Geoid;
using PLATEAU.Snap.Server.Middleware;
using PLATEAU.Snap.Server.Repositories;
using PLATEAU.Snap.Server.Services;
using Serilog;
using Serilog.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// Configuration
var configuration = builder.Configuration;
if (isDevelopment)
{
    var awsOptions = configuration.GetAWSOptions();
    Console.WriteLine($"Profile: {awsOptions.Profile}, Region: {awsOptions.Region.DisplayName}");
    builder.Services.AddDefaultAWSOptions(awsOptions);
}

var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
ArgumentNullException.ThrowIfNull(databaseSettings);
var s3Settings = configuration.GetSection("S3").Get<S3Settings>();
ArgumentNullException.ThrowIfNull(s3Settings);
var appSettings = configuration.GetSection("App").Get<AppSettings>();
ArgumentNullException.ThrowIfNull(appSettings);

// Geoid
Grid grid;
if (!isDevelopment)
{
    using var response = await new AmazonS3Client().GetObjectAsync(new GetObjectRequest { BucketName = s3Settings.Bucket, Key = "gsigeo2011_ver2_2.asc" });
    using var geoidoReader = new GeoidReader(response.ResponseStream);
    grid = geoidoReader.Read();
}
else
{
#pragma warning disable CS8604
    // Development:  read gsigeo2011_ver2_2 in the same path as the executable.
    var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "gsigeo2011_ver2_2.asc");
#pragma warning restore CS8604
    using var geoidoReader = new GeoidReader(path);
    grid = geoidoReader.Read();
}

// Lambda
var lambdaSettings = new LambdaSettings()
{
    TransformFunctionName = configuration.GetValue<string>("TransformFunctionName") ?? throw new ArgumentNullException("TransformFunctionName"),
    RoofExtractionFunctionName = configuration.GetValue<string>("RoofExtractionFunctionName") ?? throw new ArgumentNullException("RoofExtractionFunctionName"),
    ApplyTextureFunctionName = configuration.GetValue<string>("ApplyTextureFunctionName") ?? throw new ArgumentNullException("ApplyTextureFunctionName"),
    ExportBuildingFunctionName = configuration.GetValue<string>("ExportBuildingFunctionName") ?? throw new ArgumentNullException("ExportBuildingFunctionName"),
    ExportMeshFunctionName = configuration.GetValue<string>("ExportMeshFunctionName") ?? throw new ArgumentNullException("ExportMeshFunctionName"),
};

// logging
var logfilePath = builder.Configuration.GetValue<string>("LogFilePath");
var logLevel = builder.Configuration.GetValue<string>("LogLevel") ?? "Information";
var enableRequestResponseLogging = builder.Configuration.GetValue<bool>("EnableRequestResponseLogging");
var logEventLevel = Enum.TryParse(logLevel, true, out LogEventLevel level) ? level : LogEventLevel.Information;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(logEventLevel);
if (!string.IsNullOrEmpty(logfilePath))
{
    loggerConfiguration = loggerConfiguration.WriteTo.File(
        logfilePath,
        logEventLevel,
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        retainedFileCountLimit: 3,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        encoding: System.Text.Encoding.UTF8
    );
}
Log.Logger = loggerConfiguration.CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(grid);
builder.Services.AddSingleton(appSettings);
builder.Services.AddSingleton(databaseSettings);
builder.Services.UseDefaultServices();
builder.Services.UsePostgreSQLRepositories();
builder.Services.UseS3Repositories();
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add<ApiExceptionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new JobParamConverter());
    options.JsonSerializerOptions.Converters.Add(new JobResultParamConverter());
});
builder.Services.AddAuthentication(Constants.ApiAuthentication.AuthenticationScheme)
    .AddApiKeyAuthentication();
builder.Services.AddDbContext<CitydbV4DbContext>(options =>
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
builder.Services.AddSingleton(s3Settings);
builder.Services.AddSingleton(lambdaSettings);

// Exception Handler
builder.Services.AddProblemDetails();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    c.AddSecurityDefinition(Constants.ApiAuthentication.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = $"Api key Authorization header",
        Name = Constants.ApiAuthentication.ApiKeyHeader,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = Constants.ApiAuthentication.ApiKeyHeader,
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = Constants.ApiAuthentication.AuthenticationScheme
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
 {
     options.AddDefaultPolicy(builder =>
     {
         builder.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader();
     });
 });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (isDevelopment || builder.Configuration.GetValue<bool>("UseSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (enableRequestResponseLogging)
{
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
