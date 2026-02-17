using Infinity.Domain.Entities;

namespace Infinity.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task UpsertAsync(Product product);
    Task UpsertBatchAsync(IEnumerable<Product> products);
}