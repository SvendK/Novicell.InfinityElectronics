using Infinity.Domain.Entities;
using Infinity.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infinity.Application.Services;

public class ProductService : IProductService
{
    private const int ProductCacheMinutes = 10;

    private readonly IProductRepository _repo;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repo, ICacheService cache, ILogger<ProductService> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Product?> GetProductAsync(string id)
    {
        // Cache-Aside Pattern
        var cacheKey = $"product:{id}";
        var cached = await _cache.GetAsync<Product>(cacheKey);
        if (cached != null) 
            return cached;

        var product = await _repo.GetByIdAsync(id);
        if (product != null)
            await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(ProductCacheMinutes));

        return product;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        // Simple implementation: Fetch all. 
        // In real world: Pagination + Caching list keys.
        return await _repo.GetAllAsync();
    }

    public async Task ProcessProductWebhookAsync(Product product)
    {
        _logger.LogInformation("Webhook received for Product {Id}", product.Id);

        product.LastUpdatedUtc = DateTime.UtcNow;
        product.IsActive = true;

        await _repo.UpsertAsync(product);

        // Invalidate cache immediately so next read is fresh
        await _cache.RemoveAsync($"product:{product.Id}");
    }
}