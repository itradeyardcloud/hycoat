using HycoatApi.DTOs;
using HycoatApi.DTOs.Reports;
using HycoatApi.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HycoatApi.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ExcelExportService _excelService;

    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ReportsController(IReportService reportService, ExcelExportService excelService)
    {
        _reportService = reportService;
        _excelService = excelService;
    }

    // ───────────────────────── Order Tracker ─────────────────────────

    [HttpGet("order-tracker")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<PagedResponse<OrderTrackerDto>>>> GetOrderTracker(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _reportService.GetOrderTrackerAsync(dateFrom, dateTo, customerId, search, page, pageSize);
        return Ok(ApiResponse<PagedResponse<OrderTrackerDto>>.Ok(result));
    }

    [HttpGet("order-tracker/export")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<IActionResult> ExportOrderTracker(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId,
        [FromQuery] string? search)
    {
        var data = await _reportService.ExportOrderTrackerAsync(dateFrom, dateTo, customerId, search);
        var bytes = _excelService.ExportToExcel(data, "OrderTracker");
        return File(bytes, XlsxContentType, "OrderTracker.xlsx");
    }

    // ───────────────────────── Production Throughput ─────────────────────────

    [HttpGet("production-throughput")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<ThroughputSummaryDto>>> GetProductionThroughput(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId)
    {
        var result = await _reportService.GetProductionThroughputAsync(dateFrom, dateTo, customerId);
        return Ok(ApiResponse<ThroughputSummaryDto>.Ok(result));
    }

    [HttpGet("production-throughput/export")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<IActionResult> ExportProductionThroughput(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId)
    {
        var data = await _reportService.ExportProductionThroughputAsync(dateFrom, dateTo, customerId);
        var bytes = _excelService.ExportToExcel(data, "ProductionThroughput");
        return File(bytes, XlsxContentType, "ProductionThroughput.xlsx");
    }

    // ───────────────────────── Powder Consumption ─────────────────────────

    [HttpGet("powder-consumption")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<List<PowderConsumptionDto>>>> GetPowderConsumption(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetPowderConsumptionAsync(dateFrom, dateTo);
        return Ok(ApiResponse<List<PowderConsumptionDto>>.Ok(result));
    }

    [HttpGet("powder-consumption/export")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<IActionResult> ExportPowderConsumption(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var data = await _reportService.GetPowderConsumptionAsync(dateFrom, dateTo);
        var bytes = _excelService.ExportToExcel(data, "PowderConsumption");
        return File(bytes, XlsxContentType, "PowderConsumption.xlsx");
    }

    // ───────────────────────── Quality Summary ─────────────────────────

    [HttpGet("quality-summary")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<QualitySummaryDto>>> GetQualitySummary(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetQualitySummaryAsync(dateFrom, dateTo);
        return Ok(ApiResponse<QualitySummaryDto>.Ok(result));
    }

    // ───────────────────────── Customer History ─────────────────────────

    [HttpGet("customer-history/{customerId}")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<CustomerHistoryDto>>> GetCustomerHistory(int customerId)
    {
        var result = await _reportService.GetCustomerHistoryAsync(customerId);
        return Ok(ApiResponse<CustomerHistoryDto>.Ok(result));
    }

    // ───────────────────────── Dispatch Register ─────────────────────────

    [HttpGet("dispatch-register")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<ActionResult<ApiResponse<PagedResponse<DispatchRegisterDto>>>> GetDispatchRegister(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _reportService.GetDispatchRegisterAsync(dateFrom, dateTo, customerId, search, page, pageSize);
        return Ok(ApiResponse<PagedResponse<DispatchRegisterDto>>.Ok(result));
    }

    [HttpGet("dispatch-register/export")]
    [Authorize(Roles = "Admin,Leader,User")]
    public async Task<IActionResult> ExportDispatchRegister(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int? customerId,
        [FromQuery] string? search)
    {
        var data = await _reportService.ExportDispatchRegisterAsync(dateFrom, dateTo, customerId, search);
        var bytes = _excelService.ExportToExcel(data, "DispatchRegister");
        return File(bytes, XlsxContentType, "DispatchRegister.xlsx");
    }
}
