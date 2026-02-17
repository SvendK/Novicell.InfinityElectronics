using Infinity.Domain.Entities;
using Infinity.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infinity.Infrastructure.Integration;

public class ErpSyncService
{
    private readonly HttpClient _http;
    private readonly IProductRepository _repo;
    private readonly ILogger<ErpSyncService> _logger;

    public ErpSyncService(HttpClient http, IProductRepository repo, ILogger<ErpSyncService> logger)
    {
        _http = http;
        _repo = repo;
        _logger = logger;
    }

    public async Task SyncProductsAsync(string url, CancellationToken ct)
    {
        _logger.LogInformation("Fetching products from {Url}", url);

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(ct);

        var products = JsonSerializer.DeserializeAsyncEnumerable<Product>(
            stream, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, 
            ct);

        var batch = new List<Product>();
        int count = 0;

        await foreach (var product in products)
        {
            if (product == null) continue;

            // Mark as active and updated
            product.LastUpdatedUtc = DateTime.UtcNow;
            product.IsActive = true;

            batch.Add(product);
            count++;

            // Commit in chunks of 100
            if (batch.Count >= 100)
            {
                await _repo.UpsertBatchAsync(batch);
                batch.Clear();
                _logger.LogDebug("Synced {Count} products so far...", count);
            }
        }

        if (batch.Count > 0)
        {
            await _repo.UpsertBatchAsync(batch);
        }

        _logger.LogInformation("Product sync complete. Total: {Count}", count);
    }
}