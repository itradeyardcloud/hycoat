using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Quality;

public class PanelTestService : IPanelTestService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public PanelTestService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PanelTestDto>> GetAllAsync(
        string? search, DateTime? date, int? productionWorkOrderId,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.PanelTests
            .AsNoTracking()
            .Include(t => t.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(t => t.InspectorUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t =>
                t.ProductionWorkOrder.PWONumber.ToLower().Contains(term) ||
                t.ProductionWorkOrder.Customer.Name.ToLower().Contains(term));
        }

        if (date.HasValue)
            query = query.Where(t => t.Date == date.Value.Date);

        if (productionWorkOrderId.HasValue)
            query = query.Where(t => t.ProductionWorkOrderId == productionWorkOrderId.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "pwonumber" or "pwo" => sortDesc
                ? query.OrderByDescending(t => t.ProductionWorkOrder.PWONumber)
                : query.OrderBy(t => t.ProductionWorkOrder.PWONumber),
            _ => sortDesc
                ? query.OrderByDescending(t => t.Date)
                : query.OrderBy(t => t.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PanelTestDto>
        {
            Items = _mapper.Map<List<PanelTestDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PanelTestDetailDto> GetByIdAsync(int id)
    {
        var test = await _db.PanelTests
            .AsNoTracking()
            .Include(t => t.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(t => t.InspectorUser)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Panel Test with ID {id} not found.");

        return _mapper.Map<PanelTestDetailDto>(test);
    }

    public async Task<List<PanelTestDto>> GetByPWOAsync(int pwoId)
    {
        var tests = await _db.PanelTests
            .AsNoTracking()
            .Include(t => t.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(t => t.InspectorUser)
            .Where(t => t.ProductionWorkOrderId == pwoId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return _mapper.Map<List<PanelTestDto>>(tests);
    }

    public async Task<PanelTestDto> CreateAsync(CreatePanelTestDto dto, string userId)
    {
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress to create a panel test.");

        var test = _mapper.Map<PanelTest>(dto);
        test.Date = dto.Date.Date;
        test.InspectorUserId = userId;
        test.CreatedBy = userId;

        _db.PanelTests.Add(test);
        await _db.SaveChangesAsync();

        var created = await _db.PanelTests
            .AsNoTracking()
            .Include(t => t.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(t => t.InspectorUser)
            .FirstAsync(t => t.Id == test.Id);

        return _mapper.Map<PanelTestDto>(created);
    }

    public async Task<PanelTestDto> UpdateAsync(int id, CreatePanelTestDto dto, string userId)
    {
        var test = await _db.PanelTests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Panel Test with ID {id} not found.");

        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress.");

        test.Date = dto.Date.Date;
        test.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        test.BoilingWaterResult = dto.BoilingWaterResult;
        test.BoilingWaterStatus = dto.BoilingWaterStatus;
        test.ImpactTestResult = dto.ImpactTestResult;
        test.ImpactTestStatus = dto.ImpactTestStatus;
        test.ConicalMandrelResult = dto.ConicalMandrelResult;
        test.ConicalMandrelStatus = dto.ConicalMandrelStatus;
        test.PencilHardnessResult = dto.PencilHardnessResult;
        test.PencilHardnessStatus = dto.PencilHardnessStatus;
        test.Remarks = dto.Remarks;
        test.UpdatedBy = userId;
        test.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.PanelTests
            .AsNoTracking()
            .Include(t => t.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(t => t.InspectorUser)
            .FirstAsync(t => t.Id == id);

        return _mapper.Map<PanelTestDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var test = await _db.PanelTests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Panel Test with ID {id} not found.");

        test.IsDeleted = true;
        test.UpdatedBy = userId;
        test.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
