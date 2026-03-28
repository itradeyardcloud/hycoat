using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Production;
using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Production;

public class PretreatmentLogService : IPretreatmentLogService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    private static readonly string[] TankSequence =
    [
        "Degreasing",
        "Water Rinse 1",
        "Etching",
        "Water Rinse 2",
        "Deoxidizing / De-smutting",
        "Water Rinse 3",
        "Chrome Conversion Coating",
        "Water Rinse 4",
        "DI Water Rinse",
        "Oven Dry"
    ];

    public PretreatmentLogService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PretreatmentLogDto>> GetAllAsync(
        string? search, DateTime? date, string? shift, int? productionWorkOrderId,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.PretreatmentLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.OperatorUser)
            .Include(l => l.QASignOffUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(l =>
                l.ProductionWorkOrder.PWONumber.ToLower().Contains(term) ||
                l.ProductionWorkOrder.Customer.Name.ToLower().Contains(term));
        }

        if (date.HasValue)
            query = query.Where(l => l.Date == date.Value.Date);

        if (!string.IsNullOrWhiteSpace(shift))
            query = query.Where(l => l.Shift == shift);

        if (productionWorkOrderId.HasValue)
            query = query.Where(l => l.ProductionWorkOrderId == productionWorkOrderId.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "basketnumber" or "basket" => sortDesc
                ? query.OrderByDescending(l => l.BasketNumber)
                : query.OrderBy(l => l.BasketNumber),
            "pwonumber" or "pwo" => sortDesc
                ? query.OrderByDescending(l => l.ProductionWorkOrder.PWONumber)
                : query.OrderBy(l => l.ProductionWorkOrder.PWONumber),
            "shift" => sortDesc
                ? query.OrderByDescending(l => l.Shift)
                : query.OrderBy(l => l.Shift),
            _ => sortDesc
                ? query.OrderByDescending(l => l.Date).ThenByDescending(l => l.BasketNumber)
                : query.OrderBy(l => l.Date).ThenBy(l => l.BasketNumber),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PretreatmentLogDto>
        {
            Items = _mapper.Map<List<PretreatmentLogDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PretreatmentLogDetailDto> GetByIdAsync(int id)
    {
        var log = await _db.PretreatmentLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.OperatorUser)
            .Include(l => l.QASignOffUser)
            .Include(l => l.TankReadings)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Pretreatment Log with ID {id} not found.");

        return _mapper.Map<PretreatmentLogDetailDto>(log);
    }

    public async Task<List<PretreatmentLogDto>> GetByPWOAsync(int pwoId)
    {
        var logs = await _db.PretreatmentLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.OperatorUser)
            .Include(l => l.QASignOffUser)
            .Where(l => l.ProductionWorkOrderId == pwoId)
            .OrderBy(l => l.BasketNumber)
            .ToListAsync();

        return _mapper.Map<List<PretreatmentLogDto>>(logs);
    }

    public async Task<PretreatmentLogDto> CreateAsync(CreatePretreatmentLogDto dto, string userId)
    {
        // Validate PWO exists and is InProgress
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress to create a pretreatment log.");

        var log = _mapper.Map<PretreatmentLog>(dto);
        log.OperatorUserId = userId;
        log.CreatedBy = userId;

        // Pre-populate tank readings from DTO or create empty ones
        if (dto.TankReadings.Count > 0)
        {
            foreach (var readingDto in dto.TankReadings)
            {
                var reading = _mapper.Map<PretreatmentTankReading>(readingDto);
                log.TankReadings.Add(reading);
            }
        }
        else
        {
            // Auto-populate 10 empty tank reading records
            foreach (var tankName in TankSequence)
            {
                log.TankReadings.Add(new PretreatmentTankReading { TankName = tankName });
            }
        }

        _db.PretreatmentLogs.Add(log);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var created = await _db.PretreatmentLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.OperatorUser)
            .Include(l => l.QASignOffUser)
            .FirstAsync(l => l.Id == log.Id);

        return _mapper.Map<PretreatmentLogDto>(created);
    }

    public async Task<PretreatmentLogDto> UpdateAsync(int id, CreatePretreatmentLogDto dto, string userId)
    {
        var log = await _db.PretreatmentLogs
            .Include(l => l.TankReadings)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Pretreatment Log with ID {id} not found.");

        // Validate PWO
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress.");

        // Update fields
        log.Date = dto.Date;
        log.Shift = dto.Shift;
        log.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        log.BasketNumber = dto.BasketNumber;
        log.StartTime = dto.StartTime;
        log.EndTime = dto.EndTime;
        log.EtchTimeMins = dto.EtchTimeMins;
        log.Remarks = dto.Remarks;
        log.UpdatedBy = userId;
        log.UpdatedAt = DateTime.UtcNow;

        // Replace tank readings
        if (dto.TankReadings.Count > 0)
        {
            _db.PretreatmentTankReadings.RemoveRange(log.TankReadings);
            foreach (var readingDto in dto.TankReadings)
            {
                var reading = _mapper.Map<PretreatmentTankReading>(readingDto);
                reading.PretreatmentLogId = log.Id;
                _db.PretreatmentTankReadings.Add(reading);
            }
        }

        await _db.SaveChangesAsync();

        var updated = await _db.PretreatmentLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.OperatorUser)
            .Include(l => l.QASignOffUser)
            .FirstAsync(l => l.Id == id);

        return _mapper.Map<PretreatmentLogDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var log = await _db.PretreatmentLogs.FindAsync(id)
            ?? throw new KeyNotFoundException($"Pretreatment Log with ID {id} not found.");

        log.IsDeleted = true;
        log.UpdatedBy = userId;
        log.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task AddTankReadingsAsync(int id, List<TankReadingDto> readings, string userId)
    {
        var log = await _db.PretreatmentLogs
            .Include(l => l.TankReadings)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Pretreatment Log with ID {id} not found.");

        // Upsert: update existing by TankName or add new
        foreach (var readingDto in readings)
        {
            var existing = log.TankReadings.FirstOrDefault(r => r.TankName == readingDto.TankName);
            if (existing != null)
            {
                existing.Concentration = readingDto.Concentration;
                existing.Temperature = readingDto.Temperature;
                existing.ChemicalAdded = readingDto.ChemicalAdded;
                existing.ChemicalQty = readingDto.ChemicalQty;
            }
            else
            {
                var reading = _mapper.Map<PretreatmentTankReading>(readingDto);
                reading.PretreatmentLogId = id;
                _db.PretreatmentTankReadings.Add(reading);
            }
        }

        log.UpdatedBy = userId;
        log.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
