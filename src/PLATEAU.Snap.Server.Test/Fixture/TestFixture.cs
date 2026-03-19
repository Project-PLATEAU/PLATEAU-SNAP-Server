using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PLATEAU.Snap.Server.Authentication;
using PLATEAU.Snap.Server.Controllers;
using PLATEAU.Snap.Server.Filters;
using PLATEAU.Snap.Server.Services;
using PLATEAU.Snap.Server.Test.Fakes.Services;
using System.Text.Encodings.Web;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PLATEAU.Snap.Server.Test.Fixture;

public class TestFixture
{
    public IServiceProvider ServiceProvider { get; private set; }

    public TestFixture()
    {
        var services = new ServiceCollection();

        // controller
        services
            .AddScoped<TexturesController>();

        // logging
        var outputHelper = new TestOutputHelper();
        services.AddSingleton<ITestOutputHelper>(outputHelper);
        services.AddLogging(builder => builder.AddXUnit(outputHelper));

        // filter
        services.AddScoped<ApiExceptionFilter>();

        // fake services
        services
            .AddScoped<IImporterExporterService, FakeImporterExporterService>()
            .AddScoped<ITextureService, FakeTextureService>()
            .AddScoped<IInvokerService, FakeInvokerService>();

        // auth handler
        var httpContext = new DefaultHttpContext();
        var optionsMonitor = new OptionsMonitor<AuthenticationSchemeOptions>(
            new OptionsFactory<AuthenticationSchemeOptions>(
                Enumerable.Empty<IConfigureOptions<AuthenticationSchemeOptions>>(),
                Enumerable.Empty<IPostConfigureOptions<AuthenticationSchemeOptions>>()),
            Enumerable.Empty<IOptionsChangeTokenSource<AuthenticationSchemeOptions>>(),
            new OptionsCache<AuthenticationSchemeOptions>());

        var authHandler = new ApiKeyAuthHandler(optionsMonitor, new LoggerFactory(), UrlEncoder.Default, new Models.Settings.AppSettings() { ApiKey = "123"});
        services
            .AddSingleton(authHandler);

        this.ServiceProvider = services.BuildServiceProvider();
    }

    public T GetRequiredService<T>() where T : ControllerBase
    {
        var controller = this.ServiceProvider.GetRequiredService<T>();
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.Request.Headers["X-API-KEY"] = "123";

        return controller;
    }
}
