using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Reports;
using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Reports;

public class YieldReportService : IYieldReportService
{
    private readonly AppDbContext _db;

    public YieldReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<YieldReportDto>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, int? productionUnitId,
        int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.YieldReports
            .AsNoTracking()
            .Include(x => x.ProductionUnit)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(x => x.Date >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(x => x.Date <= dateTo.Value.Date);
        if (productionUnitId.HasValue)
            query = query.Where(x => x.ProductionUnitId == productionUnitId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.ProductionUnit.Name)
            .ThenBy(x => x.Shift)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<YieldReportDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<YieldSummaryDto> GetSummaryAsync(DateTime? date = null)
    {
        var targetDate = (date ?? DateTime.UtcNow).Date;

        var reports = await _db.YieldReports
            .AsNoTracking()
            .Include(x => x.ProductionUnit)
            .Where(x => x.Date == targetDate)
            .ToListAsync();

        var metrics = reports.Select(CalculateMetrics).ToList();

        var totalProduction = reports.Sum(x => x.ProductionSFT);
        var totalRejection = reports.Sum(x => x.RejectionSFT);
        var totalNet = metrics.Sum(x => x.NetProductionSFT);

        var electricityCost = metrics.Sum(x => x.ElectricityCost);
        var gasCost = metrics.Sum(x => x.OvenGasCost);
        var powderCost = metrics.Sum(x => x.PowderCost);
        var manpowerCost = reports.Sum(x => x.ManpowerCost);
        var otherCost = reports.Sum(x => x.OtherCost);
        var totalCost = electricityCost + gasCost + powderCost + manpowerCost + otherCost;

        var revenue = metrics.Sum(x => x.Revenue);
        var profit = revenue - totalCost;

        return new YieldSummaryDto
        {
            Date = targetDate,
            EntryCount = reports.Count,
            TotalProductionSFT = totalProduction,
            TotalRejectionSFT = totalRejection,
            TotalNetProductionSFT = totalNet,
            YieldPercent = totalProduction > 0
                ? Math.Round(totalNet / totalProduction * 100m, 2)
                : 0m,
            TotalElectricityConsumedKwh = metrics.Sum(x => x.ElectricityConsumedKwh),
            TotalGasConsumedUnits = metrics.Sum(x => x.OvenGasConsumedUnits),
            ElectricityCost = Math.Round(electricityCost, 2),
            OvenGasCost = Math.Round(gasCost, 2),
            PowderCost = Math.Round(powderCost, 2),
            ManpowerCost = Math.Round(manpowerCost, 2),
            OtherCost = Math.Round(otherCost, 2),
            TotalCost = Math.Round(totalCost, 2),
            CostPerSFT = totalNet > 0 ? Math.Round(totalCost / totalNet, 4) : 0m,
            Revenue = Math.Round(revenue, 2),
            Profit = Math.Round(profit, 2),
            RoiPercent = totalCost > 0 ? Math.Round(profit / totalCost * 100m, 2) : 0m,
        };
    }

    public async Task<YieldReportDto> CreateAsync(CreateYieldReportDto dto, string userId)
    {
        var unit = await _db.ProductionUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.ProductionUnitId)
            ?? throw new ArgumentException("Production Unit not found.");

        var date = dto.Date.Date;

        var duplicate = await _db.YieldReports.AnyAsync(x =>
            x.Date == date &&
            x.Shift == dto.Shift &&
            x.ProductionUnitId == dto.ProductionUnitId);

        if (duplicate)
            throw new InvalidOperationException(
                $"Yield report already exists for {unit.Name} on {date:yyyy-MM-dd} ({dto.Shift} shift).");

        var entity = new YieldReport
        {
            Date = date,
            Shift = dto.Shift,
            ProductionUnitId = dto.ProductionUnitId,
            ProductionSFT = dto.ProductionSFT,
            RejectionSFT = dto.RejectionSFT,
            ElectricityOpeningKwh = dto.ElectricityOpeningKwh,
            ElectricityClosingKwh = dto.ElectricityClosingKwh,
            ElectricityRatePerKwh = dto.ElectricityRatePerKwh,
            OvenGasOpeningReading = dto.OvenGasOpeningReading,
            OvenGasClosingReading = dto.OvenGasClosingReading,
            OvenGasRatePerUnit = dto.OvenGasRatePerUnit,
            PowderUsedKg = dto.PowderUsedKg,
            PowderRatePerKg = dto.PowderRatePerKg,
            ManpowerCost = dto.ManpowerCost,
            OtherCost = dto.OtherCost,
            SellingPricePerSFT = dto.SellingPricePerSFT,
            Remarks = dto.Remarks,
            CreatedBy = userId,
        };

        _db.YieldReports.Add(entity);
        await _db.SaveChangesAsync();

        var created = await _db.YieldReports
            .AsNoTracking()
            .Include(x => x.ProductionUnit)
            .FirstAsync(x => x.Id == entity.Id);

        return MapToDto(created);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.YieldReports.FindAsync(id)
            ?? throw new KeyNotFoundException($"Yield report with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static YieldReportDto MapToDto(YieldReport source)
    {
        var metrics = CalculateMetrics(source);

        return new YieldReportDto
        {
            Id = source.Id,
            Date = source.Date,
            Shift = source.Shift,
            ProductionUnitId = source.ProductionUnitId,
            ProductionUnitName = source.ProductionUnit?.Name ?? string.Empty,
            ProductionSFT = source.ProductionSFT,
            RejectionSFT = source.RejectionSFT,
            NetProductionSFT = metrics.NetProductionSFT,
            YieldPercent = metrics.YieldPercent,
            ElectricityOpeningKwh = source.ElectricityOpeningKwh,
            ElectricityClosingKwh = source.ElectricityClosingKwh,
            ElectricityConsumedKwh = metrics.ElectricityConsumedKwh,
            ElectricityRatePerKwh = source.ElectricityRatePerKwh,
            ElectricityCost = metrics.ElectricityCost,
            OvenGasOpeningReading = source.OvenGasOpeningReading,
            OvenGasClosingReading = source.OvenGasClosingReading,
            OvenGasConsumedUnits = metrics.OvenGasConsumedUnits,
            OvenGasRatePerUnit = source.OvenGasRatePerUnit,
            OvenGasCost = metrics.OvenGasCost,
            PowderUsedKg = source.PowderUsedKg,
            PowderRatePerKg = source.PowderRatePerKg,
            PowderCost = metrics.PowderCost,
            ManpowerCost = source.ManpowerCost,
            OtherCost = source.OtherCost,
            TotalCost = metrics.TotalCost,
            CostPerSFT = metrics.CostPerSFT,
            SellingPricePerSFT = source.SellingPricePerSFT,
            Revenue = metrics.Revenue,
            Profit = metrics.Profit,
            RoiPercent = metrics.RoiPercent,
            Remarks = source.Remarks,
        };
    }

    private static YieldMetrics CalculateMetrics(YieldReport source)
    {
        var netProduction = Math.Max(0m, source.ProductionSFT - source.RejectionSFT);
        var yieldPercent = source.ProductionSFT > 0
            ? Math.Round(netProduction / source.ProductionSFT * 100m, 2)
            : 0m;

        var electricityConsumed = Math.Max(0m, source.ElectricityClosingKwh - source.ElectricityOpeningKwh);
        var gasConsumed = Math.Max(0m, source.OvenGasClosingReading - source.OvenGasOpeningReading);

        var electricityCost = electricityConsumed * source.ElectricityRatePerKwh;
        var gasCost = gasConsumed * source.OvenGasRatePerUnit;
        var powderCost = source.PowderUsedKg * source.PowderRatePerKg;

        var totalCost = electricityCost + gasCost + powderCost + source.ManpowerCost + source.OtherCost;

        var revenue = netProduction * source.SellingPricePerSFT;
        var profit = revenue - totalCost;

        return new YieldMetrics
        {
            NetProductionSFT = netProduction,
            YieldPercent = yieldPercent,
            ElectricityConsumedKwh = Math.Round(electricityConsumed, 3),
            OvenGasConsumedUnits = Math.Round(gasConsumed, 3),
            ElectricityCost = Math.Round(electricityCost, 2),
            OvenGasCost = Math.Round(gasCost, 2),
            PowderCost = Math.Round(powderCost, 2),
            TotalCost = Math.Round(totalCost, 2),
            CostPerSFT = netProduction > 0 ? Math.Round(totalCost / netProduction, 4) : 0m,
            Revenue = Math.Round(revenue, 2),
            Profit = Math.Round(profit, 2),
            RoiPercent = totalCost > 0 ? Math.Round(profit / totalCost * 100m, 2) : 0m,
        };
    }

    private class YieldMetrics
    {
        public decimal NetProductionSFT { get; init; }
        public decimal YieldPercent { get; init; }
        public decimal ElectricityConsumedKwh { get; init; }
        public decimal OvenGasConsumedUnits { get; init; }
        public decimal ElectricityCost { get; init; }
        public decimal OvenGasCost { get; init; }
        public decimal PowderCost { get; init; }
        public decimal TotalCost { get; init; }
        public decimal CostPerSFT { get; init; }
        public decimal Revenue { get; init; }
        public decimal Profit { get; init; }
        public decimal RoiPercent { get; init; }
    }
}