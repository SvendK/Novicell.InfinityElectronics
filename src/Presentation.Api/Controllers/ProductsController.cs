using Infinity.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Infinity.Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var product = await _service.GetProductAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }
}
