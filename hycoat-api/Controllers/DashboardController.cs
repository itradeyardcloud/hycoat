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
    private static class DashboardGroups
    {
        public const string All = "Dashboard.All";
        public const string Admin = "Dashboard.Admin";
        public const string Leader = "Dashboard.Leader";
        public const string Sales = "Dashboard.Sales";
        public const string PPC = "Dashboard.PPC";
        public const string Production = "Dashboard.Production";
        public const string Quality = "Dashboard.Quality";
        public const string SCM = "Dashboard.SCM";
        public const string Purchase = "Dashboard.Purchase";
        public const string Finance = "Dashboard.Finance";
    }

    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("admin")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetAdmin(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin" }, new[] { DashboardGroups.All, DashboardGroups.Admin }))
        {
            return Forbid();
        }

        var result = await _service.GetAdminDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
    }

    [HttpGet("leader")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetLeader(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader" }, new[] { DashboardGroups.All, DashboardGroups.Leader }))
        {
            return Forbid();
        }

        var department = User.GetDepartment();
        var result = await _service.GetLeaderDashboardAsync(dateFrom, dateTo, period, department);
        return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
    }

    [HttpGet("sales")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SalesDashboardDto>>> GetSales(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.Sales }))
        {
            return Forbid();
        }

        var result = await _service.GetSalesDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<SalesDashboardDto>.Ok(result));
    }

    [HttpGet("ppc")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PPCDashboardDto>>> GetPPC(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.PPC }))
        {
            return Forbid();
        }

        var result = await _service.GetPPCDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<PPCDashboardDto>.Ok(result));
    }

    [HttpGet("production")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductionDashboardDto>>> GetProduction(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.Production }))
        {
            return Forbid();
        }

        var result = await _service.GetProductionDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<ProductionDashboardDto>.Ok(result));
    }

    [HttpGet("quality")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<QualityDashboardDto>>> GetQuality(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.Quality }))
        {
            return Forbid();
        }

        var result = await _service.GetQualityDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<QualityDashboardDto>.Ok(result));
    }

    [HttpGet("scm")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SCMDashboardDto>>> GetSCM(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.SCM }))
        {
            return Forbid();
        }

        var result = await _service.GetSCMDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<SCMDashboardDto>.Ok(result));
    }

    [HttpGet("purchase")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PurchaseDashboardDto>>> GetPurchase(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.Purchase }))
        {
            return Forbid();
        }

        var result = await _service.GetPurchaseDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<PurchaseDashboardDto>.Ok(result));
    }

    [HttpGet("finance")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<FinanceDashboardDto>>> GetFinance(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? period)
    {
        if (!HasAccess(new[] { "Admin", "Leader", "User" }, new[] { DashboardGroups.All, DashboardGroups.Finance }))
        {
            return Forbid();
        }

        var result = await _service.GetFinanceDashboardAsync(dateFrom, dateTo, period);
        return Ok(ApiResponse<FinanceDashboardDto>.Ok(result));
    }

    private bool HasAccess(IEnumerable<string> roles, IEnumerable<string> groups)
    {
        var userRole = User.GetRole();
        var hasRoleAccess = userRole is not null
            && roles.Any(r => string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase));

        return hasRoleAccess || User.IsInAnyGroup(groups);
    }
}
