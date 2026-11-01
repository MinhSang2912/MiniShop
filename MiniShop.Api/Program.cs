using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Memory;
using MiniShop.Api;
using MiniShop.Api.Health;
using MiniShop.Api.Jobs;
using MiniShop.Core.Interfaces;
using MiniShop.Core.Services;
using MiniShop.Infrastructure;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;

MongoMappings.Register();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("mongodb", tags: new[] { "ready" });

builder.Host.UseSerilog((context, services, config) => config
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/minishop-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14));

var useHangfire = !builder.Environment.IsEnvironment("Testing");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OrderExpirationService>();
builder.Services.AddScoped<ExpireOrdersJob>();

if (useHangfire)
{
    builder.Services.AddHangfire(c => c.UseInMemoryStorage());
    builder.Services.AddHangfireServer();
}
// Add services to the container.
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<OrderReports>();
builder.Services.AddScoped<IOrderReports>(sp =>
    new CachedOrderReports(
        sp.GetRequiredService<OrderReports>(),
        sp.GetRequiredService<IMemoryCache>()));
builder.Services.AddScoped<OrderService>();
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration["Mongo:ConnectionString"]));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration["Mongo:Database"]));
builder.Services.AddTransient<DataSeeder>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
    await MongoIndexes.EnsureAsync(scope.ServiceProvider.GetRequiredService<IMongoDatabase>());
}

if (useHangfire)
{
    app.UseHangfireDashboard("/hangfire");

    app.Services.GetRequiredService<IRecurringJobManager>()
        .AddOrUpdate<ExpireOrdersJob>("expire-pending-orders", j => j.RunAsync(), Cron.Hourly);
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllers();

app.Run();

public partial class Program { }