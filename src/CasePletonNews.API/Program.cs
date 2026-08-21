using CasePletonNews.API.Clients;
using CasePletonNews.API.Endpoints;
using CasePletonNews.API.Services;
using CasePletonNews.API.Settings;
using Refit;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CasePletonNews API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .WriteTo.Console());

    builder.AddServiceDefaults();

    // Settings
    var hnSettings = builder.Configuration
        .GetSection(HackerNewsSettings.SectionName)
        .Get<HackerNewsSettings>()!;

    builder.Services.Configure<HackerNewsSettings>(
        builder.Configuration.GetSection(HackerNewsSettings.SectionName)
    );

    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen();

    // ProblemDetails
    builder.Services.AddProblemDetails();

    // Config Refit
    builder.Services
        .AddRefitClient<IHackerNewsApi>()
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(hnSettings.BaseUrl);
            c.Timeout = TimeSpan.FromSeconds(hnSettings.TimeoutSeconds);
        });

    // Config MemoryCache
    builder.Services.AddMemoryCache();

    // Inject HackerNewsService
    builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    // Serilog
    app.UseSerilogRequestLogging();

    StoriesEndpoint.Map(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}