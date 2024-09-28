using IdentityServer;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Serilog;
using Log = Serilog.Log;
using IdentityServer.Models.Settings;
using Microsoft.Extensions.Options;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
           .AddEnvironmentVariables()
           .Build();

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    bool.TryParse(builder.Configuration.GetSection("OpenTelemetryEnable").Value, out bool openTelemetryEnable);
    if (openTelemetryEnable)
    {
        const string serviceName = "IdentityServerExample";

        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName))
                .AddConsoleExporter();
        });

        builder.Services.AddOpenTelemetry()
              .ConfigureResource(resource => resource.AddService(serviceName))
              .WithTracing(tracing => tracing
                  .AddAspNetCoreInstrumentation()
                  .AddHttpClientInstrumentation()
                  .AddEntityFrameworkCoreInstrumentation()
                  .AddSqlClientInstrumentation()
                  .AddConsoleExporter()
                  .AddZipkinExporter(opt => opt.Endpoint = new Uri("http://localhost:9411/api/v2/spans"))) //todo
              .WithMetrics(metrics => metrics
                  .AddAspNetCoreInstrumentation()
                  .AddConsoleExporter());
    }

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddAuthentication();

    var provider = builder.Services.BuildServiceProvider();
    var app = builder
            .ConfigureServices(builder.Configuration, provider.GetService<IOptions<AppSettings>>())
            .ConfigurePipeline(provider.GetService<IOptions<AppSettings>>());

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
