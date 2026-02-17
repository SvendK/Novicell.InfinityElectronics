using Infinity.Domain.Entities;
using Infinity.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Infinity.Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationController : ControllerBase
{
    private readonly IProductService _service;

    public IntegrationController(IProductService service)
    {
        _service = service;
    }

    [HttpPost("webhook/product")]
    public async Task<IActionResult> ProductWebhook([FromBody] Product product)
    {
        // This endpoint simulates ERP calling us when a product changes
        await _service.ProcessProductWebhookAsync(product);
        return Ok(new { message = "Product processed and cache invalidated" });
    }
}
