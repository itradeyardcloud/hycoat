using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Quality;

public class FinalInspectionService : IFinalInspectionService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public FinalInspectionService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<FinalInspectionDto>> GetAllAsync(
        string? search, DateTime? date, int? productionWorkOrderId, string? overallStatus,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.FinalInspections
            .AsNoTracking()
            .Include(f => f.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(f => f.InspectorUser)
            .Include(f => f.TestCertificate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(f =>
                f.InspectionNumber.ToLower().Contains(term) ||
                f.ProductionWorkOrder.PWONumber.ToLower().Contains(term) ||
                f.ProductionWorkOrder.Customer.Name.ToLower().Contains(term));
        }

        if (date.HasValue)
            query = query.Where(f => f.Date == date.Value.Date);

        if (productionWorkOrderId.HasValue)
            query = query.Where(f => f.ProductionWorkOrderId == productionWorkOrderId.Value);

        if (!string.IsNullOrWhiteSpace(overallStatus))
            query = query.Where(f => f.OverallStatus == overallStatus);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "inspectionnumber" => sortDesc
                ? query.OrderByDescending(f => f.InspectionNumber)
                : query.OrderBy(f => f.InspectionNumber),
            "pwonumber" or "pwo" => sortDesc
                ? query.OrderByDescending(f => f.ProductionWorkOrder.PWONumber)
                : query.OrderBy(f => f.ProductionWorkOrder.PWONumber),
            "status" => sortDesc
                ? query.OrderByDescending(f => f.OverallStatus)
                : query.OrderBy(f => f.OverallStatus),
            _ => sortDesc
                ? query.OrderByDescending(f => f.Date)
                : query.OrderBy(f => f.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<FinalInspectionDto>
        {
            Items = _mapper.Map<List<FinalInspectionDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FinalInspectionDetailDto> GetByIdAsync(int id)
    {
        var inspection = await _db.FinalInspections
            .AsNoTracking()
            .Include(f => f.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(f => f.InspectorUser)
            .Include(f => f.TestCertificate)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Final Inspection with ID {id} not found.");

        return _mapper.Map<FinalInspectionDetailDto>(inspection);
    }

    public async Task<FinalInspectionDetailDto?> GetByPWOAsync(int pwoId)
    {
        var inspection = await _db.FinalInspections
            .AsNoTracking()
            .Include(f => f.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(f => f.InspectorUser)
            .Include(f => f.TestCertificate)
            .Where(f => f.ProductionWorkOrderId == pwoId)
            .OrderByDescending(f => f.Date)
            .FirstOrDefaultAsync();

        return inspection == null ? null : _mapper.Map<FinalInspectionDetailDto>(inspection);
    }

    public async Task<FinalInspectionDto> CreateAsync(CreateFinalInspectionDto dto, string userId)
    {
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        // Check for existing non-Rework FIR
        var existingFIR = await _db.FinalInspections
            .Where(f => f.ProductionWorkOrderId == dto.ProductionWorkOrderId && f.OverallStatus != "Rework")
            .FirstOrDefaultAsync();

        if (existingFIR != null)
            throw new InvalidOperationException("A Final Inspection already exists for this PWO. Only PWOs with a Rework status allow new inspections.");

        // Auto-generate inspection number
        var year = DateTime.UtcNow.Year;
        var lastNumber = await _db.FinalInspections
            .Where(f => f.InspectionNumber.StartsWith($"FIR-{year}-"))
            .OrderByDescending(f => f.InspectionNumber)
            .Select(f => f.InspectionNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                sequence = lastSeq + 1;
        }

        var inspection = _mapper.Map<FinalInspection>(dto);
        inspection.InspectionNumber = $"FIR-{year}-{sequence:D3}";
        inspection.Date = dto.Date.Date;
        inspection.InspectorUserId = userId;
        inspection.CreatedBy = userId;

        _db.FinalInspections.Add(inspection);
        await _db.SaveChangesAsync();

        var created = await _db.FinalInspections
            .AsNoTracking()
            .Include(f => f.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(f => f.InspectorUser)
            .Include(f => f.TestCertificate)
            .FirstAsync(f => f.Id == inspection.Id);

        return _mapper.Map<FinalInspectionDto>(created);
    }

    public async Task<FinalInspectionDto> UpdateAsync(int id, CreateFinalInspectionDto dto, string userId)
    {
        var inspection = await _db.FinalInspections.FindAsync(id)
            ?? throw new KeyNotFoundException($"Final Inspection with ID {id} not found.");

        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        inspection.Date = dto.Date.Date;
        inspection.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        inspection.LotQuantity = dto.LotQuantity;
        inspection.SampledQuantity = dto.SampledQuantity;
        inspection.VisualCheckStatus = dto.VisualCheckStatus;
        inspection.DFTRecheckStatus = dto.DFTRecheckStatus;
        inspection.ShadeMatchFinalStatus = dto.ShadeMatchFinalStatus;
        inspection.OverallStatus = dto.OverallStatus;
        inspection.Remarks = dto.Remarks;
        inspection.UpdatedBy = userId;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.FinalInspections
            .AsNoTracking()
            .Include(f => f.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(f => f.InspectorUser)
            .Include(f => f.TestCertificate)
            .FirstAsync(f => f.Id == id);

        return _mapper.Map<FinalInspectionDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var inspection = await _db.FinalInspections.FindAsync(id)
            ?? throw new KeyNotFoundException($"Final Inspection with ID {id} not found.");

        inspection.IsDeleted = true;
        inspection.UpdatedBy = userId;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
