using Microsoft.AspNetCore.Mvc;
using Task9.Models.DTOs;
using Task9.Services;

namespace Task9.Controllers;

[Route(("api/[controller]"))]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] ProductDTO productDto)
    {
        var product = await _warehouseService.AddProduct(productDto);

        switch (product)
        {
            case -1:
                return NotFound("Taki produkt nie istnieje!");
            case -2:
                return NotFound("Taki magazyn nie istnieje!");
            case -3:
                return BadRequest("Taki produkt nie jest zamówiony lub jest go za mało!");
            case -4:
                return BadRequest("Zamówienie już jest dodane!");
            case -5:
                return BadRequest("err");
            default:
                return Ok(product);
        }
    }

    [HttpPost("procedure")]
    public async Task<IActionResult> AddProductWithProcedure([FromBody] ProductDTO productDto)
    {
        var product = await _warehouseService.AddProductProcedure(productDto);


        return Ok(product);
    }
}