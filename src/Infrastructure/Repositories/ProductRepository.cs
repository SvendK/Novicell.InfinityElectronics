using Infinity.Domain.Entities;
using Infinity.Domain.Interfaces;
using Infinity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infinity.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Product?> GetByIdAsync(string id)
        => await _db.Products.FindAsync(id);

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _db.Products.Where(p => p.IsActive).ToListAsync();

    public async Task UpsertAsync(Product product)
    {
        var existing = await _db.Products.FindAsync(product.Id);
        if (existing == null)
        {
            await _db.Products.AddAsync(product);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(product);
        }
        await _db.SaveChangesAsync();
    }

    public async Task UpsertBatchAsync(IEnumerable<Product> products)
    {
        foreach (var p in products)
        {
            var existing = await _db.Products.FindAsync(p.Id);
            if (existing == null)
                await _db.Products.AddAsync(p);
            else
                _db.Entry(existing).CurrentValues.SetValues(p);
        }
        await _db.SaveChangesAsync();
    }
}