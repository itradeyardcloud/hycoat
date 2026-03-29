using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Purchase;

public class PowderIndentService : IPowderIndentService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public PowderIndentService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PowderIndentDto>> GetAllAsync(
        string? search, string? status, DateTime? dateFrom, DateTime? dateTo,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.PowderIndents
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder)
            .Include(i => i.RequestedByUser)
            .Include(i => i.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.IndentNumber.ToLower().Contains(term) ||
                (i.ProductionWorkOrder != null && i.ProductionWorkOrder.PWONumber.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);

        if (dateFrom.HasValue)
            query = query.Where(i => i.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(i => i.Date <= dateTo.Value.Date);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "indentnumber" => sortDesc
                ? query.OrderByDescending(i => i.IndentNumber)
                : query.OrderBy(i => i.IndentNumber),
            "status" => sortDesc
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => sortDesc
                ? query.OrderByDescending(i => i.Date).ThenByDescending(i => i.Id)
                : query.OrderBy(i => i.Date).ThenBy(i => i.Id),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PowderIndentDto>
        {
            Items = _mapper.Map<List<PowderIndentDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PowderIndentDetailDto> GetByIdAsync(int id)
    {
        var indent = await _db.PowderIndents
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder)
            .Include(i => i.RequestedByUser)
            .Include(i => i.Lines).ThenInclude(l => l.PowderColor)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Powder Indent with ID {id} not found.");

        return _mapper.Map<PowderIndentDetailDto>(indent);
    }

    public async Task<PowderIndentDto> CreateAsync(CreatePowderIndentDto dto, string userId)
    {
        if (dto.ProductionWorkOrderId.HasValue)
        {
            var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId.Value)
                ?? throw new ArgumentException("Production Work Order not found.");
        }

        var indent = _mapper.Map<PowderIndent>(dto);
        indent.IndentNumber = await GenerateNumberAsync("IND");
        indent.Date = dto.Date.Date;
        indent.Status = "Requested";
        indent.RequestedByUserId = userId;
        indent.CreatedBy = userId;

        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<PowderIndentLine>(lineDto);
            indent.Lines.Add(line);
        }

        _db.PowderIndents.Add(indent);
        await _db.SaveChangesAsync();

        var created = await _db.PowderIndents
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder)
            .Include(i => i.RequestedByUser)
            .Include(i => i.Lines)
            .FirstAsync(i => i.Id == indent.Id);

        return _mapper.Map<PowderIndentDto>(created);
    }

    public async Task<PowderIndentDto> UpdateAsync(int id, CreatePowderIndentDto dto, string userId)
    {
        var indent = await _db.PowderIndents
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Powder Indent with ID {id} not found.");

        if (indent.Status != "Requested")
            throw new InvalidOperationException("Only indents with 'Requested' status can be updated.");

        indent.Date = dto.Date.Date;
        indent.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        indent.Notes = dto.Notes;
        indent.UpdatedBy = userId;
        indent.UpdatedAt = DateTime.UtcNow;

        indent.Lines.Clear();
        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<PowderIndentLine>(lineDto);
            indent.Lines.Add(line);
        }

        await _db.SaveChangesAsync();

        var updated = await _db.PowderIndents
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder)
            .Include(i => i.RequestedByUser)
            .Include(i => i.Lines)
            .FirstAsync(i => i.Id == id);

        return _mapper.Map<PowderIndentDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var indent = await _db.PowderIndents.FindAsync(id)
            ?? throw new KeyNotFoundException($"Powder Indent with ID {id} not found.");

        indent.IsDeleted = true;
        indent.UpdatedBy = userId;
        indent.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PowderIndentDto> UpdateStatusAsync(int id, UpdatePowderIndentStatusDto dto, string userId)
    {
        var indent = await _db.PowderIndents
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Powder Indent with ID {id} not found.");

        ValidateStatusTransition(indent.Status, dto.Status);

        indent.Status = dto.Status;
        indent.UpdatedBy = userId;
        indent.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.PowderIndents
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder)
            .Include(i => i.RequestedByUser)
            .Include(i => i.Lines)
            .FirstAsync(i => i.Id == id);

        return _mapper.Map<PowderIndentDto>(updated);
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        var allowed = currentStatus switch
        {
            "Requested" => new[] { "Approved" },
            "Approved" => new[] { "Ordered" },
            "Ordered" => new[] { "Received" },
            _ => Array.Empty<string>()
        };

        if (!allowed.Contains(newStatus))
            throw new InvalidOperationException(
                $"Cannot transition from '{currentStatus}' to '{newStatus}'.");
    }

    private async Task<string> GenerateNumberAsync(string prefix)
    {
        var year = DateTime.UtcNow.Year;
        var fmt = $"{prefix}-{year}-";

        var lastNumber = await _db.PowderIndents
            .Where(i => i.IndentNumber.StartsWith(fmt))
            .OrderByDescending(i => i.IndentNumber)
            .Select(i => i.IndentNumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastNumber != null)
        {
            var seqPart = lastNumber.Replace(fmt, "");
            if (int.TryParse(seqPart, out var seq))
                nextSeq = seq + 1;
        }

        return $"{fmt}{nextSeq:D3}";
    }
}
