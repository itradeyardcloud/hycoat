using HycoatApi.Data;
using HycoatApi.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    // ───────────────────────── Admin Dashboard ─────────────────────────

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);

        var activeWOs = await _db.WorkOrders
            .CountAsync(w => w.Status != "Completed" && w.Status != "Cancelled");

        var pendingInquiries = await _db.Inquiries
            .CountAsync(i => i.Status == "New" || i.Status == "InProgress");

        var monthlyRevenue = await _db.Invoices
            .Where(i => i.Date >= from && i.Date <= to && i.Status != "Cancelled")
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

        var monthlyProductionSFT = await _db.PWOLineItems
            .Where(l => l.ProductionWorkOrder.ProductionLogs.Any(pl => pl.Date >= from && pl.Date <= to))
            .SumAsync(l => (decimal?)l.TotalSurfaceAreaSqft) ?? 0;

        var pendingDispatches = await _db.WorkOrders
            .CountAsync(w => w.Status == "Completed" && !w.DeliveryChallans.Any());

        // Quality pass rate: approved / total final inspections in period
        var totalFI = await _db.FinalInspections
            .CountAsync(f => f.Date >= from && f.Date <= to);
        var passedFI = await _db.FinalInspections
            .CountAsync(f => f.Date >= from && f.Date <= to && f.OverallStatus == "Approved");
        var qualityPassRate = totalFI > 0 ? Math.Round((decimal)passedFI / totalFI * 100, 1) : 0;

        var lowStockAlerts = await _db.PowderStocks
            .CountAsync(s => s.ReorderLevelKg.HasValue && s.CurrentStockKg < s.ReorderLevelKg.Value);

        var overdueWOs = await _db.WorkOrders
            .CountAsync(w => w.DispatchDate.HasValue && w.DispatchDate.Value < DateTime.UtcNow.Date
                         && w.Status != "Completed" && w.Status != "Dispatched" && w.Status != "Cancelled");

        // Charts
        var revenueTrend = await GetMonthlyRevenueTrend(from, to);
        var productionThroughput = await GetDailyProductionTrend(from, to);
        var woByStatus = await _db.WorkOrders
            .GroupBy(w => w.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var topCustomers = await _db.Invoices
            .Where(i => i.Date >= from && i.Date <= to && i.Status != "Cancelled")
            .GroupBy(i => i.Customer.Name)
            .Select(g => new CustomerRevenueDto
            {
                CustomerName = g.Key,
                Revenue = g.Sum(i => i.GrandTotal),
                AreaSFT = g.Sum(i => i.LineItems.Sum(l => l.AreaSFT))
            })
            .OrderByDescending(c => c.Revenue)
            .Take(5)
            .ToListAsync();

        return new AdminDashboardDto
        {
            ActiveWorkOrders = activeWOs,
            PendingInquiries = pendingInquiries,
            MonthlyRevenue = monthlyRevenue,
            MonthlyProductionSFT = monthlyProductionSFT,
            PendingDispatches = pendingDispatches,
            QualityPassRate = qualityPassRate,
            LowStockAlerts = lowStockAlerts,
            OverdueWorkOrders = overdueWOs,
            RevenueTrend = revenueTrend,
            ProductionThroughput = productionThroughput,
            WorkOrdersByStatus = woByStatus,
            TopCustomers = topCustomers
        };
    }

    // ───────────────────────── Leader Dashboard ─────────────────────────

    public async Task<AdminDashboardDto> GetLeaderDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period, string? department)
    {
        // Leaders see the same admin dashboard (full business overview)
        return await GetAdminDashboardAsync(dateFrom, dateTo, period);
    }

    // ───────────────────────── Sales Dashboard ─────────────────────────

    public async Task<SalesDashboardDto> GetSalesDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);

        var openInquiries = await _db.Inquiries
            .CountAsync(i => i.Status == "New" || i.Status == "InProgress");

        var quotationsSent = await _db.Quotations
            .CountAsync(q => q.Date >= from && q.Date <= to && q.Status == "Sent");

        var pisAwaiting = await _db.ProformaInvoices
            .CountAsync(p => p.Status == "Draft" || p.Status == "Sent");

        var activeWOs = await _db.WorkOrders
            .CountAsync(w => w.Status != "Completed" && w.Status != "Cancelled");

        // Conversion rate: WOs created from quotations / total quotations in period
        var totalQuotations = await _db.Quotations
            .CountAsync(q => q.Date >= from && q.Date <= to);
        var convertedToWO = await _db.WorkOrders
            .CountAsync(w => w.Date >= from && w.Date <= to);
        var conversionRate = totalQuotations > 0 ? Math.Round((decimal)convertedToWO / totalQuotations * 100, 1) : 0;

        // Inquiry aging
        var today = DateTime.UtcNow.Date;
        var allOpenInquiries = await _db.Inquiries
            .Where(i => i.Status == "New" || i.Status == "InProgress")
            .Select(i => EF.Functions.DateDiffDay(i.Date, today))
            .ToListAsync();

        var inquiryAging = new List<InquiryAgingDto>
        {
            new() { Bucket = "0-7 days", Count = allOpenInquiries.Count(d => d <= 7) },
            new() { Bucket = "8-15 days", Count = allOpenInquiries.Count(d => d > 7 && d <= 15) },
            new() { Bucket = "16-30 days", Count = allOpenInquiries.Count(d => d > 15 && d <= 30) },
            new() { Bucket = "30+ days", Count = allOpenInquiries.Count(d => d > 30) }
        };

        // Monthly quotations trend (last 6 months)
        var sixMonthsAgo = from.AddMonths(-5);
        var monthlyQuotations = await _db.Quotations
            .Where(q => q.Date >= sixMonthsAgo && q.Date <= to)
            .GroupBy(q => new { q.Date.Year, q.Date.Month })
            .Select(g => new ChartPointDto
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = g.Count()
            })
            .OrderBy(c => c.Label)
            .ToListAsync();

        return new SalesDashboardDto
        {
            OpenInquiries = openInquiries,
            QuotationsSentThisMonth = quotationsSent,
            PIsAwaitingApproval = pisAwaiting,
            ActiveWorkOrders = activeWOs,
            QuotationToWOConversionRate = conversionRate,
            InquiryAging = inquiryAging,
            MonthlyQuotations = monthlyQuotations
        };
    }

    // ───────────────────────── PPC Dashboard ─────────────────────────

    public async Task<PPCDashboardDto> GetPPCDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);

        var activePWOs = await _db.ProductionWorkOrders
            .CountAsync(p => p.Status != "Completed" && p.Status != "Cancelled");

        var unscheduledPWOs = await _db.ProductionWorkOrders
            .CountAsync(p => p.Status == "Created" && !p.Schedules.Any());

        // Weekly utilization: scheduled slots this week / total available (5 days × 2 shifts × units)
        var weekStart = from;
        var weekEnd = to;
        var scheduledSlots = await _db.ProductionSchedules
            .CountAsync(s => s.Date >= weekStart && s.Date <= weekEnd);
        var productionUnits = await _db.ProductionUnits.CountAsync();
        var totalSlots = 5 * 2 * Math.Max(productionUnits, 1); // 5 days × 2 shifts × units
        var utilization = totalSlots > 0 ? Math.Round((decimal)scheduledSlots / totalSlots * 100, 1) : 0;

        var totalScheduledSFT = await _db.PWOLineItems
            .Where(l => l.ProductionWorkOrder.Schedules.Any(s => s.Date >= from && s.Date <= to))
            .SumAsync(l => (decimal?)l.TotalSurfaceAreaSqft) ?? 0;

        // Weekly schedule load
        var weeklyLoad = await _db.ProductionSchedules
            .Where(s => s.Date >= from && s.Date <= to)
            .GroupBy(s => s.Date)
            .Select(g => new ChartPointDto
            {
                Label = g.Key.ToString("ddd dd/MM"),
                Value = g.Count()
            })
            .OrderBy(c => c.Label)
            .ToListAsync();

        var pwosByStatus = await _db.ProductionWorkOrders
            .GroupBy(p => p.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new PPCDashboardDto
        {
            ActivePWOs = activePWOs,
            UnscheduledPWOs = unscheduledPWOs,
            WeekUtilizationPercent = utilization,
            TotalScheduledSFT = totalScheduledSFT,
            WeeklyScheduleLoad = weeklyLoad,
            PWOsByStatus = pwosByStatus
        };
    }

    // ───────────────────────── Production Dashboard ─────────────────────────

    public async Task<ProductionDashboardDto> GetProductionDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);
        var today = DateTime.UtcNow.Date;

        var basketsToday = await _db.PretreatmentLogs
            .CountAsync(l => l.Date == today);

        var sftToday = await _db.PWOLineItems
            .Where(l => l.ProductionWorkOrder.ProductionLogs.Any(pl => pl.Date == today))
            .SumAsync(l => (decimal?)l.TotalSurfaceAreaSqft) ?? 0;

        var activeLogs = await _db.ProductionLogs
            .CountAsync(l => l.Date >= from && l.Date <= to);

        var avgSpeed = await _db.ProductionLogs
            .Where(l => l.Date >= from && l.Date <= to && l.ConveyorSpeedMtrPerMin.HasValue)
            .AverageAsync(l => (decimal?)l.ConveyorSpeedMtrPerMin) ?? 0;

        // Daily output trend
        var dailyOutput = await _db.ProductionLogs
            .Where(l => l.Date >= from && l.Date <= to)
            .GroupBy(l => l.Date)
            .Select(g => new ChartPointDto
            {
                Label = g.Key.ToString("dd/MM"),
                Value = g.Count()
            })
            .OrderBy(c => c.Label)
            .ToListAsync();

        return new ProductionDashboardDto
        {
            BasketsProcessedToday = basketsToday,
            SFTCoatedToday = sftToday,
            ActiveProductionLogs = activeLogs,
            AvgConveyorSpeed = Math.Round(avgSpeed, 1),
            DailyOutput = dailyOutput
        };
    }

    // ───────────────────────── Quality Dashboard ─────────────────────────

    public async Task<QualityDashboardDto> GetQualityDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);
        var today = DateTime.UtcNow.Date;

        var inspectionsToday = await _db.InProcessInspections
            .CountAsync(i => i.Date == today);

        // DFT pass rate: readings within spec / total readings in period
        var totalReadings = await _db.InProcessDFTReadings
            .CountAsync(r => r.InProcessInspection.Date >= from && r.InProcessInspection.Date <= to);
        var passReadings = await _db.InProcessDFTReadings
            .CountAsync(r => r.InProcessInspection.Date >= from && r.InProcessInspection.Date <= to && r.IsWithinSpec);
        var dftPassRate = totalReadings > 0 ? Math.Round((decimal)passReadings / totalReadings * 100, 1) : 0;

        var pendingFI = await _db.ProductionWorkOrders
            .CountAsync(p => p.Status == "Completed" && !p.FinalInspections.Any());

        var tcIssued = await _db.TestCertificates
            .CountAsync(t => t.Date >= from && t.Date <= to);

        // Overall pass rate from final inspections
        var totalFinal = await _db.FinalInspections
            .CountAsync(f => f.Date >= from && f.Date <= to);
        var passedFinal = await _db.FinalInspections
            .CountAsync(f => f.Date >= from && f.Date <= to && f.OverallStatus == "Approved");
        var overallPassRate = totalFinal > 0 ? Math.Round((decimal)passedFinal / totalFinal * 100, 1) : 0;

        // DFT trend (daily avg)
        var dftTrend = await _db.InProcessDFTReadings
            .Where(r => r.InProcessInspection.Date >= from && r.InProcessInspection.Date <= to
                     && r.AvgReading.HasValue)
            .GroupBy(r => r.InProcessInspection.Date)
            .Select(g => new ChartPointDto
            {
                Label = g.Key.ToString("dd/MM"),
                Value = g.Average(r => r.AvgReading!.Value)
            })
            .OrderBy(c => c.Label)
            .ToListAsync();

        // Inspection results breakdown
        var inspectionResults = await _db.FinalInspections
            .Where(f => f.Date >= from && f.Date <= to)
            .GroupBy(f => f.OverallStatus)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new QualityDashboardDto
        {
            InspectionsToday = inspectionsToday,
            DFTPassRate = dftPassRate,
            PendingFinalInspections = pendingFI,
            TestCertificatesIssued = tcIssued,
            OverallPassRate = overallPassRate,
            DFTTrend = dftTrend,
            InspectionResults = inspectionResults
        };
    }

    // ───────────────────────── SCM Dashboard ─────────────────────────

    public async Task<SCMDashboardDto> GetSCMDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1); // Monday

        var materialsToday = await _db.MaterialInwards
            .CountAsync(m => m.Date == today);

        var pendingDispatches = await _db.WorkOrders
            .CountAsync(w => w.Status == "Completed" && !w.DeliveryChallans.Any());

        var challansToday = await _db.DeliveryChallans
            .CountAsync(d => d.Date == today);

        var dispatchedThisWeek = await _db.DeliveryChallans
            .CountAsync(d => d.Date >= weekStart && d.Date <= today && d.Status == "Dispatched");

        var dcsByStatus = await _db.DeliveryChallans
            .GroupBy(d => d.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new SCMDashboardDto
        {
            MaterialsReceivedToday = materialsToday,
            PendingDispatches = pendingDispatches,
            ChallansDraftedToday = challansToday,
            DispatchedThisWeek = dispatchedThisWeek,
            DCsByStatus = dcsByStatus
        };
    }

    // ───────────────────────── Purchase Dashboard ─────────────────────────

    public async Task<PurchaseDashboardDto> GetPurchaseDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);

        var openIndents = await _db.PowderIndents
            .CountAsync(i => i.Status == "Requested" || i.Status == "Approved");

        var pendingPOs = await _db.PurchaseOrders
            .CountAsync(p => p.Status == "Draft" || p.Status == "Sent");

        var lowStockItems = await _db.PowderStocks
            .CountAsync(s => s.ReorderLevelKg.HasValue && s.CurrentStockKg < s.ReorderLevelKg.Value);

        var totalStockKg = await _db.PowderStocks
            .SumAsync(s => (decimal?)s.CurrentStockKg) ?? 0;

        var lowStockAlerts = await _db.PowderStocks
            .Where(s => s.ReorderLevelKg.HasValue && s.CurrentStockKg < s.ReorderLevelKg.Value)
            .Select(s => new DashboardPowderStockDto
            {
                PowderCode = s.PowderColor.PowderCode,
                ColorName = s.PowderColor.ColorName,
                CurrentStockKg = s.CurrentStockKg,
                ReorderLevelKg = s.ReorderLevelKg
            })
            .ToListAsync();

        // Monthly purchase spend (last 6 months)
        var sixMonthsAgo = from.AddMonths(-5);
        var monthlySpend = await _db.PurchaseOrders
            .Where(p => p.Date >= sixMonthsAgo && p.Date <= to && p.Status != "Cancelled")
            .GroupBy(p => new { p.Date.Year, p.Date.Month })
            .Select(g => new ChartPointDto
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = g.Sum(p => p.LineItems.Sum(l => l.Amount))
            })
            .OrderBy(c => c.Label)
            .ToListAsync();

        return new PurchaseDashboardDto
        {
            OpenIndents = openIndents,
            PendingPOs = pendingPOs,
            LowStockItems = lowStockItems,
            TotalPowderStockKg = totalStockKg,
            LowStockAlerts = lowStockAlerts,
            MonthlyPurchaseSpend = monthlySpend
        };
    }

    // ───────────────────────── Finance Dashboard ─────────────────────────

    public async Task<FinanceDashboardDto> GetFinanceDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var (from, to) = ResolveDateRange(dateFrom, dateTo, period);

        var monthlyInvoiced = await _db.Invoices
            .Where(i => i.Date >= from && i.Date <= to && i.Status != "Cancelled")
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

        var invoicesSent = await _db.Invoices
            .CountAsync(i => i.Date >= from && i.Date <= to && (i.Status == "Sent" || i.Status == "Finalized" || i.Status == "Paid"));

        var unpaidInvoices = await _db.Invoices
            .CountAsync(i => i.Status != "Paid" && i.Status != "Cancelled" && i.Status != "Draft");

        var outstandingAmount = await _db.Invoices
            .Where(i => i.Status != "Paid" && i.Status != "Cancelled" && i.Status != "Draft")
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

        // Monthly revenue (last 6 months)
        var sixMonthsAgo = from.AddMonths(-5);
        var monthlyRevenue = await _db.Invoices
            .Where(i => i.Date >= sixMonthsAgo && i.Date <= to && i.Status != "Cancelled")
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .Select(g => new ChartPointDto
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = g.Sum(i => i.GrandTotal)
            })
            .OrderBy(c => c.Label)
            .ToListAsync();

        var invoicesByStatus = await _db.Invoices
            .GroupBy(i => i.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new FinanceDashboardDto
        {
            MonthlyInvoicedAmount = monthlyInvoiced,
            InvoicesSentThisMonth = invoicesSent,
            UnpaidInvoices = unpaidInvoices,
            OutstandingAmount = outstandingAmount,
            MonthlyRevenue = monthlyRevenue,
            InvoicesByStatus = invoicesByStatus
        };
    }

    // ───────────────────────── Helpers ─────────────────────────

    private static (DateTime from, DateTime to) ResolveDateRange(DateTime? dateFrom, DateTime? dateTo, string? period)
    {
        var now = DateTime.UtcNow;

        if (dateFrom.HasValue && dateTo.HasValue)
            return (dateFrom.Value.Date, dateTo.Value.Date);

        return (period?.ToLower()) switch
        {
            "today" => (now.Date, now.Date),
            "week" => (now.Date.AddDays(-(int)now.DayOfWeek + 1), now.Date),
            "quarter" => (new DateTime(now.Year, (now.Month - 1) / 3 * 3 + 1, 1), now.Date),
            "year" => (new DateTime(now.Year, 1, 1), now.Date),
            _ => (new DateTime(now.Year, now.Month, 1), now.Date) // default: current month
        };
    }

    private async Task<List<ChartPointDto>> GetMonthlyRevenueTrend(DateTime from, DateTime to)
    {
        var sixMonthsAgo = from.AddMonths(-5);
        return await _db.Invoices
            .Where(i => i.Date >= sixMonthsAgo && i.Date <= to && i.Status != "Cancelled")
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .Select(g => new ChartPointDto
            {
                Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                Value = g.Sum(i => i.GrandTotal)
            })
            .OrderBy(c => c.Label)
            .ToListAsync();
    }

    private async Task<List<ChartPointDto>> GetDailyProductionTrend(DateTime from, DateTime to)
    {
        return await _db.ProductionLogs
            .Where(l => l.Date >= from && l.Date <= to)
            .GroupBy(l => l.Date)
            .Select(g => new ChartPointDto
            {
                Label = g.Key.ToString("dd/MM"),
                Value = g.Count()
            })
            .OrderBy(c => c.Label)
            .ToListAsync();
    }
}
