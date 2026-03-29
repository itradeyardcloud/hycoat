using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Dashboard;
using HycoatApi.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    // ───────────────────────── Order Tracker ─────────────────────────

    public async Task<PagedResponse<OrderTrackerDto>> GetOrderTrackerAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search,
        int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = BuildOrderTrackerQuery(dateFrom, dateTo, customerId, search);
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToOrderTrackerDto).ToList();

        return new PagedResponse<OrderTrackerDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<OrderTrackerDto>> ExportOrderTrackerAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search)
    {
        var items = await BuildOrderTrackerQuery(dateFrom, dateTo, customerId, search)
            .OrderByDescending(w => w.Date)
            .ToListAsync();

        return items.Select(MapToOrderTrackerDto).ToList();
    }

    private IQueryable<Models.Sales.WorkOrder> BuildOrderTrackerQuery(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search)
    {
        var query = _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.MaterialInwards).ThenInclude(m => m.IncomingInspections)
            .Include(w => w.ProductionWorkOrders).ThenInclude(p => p.PretreatmentLogs)
            .Include(w => w.ProductionWorkOrders).ThenInclude(p => p.ProductionLogs)
            .Include(w => w.ProductionWorkOrders).ThenInclude(p => p.FinalInspections)
            .Include(w => w.DeliveryChallans)
            .Include(w => w.Invoices)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(w => w.Date >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(w => w.Date <= dateTo.Value.Date);
        if (customerId.HasValue)
            query = query.Where(w => w.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(w =>
                w.WONumber.ToLower().Contains(term) ||
                w.Customer.Name.ToLower().Contains(term));
        }

        return query;
    }

    private static OrderTrackerDto MapToOrderTrackerDto(Models.Sales.WorkOrder w)
    {
        // Check which stages have been completed
        var hasInquiry = w.ProformaInvoiceId.HasValue; // WO came through sales pipeline
        var hasQuotation = hasInquiry; // Quotation leads to PI
        var hasPI = w.ProformaInvoiceId.HasValue;
        var hasMaterial = w.MaterialInwards.Any();
        var hasIncomingInspection = w.MaterialInwards.Any(m => m.IncomingInspections.Any());
        var hasPWO = w.ProductionWorkOrders.Any();
        var hasPretreatment = w.ProductionWorkOrders.Any(p => p.PretreatmentLogs.Any());
        var hasCoating = w.ProductionWorkOrders.Any(p => p.ProductionLogs.Any());
        var hasFinalInspection = w.ProductionWorkOrders.Any(p => p.FinalInspections.Any());
        var hasDispatched = w.DeliveryChallans.Any();
        var hasInvoiced = w.Invoices.Any();

        // Count completed stages (11 total)
        var stages = new[] { hasInquiry, hasQuotation, hasPI, hasMaterial, hasIncomingInspection,
                             hasPWO, hasPretreatment, hasCoating, hasFinalInspection, hasDispatched, hasInvoiced };
        var completedStages = stages.Count(s => s);
        var completionPercent = (int)Math.Round(completedStages / 11.0 * 100);

        // Determine current stage (last completed)
        var currentStage = "Created";
        if (hasInvoiced) currentStage = "Invoiced";
        else if (hasDispatched) currentStage = "Dispatch";
        else if (hasFinalInspection) currentStage = "QA";
        else if (hasCoating) currentStage = "Coating";
        else if (hasPretreatment) currentStage = "Pretreatment";
        else if (hasPWO) currentStage = "Planning";
        else if (hasIncomingInspection) currentStage = "IncomingInspection";
        else if (hasMaterial) currentStage = "MaterialInward";
        else if (hasPI) currentStage = "PI";

        var daysInProcess = (DateTime.UtcNow.Date - w.Date.Date).Days;

        return new OrderTrackerDto
        {
            WorkOrderId = w.Id,
            WorkOrderNumber = w.WONumber,
            CustomerName = w.Customer.Name,
            OrderDate = w.Date,
            CurrentStage = currentStage,
            CompletionPercent = completionPercent,
            InquiryDone = hasInquiry,
            QuotationDone = hasQuotation,
            PIDone = hasPI,
            MaterialReceived = hasMaterial,
            IncomingInspectionDone = hasIncomingInspection,
            PWOCreated = hasPWO,
            PretreatmentDone = hasPretreatment,
            CoatingDone = hasCoating,
            FinalInspectionDone = hasFinalInspection,
            Dispatched = hasDispatched,
            Invoiced = hasInvoiced,
            DaysInProcess = daysInProcess
        };
    }

    // ───────────────────────── Production Throughput ─────────────────────────

    public async Task<ThroughputSummaryDto> GetProductionThroughputAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo);

        var query = _db.ProductionLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.LineItems)
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.PretreatmentLogs)
            .Where(l => l.Date >= from && l.Date <= to);

        if (customerId.HasValue)
            query = query.Where(l => l.ProductionWorkOrder.CustomerId == customerId.Value);

        var logs = await query.ToListAsync();

        var details = logs.Select(l => new ProductionThroughputDto
        {
            Date = l.Date,
            Shift = l.Shift,
            CustomerName = l.ProductionWorkOrder.Customer.Name,
            PWONumber = l.ProductionWorkOrder.PWONumber,
            AreaSFT = l.ProductionWorkOrder.LineItems.Sum(li => li.TotalSurfaceAreaSqft),
            PowderUsedKg = 0, // Powder usage not tracked per production log
            BasketsProcessed = l.ProductionWorkOrder.PretreatmentLogs
                .Count(pt => pt.Date == l.Date && pt.Shift == l.Shift)
        }).ToList();

        var dailyTrend = details
            .GroupBy(d => d.Date)
            .Select(g => new ChartPointDto
            {
                Label = g.Key.ToString("dd/MM"),
                Value = g.Sum(d => d.AreaSFT)
            })
            .OrderBy(c => c.Label)
            .ToList();

        return new ThroughputSummaryDto
        {
            TotalSFT = details.Sum(d => d.AreaSFT),
            TotalPowderKg = details.Sum(d => d.PowderUsedKg),
            TotalBaskets = details.Sum(d => d.BasketsProcessed),
            Details = details,
            DailyTrend = dailyTrend
        };
    }

    public async Task<List<ProductionThroughputDto>> ExportProductionThroughputAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId)
    {
        var summary = await GetProductionThroughputAsync(dateFrom, dateTo, customerId);
        return summary.Details;
    }

    // ───────────────────────── Powder Consumption ─────────────────────────

    public async Task<List<PowderConsumptionDto>> GetPowderConsumptionAsync(
        DateTime? dateFrom, DateTime? dateTo)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo);

        // Get all powder colors that have stock entries
        var stocks = await _db.PowderStocks
            .AsNoTracking()
            .Include(s => s.PowderColor)
            .ToListAsync();

        // Get ordered (from PO line items) in date range
        var ordered = await _db.POLineItems
            .AsNoTracking()
            .Where(l => l.PurchaseOrder.Date >= from && l.PurchaseOrder.Date <= to
                     && l.PurchaseOrder.Status != "Cancelled")
            .GroupBy(l => l.PowderColorId)
            .Select(g => new { PowderColorId = g.Key, TotalKg = g.Sum(l => l.QtyKg) })
            .ToListAsync();

        // Get received (from GRN line items) in date range
        var received = await _db.GRNLineItems
            .AsNoTracking()
            .Where(l => l.GoodsReceivedNote.Date >= from && l.GoodsReceivedNote.Date <= to)
            .GroupBy(l => l.PowderColorId)
            .Select(g => new { PowderColorId = g.Key, TotalKg = g.Sum(l => l.QtyReceivedKg) })
            .ToListAsync();

        return stocks.Select(s =>
        {
            var orderedKg = ordered.FirstOrDefault(o => o.PowderColorId == s.PowderColorId)?.TotalKg ?? 0;
            var receivedKg = received.FirstOrDefault(r => r.PowderColorId == s.PowderColorId)?.TotalKg ?? 0;
            // Consumed = received - current stock (approximation)
            var consumed = receivedKg > s.CurrentStockKg ? receivedKg - s.CurrentStockKg : 0;
            var wastage = receivedKg > 0 ? Math.Round((receivedKg - consumed - s.CurrentStockKg) / receivedKg * 100, 1) : 0;

            return new PowderConsumptionDto
            {
                PowderCode = s.PowderColor.PowderCode,
                ColorName = s.PowderColor.ColorName,
                OrderedKg = orderedKg,
                ReceivedKg = receivedKg,
                ConsumedKg = consumed,
                CurrentStockKg = s.CurrentStockKg,
                WastagePercent = Math.Max(0, wastage)
            };
        }).ToList();
    }

    // ───────────────────────── Quality Summary ─────────────────────────

    public async Task<QualitySummaryDto> GetQualitySummaryAsync(
        DateTime? dateFrom, DateTime? dateTo)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo);

        var finalInspections = await _db.FinalInspections
            .AsNoTracking()
            .Where(f => f.Date >= from && f.Date <= to)
            .ToListAsync();

        var totalInspections = finalInspections.Count;
        var passed = finalInspections.Count(f => f.OverallStatus == "Approved");
        var failed = finalInspections.Count(f => f.OverallStatus == "Rejected");
        var rework = finalInspections.Count(f => f.OverallStatus == "Rework");
        var passRate = totalInspections > 0 ? Math.Round((decimal)passed / totalInspections * 100, 1) : 0;

        // Average DFT from in-process readings
        var avgDFT = await _db.InProcessDFTReadings
            .Where(r => r.InProcessInspection.Date >= from && r.InProcessInspection.Date <= to
                     && r.AvgReading.HasValue)
            .AverageAsync(r => (decimal?)r.AvgReading) ?? 0;

        // DFT trend (daily avg)
        var dftTrend = (await _db.InProcessDFTReadings
            .Where(r => r.InProcessInspection.Date >= from && r.InProcessInspection.Date <= to
                     && r.AvgReading.HasValue)
            .GroupBy(r => r.InProcessInspection.Date)
            .Select(g => new { Date = g.Key, Avg = g.Average(r => r.AvgReading!.Value) })
            .OrderBy(c => c.Date)
            .ToListAsync())
            .Select(g => new ChartPointDto { Label = g.Date.ToString("dd/MM"), Value = g.Avg })
            .ToList();

        // Failure reasons (group by OverallStatus for non-Approved)
        var failureReasons = finalInspections
            .Where(f => f.OverallStatus != "Approved")
            .GroupBy(f => f.OverallStatus)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToList();

        return new QualitySummaryDto
        {
            TotalInspections = totalInspections,
            PassedCount = passed,
            FailedCount = failed,
            ReworkCount = rework,
            PassRate = passRate,
            AvgDFT = Math.Round(avgDFT, 1),
            DFTTrend = dftTrend,
            FailureReasons = failureReasons
        };
    }

    // ───────────────────────── Customer History ─────────────────────────

    public async Task<CustomerHistoryDto> GetCustomerHistoryAsync(int customerId)
    {
        var customer = await _db.Customers.FindAsync(customerId)
            ?? throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

        var workOrders = await _db.WorkOrders
            .AsNoTracking()
            .Where(w => w.CustomerId == customerId)
            .Include(w => w.Invoices)
            .Include(w => w.ProductionWorkOrders).ThenInclude(p => p.LineItems)
            .OrderByDescending(w => w.Date)
            .ToListAsync();

        var orders = workOrders.Select(w =>
        {
            var invoice = w.Invoices.FirstOrDefault(i => i.Status != "Cancelled");
            var areaSFT = w.ProductionWorkOrders
                .SelectMany(p => p.LineItems)
                .Sum(l => l.TotalSurfaceAreaSqft);

            return new CustomerOrderDto
            {
                WorkOrderNumber = w.WONumber,
                Date = w.Date,
                AreaSFT = areaSFT,
                InvoiceNumber = invoice?.InvoiceNumber,
                InvoiceAmount = invoice?.GrandTotal,
                Status = w.Status
            };
        }).ToList();

        return new CustomerHistoryDto
        {
            CustomerName = customer.Name,
            TotalOrders = orders.Count,
            TotalAreaSFT = orders.Sum(o => o.AreaSFT),
            TotalInvoicedAmount = orders.Sum(o => o.InvoiceAmount ?? 0),
            Orders = orders
        };
    }

    // ───────────────────────── Dispatch Register ─────────────────────────

    public async Task<PagedResponse<DispatchRegisterDto>> GetDispatchRegisterAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search,
        int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = BuildDispatchQuery(dateFrom, dateTo, customerId, search);
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => MapToDispatchRegisterDto(d))
            .ToListAsync();

        return new PagedResponse<DispatchRegisterDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<DispatchRegisterDto>> ExportDispatchRegisterAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search)
    {
        return await BuildDispatchQuery(dateFrom, dateTo, customerId, search)
            .OrderByDescending(d => d.Date)
            .Select(d => MapToDispatchRegisterDto(d))
            .ToListAsync();
    }

    private IQueryable<Models.Dispatch.DeliveryChallan> BuildDispatchQuery(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search)
    {
        var query = _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(d => d.Date >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(d => d.Date <= dateTo.Value.Date);
        if (customerId.HasValue)
            query = query.Where(d => d.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(d =>
                d.DCNumber.ToLower().Contains(term) ||
                d.Customer.Name.ToLower().Contains(term) ||
                d.WorkOrder.WONumber.ToLower().Contains(term));
        }

        return query;
    }

    private static DispatchRegisterDto MapToDispatchRegisterDto(Models.Dispatch.DeliveryChallan d)
    {
        return new DispatchRegisterDto
        {
            DCNumber = d.DCNumber,
            Date = d.Date,
            CustomerName = d.Customer.Name,
            WorkOrderNumber = d.WorkOrder.WONumber,
            TotalQuantity = d.LineItems.Sum(l => l.Quantity),
            VehicleNumber = d.VehicleNumber,
            Status = d.Status,
            InvoiceNumber = null // Invoice linked through WorkOrder, not directly on DC
        };
    }

    // ───────────────────────── Helpers ─────────────────────────

    private static (DateTime from, DateTime to) ResolveDateRange(DateTime? dateFrom, DateTime? dateTo)
    {
        var now = DateTime.UtcNow;
        var from = dateFrom?.Date ?? new DateTime(now.Year, now.Month, 1);
        var to = dateTo?.Date ?? now.Date;
        return (from, to);
    }
}
