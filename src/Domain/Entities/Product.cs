namespace Infinity.Domain.Entities;

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
    public bool IsActive { get; set; } = true;
}