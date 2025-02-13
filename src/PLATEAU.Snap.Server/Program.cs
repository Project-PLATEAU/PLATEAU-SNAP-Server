using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using PLATEAU.Snap.Server;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Extensions.DependencyInjection;
using PLATEAU.Snap.Server.Geoid;
using PLATEAU.Snap.Server.Repositories;
using PLATEAU.Snap.Server.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// Configuration
var configuration = builder.Configuration;
if (!isDevelopment)
{
    var secretName = Environment.GetEnvironmentVariable("SECRET_NAME");
    ArgumentNullException.ThrowIfNull(secretName);
    Console.WriteLine($"Secret Name: {secretName}");

    var response = await new AmazonSecretsManagerClient().GetSecretValueAsync(new GetSecretValueRequest() { SecretId = secretName });
    Console.WriteLine($"GetSecretValueResponse: {response.HttpStatusCode}");
    var dic = JsonSerializer.Deserialize<Dictionary<string, string?>>(response.SecretString);
    configuration.AddInMemoryCollection(dic);
}
else
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
    configuration.GetValue<string>("Geoid:Path");
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
Console.WriteLine($"GridInfo: {grid.GridInfo}");

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(grid);
builder.Services.AddSingleton(appSettings);
builder.Services.UseDefaultServices();
builder.Services.UsePostgreSQLRepositories();
builder.Services.UseS3Repositories();
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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
    options.UseNpgsql(builder.ConnectionString, o => o.UseNetTopologySuite());
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
    if (!isDevelopment)
    {
        //options.EnableSensitiveDataLogging();
    }
});
builder.Services.AddSingleton(s3Settings);

// Exception Handler
builder.Services.AddProblemDetails();
builder.Services.AddHttpLogging(o => o = new HttpLoggingOptions());

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

// Logging
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
