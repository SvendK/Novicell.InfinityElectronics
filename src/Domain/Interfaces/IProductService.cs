using Infinity.Domain.Entities;

namespace Infinity.Domain.Interfaces;

public interface IProductService
{
    Task<Product?> GetProductAsync(string id);
    Task<IEnumerable<Product>> GetProductsAsync();
    Task ProcessProductWebhookAsync(Product product);
}
//public interface IProductService
//{
//    Task<Product?> GetProductAsync(string id);
//    Task<IEnumerable<Product>> GetProductsAsync();
//    Task ProcessProductUpdateAsync(Product product); // For Webhooks
//}
