using Infinity.Application.Services;
using Infinity.Domain.Interfaces;
using Infinity.Infrastructure.Data;
using Infinity.Infrastructure.Repositories;
using Infinity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Domain Services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

// Simple API Key Middleware for Integration Endpoints
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/integration"))
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedKey) ||
            extractedKey != builder.Configuration["ApiSettings:ApiKey"])
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Missing or Invalid API Key");
            return;
        }
    }
    await next();
});

app.MapControllers();

app.Run();