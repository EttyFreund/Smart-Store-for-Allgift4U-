using Microsoft.AspNetCore.Mvc;
using SmartStore.BLL.Interfaces;
using System.Text.Json.Serialization;

namespace SmartStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("process-order")]
    public async Task<IActionResult> ProcessOrder([FromBody] OrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.OrderName))
            return BadRequest("Order name is required.");

        try
        {
            var result = await _inventoryService.ProcessIncomingOrder(request.OrderName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class OrderRequest
{
    public string OrderName { get; set; } = string.Empty;
}
