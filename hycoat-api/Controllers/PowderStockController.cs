using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Services.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/powder-stock")]
[Authorize]
public class PowderStockController : ControllerBase
{
    private readonly IPowderStockService _service;

    public PowderStockController(IPowderStockService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PowderStockDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<PowderStockDto>>.Ok(result));
    }

    [HttpGet("{powderColorId}")]
    public async Task<ActionResult<ApiResponse<PowderStockDto>>> GetByPowderColorId(int powderColorId)
    {
        var result = await _service.GetByPowderColorIdAsync(powderColorId);
        return Ok(ApiResponse<PowderStockDto>.Ok(result));
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Purchase,PPC,Admin,Leader")]
    public async Task<ActionResult<ApiResponse<List<PowderStockDto>>>> GetLowStock()
    {
        var result = await _service.GetLowStockAsync();
        return Ok(ApiResponse<List<PowderStockDto>>.Ok(result));
    }

    [HttpPut("{powderColorId}/reorder-level")]
    [Authorize(Roles = "Purchase,Admin")]
    public async Task<ActionResult<ApiResponse<PowderStockDto>>> UpdateReorderLevel(
        int powderColorId, UpdateReorderLevelDto dto)
    {
        var result = await _service.UpdateReorderLevelAsync(powderColorId, dto);
        return Ok(ApiResponse<PowderStockDto>.Ok(result, "Reorder level updated successfully."));
    }
}
