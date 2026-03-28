using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs.Planning;
using HycoatApi.Models.Planning;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Planning;

public class ProductionScheduleService : IProductionScheduleService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["Planned"] = ["InProgress", "Cancelled"],
        ["InProgress"] = ["Completed", "Cancelled"],
    };

    public ProductionScheduleService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ScheduleEntryDto>> GetScheduleAsync(
        DateTime startDate, DateTime endDate, int? productionUnitId, string? shift)
    {
        var query = _db.ProductionSchedules
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .Include(s => s.ProductionUnit)
            .Include(s => s.ProductionWorkOrder)
                .ThenInclude(p => p.Customer)
            .AsQueryable();

        if (productionUnitId.HasValue)
            query = query.Where(s => s.ProductionUnitId == productionUnitId.Value);

        if (!string.IsNullOrWhiteSpace(shift))
            query = query.Where(s => s.Shift == shift);

        var entries = await query
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Shift)
            .ThenBy(s => s.SortOrder)
            .ToListAsync();

        return _mapper.Map<List<ScheduleEntryDto>>(entries);
    }

    public async Task<ScheduleEntryDto> CreateAsync(CreateScheduleEntryDto dto, string userId)
    {
        // Validate PWO exists
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "Created" && pwo.Status != "Scheduled")
            throw new InvalidOperationException($"PWO status must be Created or Scheduled. Current: {pwo.Status}");

        // Validate Production Unit
        var unitExists = await _db.ProductionUnits.AnyAsync(u => u.Id == dto.ProductionUnitId);
        if (!unitExists) throw new ArgumentException("Production Unit not found.");

        // Check unique constraint: same PWO can't be in same date+shift+unit
        var duplicate = await _db.ProductionSchedules
            .AnyAsync(s => !s.IsDeleted &&
                s.ProductionWorkOrderId == dto.ProductionWorkOrderId &&
                s.Date == dto.Date &&
                s.Shift == dto.Shift &&
                s.ProductionUnitId == dto.ProductionUnitId);

        if (duplicate)
            throw new InvalidOperationException("This PWO is already scheduled for this date, shift, and unit.");

        // Get next sort order for this slot
        var maxSort = await _db.ProductionSchedules
            .Where(s => !s.IsDeleted &&
                s.Date == dto.Date &&
                s.Shift == dto.Shift &&
                s.ProductionUnitId == dto.ProductionUnitId)
            .MaxAsync(s => (int?)s.SortOrder) ?? 0;

        var schedule = _mapper.Map<ProductionSchedule>(dto);
        schedule.SortOrder = maxSort + 1;
        schedule.Status = "Planned";
        schedule.CreatedBy = userId;

        _db.ProductionSchedules.Add(schedule);

        // Update PWO status to Scheduled
        if (pwo.Status == "Created")
        {
            pwo.Status = "Scheduled";
            pwo.UpdatedBy = userId;
            pwo.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var created = await _db.ProductionSchedules
            .AsNoTracking()
            .Include(s => s.ProductionUnit)
            .Include(s => s.ProductionWorkOrder)
                .ThenInclude(p => p.Customer)
            .FirstAsync(s => s.Id == schedule.Id);

        return _mapper.Map<ScheduleEntryDto>(created);
    }

    public async Task<ScheduleEntryDto> UpdateAsync(int id, UpdateScheduleEntryDto dto, string userId)
    {
        var schedule = await _db.ProductionSchedules.FindAsync(id)
            ?? throw new KeyNotFoundException($"Schedule entry with ID {id} not found.");

        if (schedule.IsDeleted)
            throw new KeyNotFoundException($"Schedule entry with ID {id} not found.");

        // Validate references
        var pwoExists = await _db.ProductionWorkOrders.AnyAsync(p => p.Id == dto.ProductionWorkOrderId);
        if (!pwoExists) throw new ArgumentException("Production Work Order not found.");

        var unitExists = await _db.ProductionUnits.AnyAsync(u => u.Id == dto.ProductionUnitId);
        if (!unitExists) throw new ArgumentException("Production Unit not found.");

        // Check duplicate (exclude self)
        var duplicate = await _db.ProductionSchedules
            .AnyAsync(s => !s.IsDeleted &&
                s.Id != id &&
                s.ProductionWorkOrderId == dto.ProductionWorkOrderId &&
                s.Date == dto.Date &&
                s.Shift == dto.Shift &&
                s.ProductionUnitId == dto.ProductionUnitId);

        if (duplicate)
            throw new InvalidOperationException("This PWO is already scheduled for this date, shift, and unit.");

        _mapper.Map(dto, schedule);
        schedule.UpdatedBy = userId;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.ProductionSchedules
            .AsNoTracking()
            .Include(s => s.ProductionUnit)
            .Include(s => s.ProductionWorkOrder)
                .ThenInclude(p => p.Customer)
            .FirstAsync(s => s.Id == id);

        return _mapper.Map<ScheduleEntryDto>(updated);
    }

    public async Task UpdateStatusAsync(int id, UpdateScheduleStatusDto dto, string userId)
    {
        var schedule = await _db.ProductionSchedules.FindAsync(id)
            ?? throw new KeyNotFoundException($"Schedule entry with ID {id} not found.");

        if (!StatusTransitions.TryGetValue(schedule.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException($"Cannot transition from {schedule.Status} to {dto.Status}");

        schedule.Status = dto.Status;
        schedule.UpdatedBy = userId;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Cascade PWO status
        var pwo = await _db.ProductionWorkOrders.FindAsync(schedule.ProductionWorkOrderId);
        if (pwo != null)
        {
            if (dto.Status == "InProgress" && pwo.Status == "Scheduled")
            {
                pwo.Status = "InProgress";
                pwo.UpdatedBy = userId;
                pwo.UpdatedAt = DateTime.UtcNow;
            }
            else if (dto.Status == "Completed")
            {
                // Check if all schedules for this PWO are completed
                var allComplete = await _db.ProductionSchedules
                    .Where(s => !s.IsDeleted &&
                        s.ProductionWorkOrderId == pwo.Id &&
                        s.Id != id)
                    .AllAsync(s => s.Status == "Completed");

                if (allComplete)
                {
                    pwo.Status = "Complete";
                    pwo.UpdatedBy = userId;
                    pwo.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var schedule = await _db.ProductionSchedules.FindAsync(id)
            ?? throw new KeyNotFoundException($"Schedule entry with ID {id} not found.");

        schedule.IsDeleted = true;
        schedule.UpdatedBy = userId;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderScheduleDto dto, string userId)
    {
        var entries = await _db.ProductionSchedules
            .Where(s => dto.ScheduleIds.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync();

        // Validate all belong to same date/shift/unit
        foreach (var entry in entries)
        {
            if (entry.Date != dto.Date || entry.Shift != dto.Shift || entry.ProductionUnitId != dto.ProductionUnitId)
                throw new InvalidOperationException("All schedule entries must belong to the same date, shift, and unit.");
        }

        if (entries.Count != dto.ScheduleIds.Count)
            throw new ArgumentException("One or more schedule entries not found.");

        // Apply new sort order
        for (int i = 0; i < dto.ScheduleIds.Count; i++)
        {
            var entry = entries.First(e => e.Id == dto.ScheduleIds[i]);
            entry.SortOrder = i + 1;
            entry.UpdatedBy = userId;
            entry.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
