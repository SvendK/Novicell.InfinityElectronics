using Infinity.Domain.Interfaces;
using Infinity.Infrastructure.Data;
using Infinity.Infrastructure.Integration;
using Infinity.Infrastructure.Repositories;
using Infinity.Infrastructure.Services;
using Infinity.WorkerService;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// 1. Logging (Serilog)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// 2. Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// 4. Domain & Infrastructure
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddHttpClient<ErpSyncService>(); // Registers typed client

// 5. The Worker Service
builder.Services.AddHostedService<SyncWorker>();

var host = builder.Build();

// Ensure DB is created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Wait for SQL Server to be ready in Docker
    await Task.Delay(5000);
    db.Database.Migrate();
}

host.Run();