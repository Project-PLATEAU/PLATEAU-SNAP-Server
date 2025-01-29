using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PLATEAU.Snap.Server;
using PLATEAU.Snap.Server.Entities;
using PLATEAU.Snap.Server.Repositories;
using PLATEAU.Snap.Server.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
ArgumentNullException.ThrowIfNull(databaseSettings);
var s3Settings = configuration.GetSection("S3").Get<S3Settings>();
ArgumentNullException.ThrowIfNull(s3Settings);

// Add services to the container.
builder.Services.UseDefaultServices();
builder.Services.UsePostgreSQLRepositories();
builder.Services.UseS3Repositories(configuration);
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddDbContext<CitydbV4DbContext>(options =>
{
    var builder = new NpgsqlConnectionStringBuilder();
    builder.Host = databaseSettings.Host;
    builder.Port = databaseSettings.Port;
    builder.Username = databaseSettings.Username;
    builder.Password = databaseSettings.Password;
    builder.Database = databaseSettings.Database;
    options.UseNpgsql(builder.ConnectionString);
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
    //options.EnableSensitiveDataLogging();
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
});

// Logging
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
