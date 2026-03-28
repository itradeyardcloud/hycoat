using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Planning;
using HycoatApi.Models.Planning;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Planning;

public class ProductionWorkOrderService : IProductionWorkOrderService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IProductionTimeCalcService _timeCalcService;

    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["Created"] = ["Scheduled"],
        ["Scheduled"] = ["InProgress"],
        ["InProgress"] = ["Complete"],
    };

    public ProductionWorkOrderService(AppDbContext db, IMapper mapper, IProductionTimeCalcService timeCalcService)
    {
        _db = db;
        _mapper = mapper;
        _timeCalcService = timeCalcService;
    }

    public async Task<PagedResponse<ProductionWorkOrderDto>> GetAllAsync(
        string? search, string? status, int? workOrderId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.ProductionWorkOrders
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Include(p => p.WorkOrder)
            .Include(p => p.Customer)
            .Include(p => p.ProductionUnit)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.PWONumber.ToLower().Contains(term) ||
                p.Customer.Name.ToLower().Contains(term) ||
                p.WorkOrder.WONumber.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        if (workOrderId.HasValue)
            query = query.Where(p => p.WorkOrderId == workOrderId.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "pwonumber" => sortDesc ? query.OrderByDescending(p => p.PWONumber) : query.OrderBy(p => p.PWONumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(p => p.Customer.Name) : query.OrderBy(p => p.Customer.Name),
            "status" => sortDesc ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "wonumber" => sortDesc ? query.OrderByDescending(p => p.WorkOrder.WONumber) : query.OrderBy(p => p.WorkOrder.WONumber),
            _ => sortDesc ? query.OrderByDescending(p => p.Date) : query.OrderBy(p => p.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductionWorkOrderDto
            {
                Id = p.Id,
                PWONumber = p.PWONumber,
                Date = p.Date,
                WONumber = p.WorkOrder.WONumber,
                CustomerName = p.Customer.Name,
                PowderCode = p.PowderCode,
                ColorName = p.ColorName,
                ProductionUnitName = p.ProductionUnit.Name,
                ShiftAllocation = p.ShiftAllocation,
                TotalTimeHrs = p.TotalTimeHrs,
                Status = p.Status
            })
            .ToListAsync();

        return new PagedResponse<ProductionWorkOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductionWorkOrderDetailDto> GetByIdAsync(int id)
    {
        var pwo = await _db.ProductionWorkOrders
            .AsNoTracking()
            .Include(p => p.WorkOrder)
            .Include(p => p.Customer)
            .Include(p => p.ProcessType)
            .Include(p => p.ProductionUnit)
            .Include(p => p.LineItems)
                .ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Production Work Order with ID {id} not found.");

        var dto = _mapper.Map<ProductionWorkOrderDetailDto>(pwo);

        // Load time calcs for line items
        var lineItemIds = pwo.LineItems.Select(l => l.Id).ToList();
        var timeCalcs = await _db.ProductionTimeCalcs
            .AsNoTracking()
            .Where(tc => lineItemIds.Contains(tc.PWOLineItemId))
            .ToListAsync();

        foreach (var lineDto in dto.LineItems)
        {
            var tc = timeCalcs.FirstOrDefault(t => t.PWOLineItemId == lineDto.Id);
            if (tc != null)
                lineDto.TimeCalc = _mapper.Map<ProductionTimeCalcDto>(tc);
        }

        return dto;
    }

    public async Task<ProductionWorkOrderDto> CreateAsync(CreateProductionWorkOrderDto dto, string userId)
    {
        // Validate Work Order exists and status allows PWO creation
        var wo = await _db.WorkOrders.FindAsync(dto.WorkOrderId)
            ?? throw new ArgumentException("Work Order not found.");

        var allowedWoStatuses = new[] { "MaterialReceived", "InProduction", "Confirmed" };
        if (!allowedWoStatuses.Contains(wo.Status))
            throw new InvalidOperationException($"Work Order status must be MaterialReceived, Confirmed, or InProduction. Current: {wo.Status}");

        // Validate references
        var unitExists = await _db.ProductionUnits.AnyAsync(u => u.Id == dto.ProductionUnitId);
        if (!unitExists) throw new ArgumentException("Production Unit not found.");

        var ptExists = await _db.ProcessTypes.AnyAsync(pt => pt.Id == dto.ProcessTypeId);
        if (!ptExists) throw new ArgumentException("Process Type not found.");

        // Validate section profiles
        var sectionIds = dto.LineItems.Select(l => l.SectionProfileId).Distinct().ToList();
        var sections = await _db.SectionProfiles.Where(s => sectionIds.Contains(s.Id)).ToListAsync();
        if (sections.Count != sectionIds.Count)
            throw new ArgumentException("One or more Section Profiles not found.");

        // Snapshot powder info
        string? powderCode = null;
        string? colorName = null;
        if (dto.PowderColorId.HasValue)
        {
            var pc = await _db.PowderColors.FindAsync(dto.PowderColorId.Value);
            if (pc != null)
            {
                powderCode = pc.PowderCode;
                colorName = pc.ColorName;
            }
        }

        var pwo = _mapper.Map<ProductionWorkOrder>(dto);
        pwo.PWONumber = await GeneratePWONumberAsync();
        pwo.CustomerId = wo.CustomerId;
        pwo.PowderCode = powderCode;
        pwo.ColorName = colorName;
        pwo.Status = "Created";
        pwo.CreatedBy = userId;

        // Create line items with computed fields
        var unit = await _db.ProductionUnits.FindAsync(dto.ProductionUnitId);
        var timeCalcLines = new List<TimeCalcLineItemDto>();

        foreach (var lineDto in dto.LineItems)
        {
            var section = sections.First(s => s.Id == lineDto.SectionProfileId);
            var lineItem = _mapper.Map<PWOLineItem>(lineDto);
            lineItem.PerimeterMM = section.PerimeterMM;

            // Surface area: Perimeter(mm) × Length(mm) / 1,000,000 = m²
            lineItem.UnitSurfaceAreaSqMtr = section.PerimeterMM * lineDto.LengthMM / 1_000_000m;
            // Total in sqft: m² × 10.7639 × quantity
            lineItem.TotalSurfaceAreaSqft = Math.Round(
                lineItem.UnitSurfaceAreaSqMtr * 10.7639m * lineDto.Quantity, 2);

            pwo.LineItems.Add(lineItem);

            timeCalcLines.Add(new TimeCalcLineItemDto
            {
                SectionProfileId = lineDto.SectionProfileId,
                Quantity = lineDto.Quantity,
                LengthMM = lineDto.LengthMM
            });
        }

        // Calculate production time
        var timeResult = _timeCalcService.Calculate(timeCalcLines, sections, unit!);
        pwo.PreTreatmentTimeHrs = timeResult.PreTreatmentTimeHrs;
        pwo.PostTreatmentTimeHrs = timeResult.PostTreatmentTimeHrs;
        pwo.TotalTimeHrs = timeResult.TotalTimeHrs;

        _db.ProductionWorkOrders.Add(pwo);
        await _db.SaveChangesAsync();

        // Save time calc records per line item
        for (int i = 0; i < pwo.LineItems.Count && i < timeResult.Lines.Count; i++)
        {
            var lineItem = pwo.LineItems.ElementAt(i);
            var calcDto = timeResult.Lines[i].Calc;
            var timeCalc = new ProductionTimeCalc
            {
                PWOLineItemId = lineItem.Id,
                ThicknessMM = calcDto.ThicknessMM,
                WidthMM = calcDto.WidthMM,
                HeightMM = calcDto.HeightMM,
                SpecificWeight = calcDto.SpecificWeight,
                WeightPerMtr = calcDto.WeightPerMtr,
                TotalWeightKg = calcDto.TotalWeightKg,
                LoadsRequired = calcDto.LoadsRequired,
                TotalTimePreTreatMins = calcDto.TotalTimePreTreatMins,
                ConveyorSpeedMtrPerMin = calcDto.ConveyorSpeedMtrPerMin,
                JigLengthMM = calcDto.JigLengthMM,
                GapBetweenJigsMM = calcDto.GapBetweenJigsMM,
                TotalConveyorDistanceMtrs = calcDto.TotalConveyorDistanceMtrs,
                TotalTimePostTreatMins = calcDto.TotalTimePostTreatMins
            };
            _db.ProductionTimeCalcs.Add(timeCalc);
        }

        // Update WO status to InProduction
        wo.Status = "InProduction";
        wo.UpdatedBy = userId;
        wo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Reload for response
        var created = await _db.ProductionWorkOrders
            .AsNoTracking()
            .Include(p => p.WorkOrder)
            .Include(p => p.Customer)
            .Include(p => p.ProductionUnit)
            .FirstAsync(p => p.Id == pwo.Id);

        return _mapper.Map<ProductionWorkOrderDto>(created);
    }

    public async Task<ProductionWorkOrderDto> UpdateAsync(int id, UpdateProductionWorkOrderDto dto, string userId)
    {
        var pwo = await _db.ProductionWorkOrders
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Production Work Order with ID {id} not found.");

        if (pwo.Status != "Created")
            throw new InvalidOperationException("Only PWOs with status 'Created' can be edited.");

        // Validate references
        var wo = await _db.WorkOrders.FindAsync(dto.WorkOrderId)
            ?? throw new ArgumentException("Work Order not found.");
        var unitExists = await _db.ProductionUnits.AnyAsync(u => u.Id == dto.ProductionUnitId);
        if (!unitExists) throw new ArgumentException("Production Unit not found.");

        var sectionIds = dto.LineItems.Select(l => l.SectionProfileId).Distinct().ToList();
        var sections = await _db.SectionProfiles.Where(s => sectionIds.Contains(s.Id)).ToListAsync();
        if (sections.Count != sectionIds.Count)
            throw new ArgumentException("One or more Section Profiles not found.");

        // Snapshot powder info
        string? powderCode = null;
        string? colorName = null;
        if (dto.PowderColorId.HasValue)
        {
            var pc = await _db.PowderColors.FindAsync(dto.PowderColorId.Value);
            if (pc != null)
            {
                powderCode = pc.PowderCode;
                colorName = pc.ColorName;
            }
        }

        // Remove old line items and time calcs
        var oldLineIds = pwo.LineItems.Select(l => l.Id).ToList();
        var oldTimeCalcs = await _db.ProductionTimeCalcs
            .Where(tc => oldLineIds.Contains(tc.PWOLineItemId))
            .ToListAsync();
        _db.ProductionTimeCalcs.RemoveRange(oldTimeCalcs);
        _db.PWOLineItems.RemoveRange(pwo.LineItems);

        // Update PWO fields
        _mapper.Map(dto, pwo);
        pwo.CustomerId = wo.CustomerId;
        pwo.PowderCode = powderCode;
        pwo.ColorName = colorName;
        pwo.UpdatedBy = userId;
        pwo.UpdatedAt = DateTime.UtcNow;

        // Re-create line items
        var unit = await _db.ProductionUnits.FindAsync(dto.ProductionUnitId);
        var timeCalcLines = new List<TimeCalcLineItemDto>();

        foreach (var lineDto in dto.LineItems)
        {
            var section = sections.First(s => s.Id == lineDto.SectionProfileId);
            var lineItem = _mapper.Map<PWOLineItem>(lineDto);
            lineItem.PerimeterMM = section.PerimeterMM;
            lineItem.UnitSurfaceAreaSqMtr = section.PerimeterMM * lineDto.LengthMM / 1_000_000m;
            lineItem.TotalSurfaceAreaSqft = Math.Round(
                lineItem.UnitSurfaceAreaSqMtr * 10.7639m * lineDto.Quantity, 2);

            pwo.LineItems.Add(lineItem);

            timeCalcLines.Add(new TimeCalcLineItemDto
            {
                SectionProfileId = lineDto.SectionProfileId,
                Quantity = lineDto.Quantity,
                LengthMM = lineDto.LengthMM
            });
        }

        // Recalculate time
        var timeResult = _timeCalcService.Calculate(timeCalcLines, sections, unit!);
        pwo.PreTreatmentTimeHrs = timeResult.PreTreatmentTimeHrs;
        pwo.PostTreatmentTimeHrs = timeResult.PostTreatmentTimeHrs;
        pwo.TotalTimeHrs = timeResult.TotalTimeHrs;

        await _db.SaveChangesAsync();

        // Save new time calc records
        for (int i = 0; i < pwo.LineItems.Count && i < timeResult.Lines.Count; i++)
        {
            var lineItem = pwo.LineItems.ElementAt(i);
            var calcDto = timeResult.Lines[i].Calc;
            _db.ProductionTimeCalcs.Add(new ProductionTimeCalc
            {
                PWOLineItemId = lineItem.Id,
                ThicknessMM = calcDto.ThicknessMM,
                WidthMM = calcDto.WidthMM,
                HeightMM = calcDto.HeightMM,
                SpecificWeight = calcDto.SpecificWeight,
                WeightPerMtr = calcDto.WeightPerMtr,
                TotalWeightKg = calcDto.TotalWeightKg,
                LoadsRequired = calcDto.LoadsRequired,
                TotalTimePreTreatMins = calcDto.TotalTimePreTreatMins,
                ConveyorSpeedMtrPerMin = calcDto.ConveyorSpeedMtrPerMin,
                JigLengthMM = calcDto.JigLengthMM,
                GapBetweenJigsMM = calcDto.GapBetweenJigsMM,
                TotalConveyorDistanceMtrs = calcDto.TotalConveyorDistanceMtrs,
                TotalTimePostTreatMins = calcDto.TotalTimePostTreatMins
            });
        }

        await _db.SaveChangesAsync();

        var updated = await _db.ProductionWorkOrders
            .AsNoTracking()
            .Include(p => p.WorkOrder)
            .Include(p => p.Customer)
            .Include(p => p.ProductionUnit)
            .FirstAsync(p => p.Id == id);

        return _mapper.Map<ProductionWorkOrderDto>(updated);
    }

    public async Task UpdateStatusAsync(int id, UpdatePWOStatusDto dto, string userId)
    {
        var pwo = await _db.ProductionWorkOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production Work Order with ID {id} not found.");

        if (!StatusTransitions.TryGetValue(pwo.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException($"Cannot transition from {pwo.Status} to {dto.Status}");

        pwo.Status = dto.Status;
        pwo.UpdatedBy = userId;
        pwo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var pwo = await _db.ProductionWorkOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production Work Order with ID {id} not found.");

        pwo.IsDeleted = true;
        pwo.UpdatedBy = userId;
        pwo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<ProductionTimeCalcResultDto> CalculateTimeAsync(ProductionTimeCalcRequestDto dto)
    {
        var unit = await _db.ProductionUnits.FindAsync(dto.ProductionUnitId)
            ?? throw new ArgumentException("Production Unit not found.");

        var sectionIds = dto.LineItems.Select(l => l.SectionProfileId).Distinct().ToList();
        var sections = await _db.SectionProfiles
            .Where(s => sectionIds.Contains(s.Id))
            .ToListAsync();

        return _timeCalcService.Calculate(dto.LineItems, sections, unit);
    }

    public async Task<List<LookupDto>> GetLookupAsync(string? status)
    {
        var query = _db.ProductionWorkOrders
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        return await query
            .OrderByDescending(p => p.Date)
            .Select(p => new LookupDto
            {
                Id = p.Id,
                Name = p.PWONumber
            })
            .ToListAsync();
    }

    private async Task<string> GeneratePWONumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PWO-{year}-";

        var lastPwo = await _db.ProductionWorkOrders
            .AsNoTracking()
            .Where(p => p.PWONumber.StartsWith(prefix))
            .OrderByDescending(p => p.PWONumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastPwo != null)
        {
            var parts = lastPwo.PWONumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var last))
                nextNumber = last + 1;
        }

        return $"{prefix}{nextNumber:D3}";
    }
}
