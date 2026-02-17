using Infinity.Domain.Interfaces;
using Infinity.Infrastructure.Integration;

namespace Infinity.WorkerService;

public class SyncWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SyncWorker> _logger;
    private readonly IConfiguration _config;

    public SyncWorker(IServiceProvider services, ILogger<SyncWorker> logger, IConfiguration config)
    {
        _services = services;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            using (var scope = _services.CreateScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

                // Distributed Lock: Ensure only one worker runs the sync
                if (await cache.AcquireLockAsync("lock:sync-job", TimeSpan.FromMinutes(5)))
                {
                    try
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<ErpSyncService>();
                        var url = _config["ErpSettings:ProductEndpoint"];
                        await syncService.SyncProductsAsync(url!, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during sync");
                    }
                    finally
                    {
                        await cache.ReleaseLockAsync("lock:sync-job");
                    }
                }
                else
                {
                    _logger.LogInformation("Another worker is running the job. Skipping.");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
