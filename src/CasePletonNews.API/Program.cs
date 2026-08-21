using CasePletonNews.API.Clients;
using CasePletonNews.API.Endpoints;
using CasePletonNews.API.Services;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Config Refit
builder.Services
    .AddRefitClient<IHackerNewsApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
        c.Timeout = TimeSpan.FromSeconds(10);
    });

// Config MemoryCache
builder.Services.AddMemoryCache();

// Inject HackerNewsService
builder.Services.AddScoped<HackerNewsService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints
StoriesEndpoint.Map(app);

app.Run();