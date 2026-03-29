using HycoatApi.DTOs;
using HycoatApi.DTOs.Dashboard;
using HycoatApi.Helpers;
using HycoatApi.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetAdmin(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetAdminDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
    }

    [HttpGet("leader")]
    [Authorize(Roles = "Leader")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetLeader(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var department = User.GetDepartment();
        var result = await _service.GetLeaderDashboardAsync(dateFrom, dateTo, period, department);
        return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
    }

    [HttpGet("sales")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<SalesDashboardDto>>> GetSales(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetSalesDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<SalesDashboardDto>.Ok(result));
    }

    [HttpGet("ppc")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<PPCDashboardDto>>> GetPPC(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetPPCDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<PPCDashboardDto>.Ok(result));
    }

    [HttpGet("production")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<ProductionDashboardDto>>> GetProduction(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetProductionDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<ProductionDashboardDto>.Ok(result));
    }

    [HttpGet("quality")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<QualityDashboardDto>>> GetQuality(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetQualityDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<QualityDashboardDto>.Ok(result));
    }

    [HttpGet("scm")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<SCMDashboardDto>>> GetSCM(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetSCMDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<SCMDashboardDto>.Ok(result));
    }

    [HttpGet("purchase")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<PurchaseDashboardDto>>> GetPurchase(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetPurchaseDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<PurchaseDashboardDto>.Ok(result));
    }

    [HttpGet("finance")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<FinanceDashboardDto>>> GetFinance(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        var result = await _service.GetFinanceDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<FinanceDashboardDto>.Ok(result));
    }
}
