using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Purchase;

public class GRNService : IGRNService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public GRNService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<GRNDto>> GetAllAsync(
        string? search, int? purchaseOrderId, DateTime? dateFrom, DateTime? dateTo,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.GoodsReceivedNotes
            .AsNoTracking()
            .Include(g => g.PurchaseOrder)
            .Include(g => g.ReceivedByUser)
            .Include(g => g.LineItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(g =>
                g.GRNNumber.ToLower().Contains(term) ||
                g.PurchaseOrder.PONumber.ToLower().Contains(term));
        }

        if (purchaseOrderId.HasValue)
            query = query.Where(g => g.PurchaseOrderId == purchaseOrderId.Value);

        if (dateFrom.HasValue)
            query = query.Where(g => g.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(g => g.Date <= dateTo.Value.Date);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "grnnumber" => sortDesc
                ? query.OrderByDescending(g => g.GRNNumber)
                : query.OrderBy(g => g.GRNNumber),
            _ => sortDesc
                ? query.OrderByDescending(g => g.Date).ThenByDescending(g => g.Id)
                : query.OrderBy(g => g.Date).ThenBy(g => g.Id),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<GRNDto>
        {
            Items = _mapper.Map<List<GRNDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<GRNDetailDto> GetByIdAsync(int id)
    {
        var grn = await _db.GoodsReceivedNotes
            .AsNoTracking()
            .Include(g => g.PurchaseOrder)
            .Include(g => g.ReceivedByUser)
            .Include(g => g.LineItems).ThenInclude(l => l.PowderColor)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException($"GRN with ID {id} not found.");

        return _mapper.Map<GRNDetailDto>(grn);
    }

    public async Task<GRNDto> CreateAsync(CreateGRNDto dto, string userId)
    {
        var po = await _db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == dto.PurchaseOrderId)
            ?? throw new ArgumentException("Purchase Order not found.");

        if (po.Status == "Cancelled")
            throw new InvalidOperationException("Cannot create GRN for a cancelled Purchase Order.");

        var grn = _mapper.Map<GoodsReceivedNote>(dto);
        grn.GRNNumber = await GenerateNumberAsync();
        grn.Date = dto.Date.Date;
        grn.ReceivedByUserId = userId;
        grn.CreatedBy = userId;

        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<GRNLineItem>(lineDto);
            grn.LineItems.Add(line);
        }

        _db.GoodsReceivedNotes.Add(grn);

        // Update powder stock
        foreach (var line in grn.LineItems)
        {
            var stock = await _db.PowderStocks
                .FirstOrDefaultAsync(s => s.PowderColorId == line.PowderColorId);

            if (stock == null)
            {
                stock = new PowderStock
                {
                    PowderColorId = line.PowderColorId,
                    CurrentStockKg = line.QtyReceivedKg,
                    LastUpdated = DateTime.UtcNow
                };
                _db.PowderStocks.Add(stock);
            }
            else
            {
                stock.CurrentStockKg += line.QtyReceivedKg;
                stock.LastUpdated = DateTime.UtcNow;
            }
        }

        // Update PO status based on total received
        await UpdatePOStatusAsync(po, userId);

        await _db.SaveChangesAsync();

        var created = await _db.GoodsReceivedNotes
            .AsNoTracking()
            .Include(g => g.PurchaseOrder)
            .Include(g => g.ReceivedByUser)
            .Include(g => g.LineItems)
            .FirstAsync(g => g.Id == grn.Id);

        return _mapper.Map<GRNDto>(created);
    }

    public async Task<GRNDto> UpdateAsync(int id, CreateGRNDto dto, string userId)
    {
        var grn = await _db.GoodsReceivedNotes
            .Include(g => g.LineItems)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException($"GRN with ID {id} not found.");

        // Reverse old stock
        foreach (var oldLine in grn.LineItems)
        {
            var stock = await _db.PowderStocks
                .FirstOrDefaultAsync(s => s.PowderColorId == oldLine.PowderColorId);
            if (stock != null)
            {
                stock.CurrentStockKg -= oldLine.QtyReceivedKg;
                stock.LastUpdated = DateTime.UtcNow;
            }
        }

        grn.Date = dto.Date.Date;
        grn.PurchaseOrderId = dto.PurchaseOrderId;
        grn.Notes = dto.Notes;
        grn.UpdatedBy = userId;
        grn.UpdatedAt = DateTime.UtcNow;

        grn.LineItems.Clear();
        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<GRNLineItem>(lineDto);
            grn.LineItems.Add(line);
        }

        // Apply new stock
        foreach (var line in grn.LineItems)
        {
            var stock = await _db.PowderStocks
                .FirstOrDefaultAsync(s => s.PowderColorId == line.PowderColorId);

            if (stock == null)
            {
                stock = new PowderStock
                {
                    PowderColorId = line.PowderColorId,
                    CurrentStockKg = line.QtyReceivedKg,
                    LastUpdated = DateTime.UtcNow
                };
                _db.PowderStocks.Add(stock);
            }
            else
            {
                stock.CurrentStockKg += line.QtyReceivedKg;
                stock.LastUpdated = DateTime.UtcNow;
            }
        }

        // Recalculate PO status
        var po = await _db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == dto.PurchaseOrderId);
        if (po != null)
            await UpdatePOStatusAsync(po, userId);

        await _db.SaveChangesAsync();

        var updated = await _db.GoodsReceivedNotes
            .AsNoTracking()
            .Include(g => g.PurchaseOrder)
            .Include(g => g.ReceivedByUser)
            .Include(g => g.LineItems)
            .FirstAsync(g => g.Id == id);

        return _mapper.Map<GRNDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var grn = await _db.GoodsReceivedNotes
            .Include(g => g.LineItems)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException($"GRN with ID {id} not found.");

        // Reverse stock on soft delete
        foreach (var line in grn.LineItems)
        {
            var stock = await _db.PowderStocks
                .FirstOrDefaultAsync(s => s.PowderColorId == line.PowderColorId);
            if (stock != null)
            {
                stock.CurrentStockKg -= line.QtyReceivedKg;
                stock.LastUpdated = DateTime.UtcNow;
            }
        }

        grn.IsDeleted = true;
        grn.UpdatedBy = userId;
        grn.UpdatedAt = DateTime.UtcNow;

        // Recalculate PO status
        var po = await _db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == grn.PurchaseOrderId);
        if (po != null)
            await UpdatePOStatusAsync(po, userId);

        await _db.SaveChangesAsync();
    }

    private async Task UpdatePOStatusAsync(PurchaseOrder po, string userId)
    {
        var totalOrdered = po.LineItems.Sum(l => l.QtyKg);

        var totalReceived = await _db.GRNLineItems
            .Where(l => l.GoodsReceivedNote.PurchaseOrderId == po.Id && !l.GoodsReceivedNote.IsDeleted)
            .SumAsync(l => l.QtyReceivedKg);

        var newStatus = totalReceived >= totalOrdered
            ? "Received"
            : totalReceived > 0
                ? "PartiallyReceived"
                : po.Status;

        if (newStatus != po.Status && po.Status != "Cancelled")
        {
            po.Status = newStatus;
            po.UpdatedBy = userId;
            po.UpdatedAt = DateTime.UtcNow;

            // Also update linked indent if fully received
            if (newStatus == "Received" && po.PowderIndentId.HasValue)
            {
                var indent = await _db.PowderIndents.FindAsync(po.PowderIndentId.Value);
                if (indent != null && indent.Status == "Ordered")
                {
                    indent.Status = "Received";
                    indent.UpdatedBy = userId;
                    indent.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }

    private async Task<string> GenerateNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"GRN-{year}-";

        var lastNumber = await _db.GoodsReceivedNotes
            .Where(g => g.GRNNumber.StartsWith(prefix))
            .OrderByDescending(g => g.GRNNumber)
            .Select(g => g.GRNNumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastNumber != null)
        {
            var seqPart = lastNumber.Replace(prefix, "");
            if (int.TryParse(seqPart, out var seq))
                nextSeq = seq + 1;
        }

        return $"{prefix}{nextSeq:D3}";
    }
}
