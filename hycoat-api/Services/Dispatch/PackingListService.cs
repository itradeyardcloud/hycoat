using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Dispatch;

public class PackingListService : IPackingListService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public PackingListService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PackingListDto>> GetAllAsync(
        string? search, int? workOrderId, int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.PackingLists
            .AsNoTracking()
            .Include(p => p.WorkOrder).ThenInclude(w => w.Customer)
            .Include(p => p.ProductionWorkOrder)
            .Include(p => p.PreparedByUser)
            .Include(p => p.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.WorkOrder.WONumber.ToLower().Contains(term) ||
                p.WorkOrder.Customer.Name.ToLower().Contains(term) ||
                p.ProductionWorkOrder.PWONumber.ToLower().Contains(term));
        }

        if (workOrderId.HasValue)
            query = query.Where(p => p.WorkOrderId == workOrderId.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "wonumber" => sortDesc
                ? query.OrderByDescending(p => p.WorkOrder.WONumber)
                : query.OrderBy(p => p.WorkOrder.WONumber),
            _ => sortDesc
                ? query.OrderByDescending(p => p.Date)
                : query.OrderBy(p => p.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PackingListDto>
        {
            Items = _mapper.Map<List<PackingListDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PackingListDetailDto> GetByIdAsync(int id)
    {
        var packingList = await _db.PackingLists
            .AsNoTracking()
            .Include(p => p.WorkOrder).ThenInclude(w => w.Customer)
            .Include(p => p.ProductionWorkOrder)
            .Include(p => p.PreparedByUser)
            .Include(p => p.Lines).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Packing List with ID {id} not found.");

        return _mapper.Map<PackingListDetailDto>(packingList);
    }

    public async Task<PackingListDetailDto?> GetByWorkOrderIdAsync(int woId)
    {
        var packingList = await _db.PackingLists
            .AsNoTracking()
            .Include(p => p.WorkOrder).ThenInclude(w => w.Customer)
            .Include(p => p.ProductionWorkOrder)
            .Include(p => p.PreparedByUser)
            .Include(p => p.Lines).ThenInclude(l => l.SectionProfile)
            .Where(p => p.WorkOrderId == woId)
            .OrderByDescending(p => p.Date)
            .FirstOrDefaultAsync();

        return packingList == null ? null : _mapper.Map<PackingListDetailDto>(packingList);
    }

    public async Task<PackingListDto> CreateAsync(CreatePackingListDto dto, string userId)
    {
        _ = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");
        _ = await _db.WorkOrders.FindAsync(dto.WorkOrderId)
            ?? throw new ArgumentException("Work Order not found.");

        var packingList = _mapper.Map<PackingList>(dto);
        packingList.Date = dto.Date.Date;
        packingList.PreparedByUserId = userId;
        packingList.CreatedBy = userId;
        packingList.Lines = _mapper.Map<List<PackingListLine>>(dto.Lines);

        _db.PackingLists.Add(packingList);
        await _db.SaveChangesAsync();

        var created = await _db.PackingLists
            .AsNoTracking()
            .Include(p => p.WorkOrder).ThenInclude(w => w.Customer)
            .Include(p => p.ProductionWorkOrder)
            .Include(p => p.PreparedByUser)
            .Include(p => p.Lines)
            .FirstAsync(p => p.Id == packingList.Id);

        return _mapper.Map<PackingListDto>(created);
    }

    public async Task<PackingListDto> UpdateAsync(int id, CreatePackingListDto dto, string userId)
    {
        var packingList = await _db.PackingLists
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Packing List with ID {id} not found.");

        packingList.Date = dto.Date.Date;
        packingList.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        packingList.WorkOrderId = dto.WorkOrderId;
        packingList.PackingType = dto.PackingType;
        packingList.BundleCount = dto.BundleCount;
        packingList.Notes = dto.Notes;
        packingList.UpdatedBy = userId;
        packingList.UpdatedAt = DateTime.UtcNow;

        // Replace lines
        _db.PackingListLines.RemoveRange(packingList.Lines);
        packingList.Lines = _mapper.Map<List<PackingListLine>>(dto.Lines);

        await _db.SaveChangesAsync();

        var updated = await _db.PackingLists
            .AsNoTracking()
            .Include(p => p.WorkOrder).ThenInclude(w => w.Customer)
            .Include(p => p.ProductionWorkOrder)
            .Include(p => p.PreparedByUser)
            .Include(p => p.Lines)
            .FirstAsync(p => p.Id == id);

        return _mapper.Map<PackingListDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var packingList = await _db.PackingLists.FindAsync(id)
            ?? throw new KeyNotFoundException($"Packing List with ID {id} not found.");

        packingList.IsDeleted = true;
        packingList.UpdatedBy = userId;
        packingList.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
