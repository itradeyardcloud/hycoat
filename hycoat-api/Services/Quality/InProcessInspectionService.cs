using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Quality;

public class InProcessInspectionService : IInProcessInspectionService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    private const decimal DFT_MIN = 60m;
    private const decimal DFT_MAX = 80m;

    public InProcessInspectionService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<InProcessInspectionDto>> GetAllAsync(
        string? search, DateTime? date, int? productionWorkOrderId, string? inspectorUserId,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.InProcessInspections
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(i => i.InspectorUser)
            .Include(i => i.DFTReadings)
            .Include(i => i.TestResults)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.ProductionWorkOrder.PWONumber.ToLower().Contains(term) ||
                i.ProductionWorkOrder.Customer.Name.ToLower().Contains(term));
        }

        if (date.HasValue)
            query = query.Where(i => i.Date == date.Value.Date);

        if (productionWorkOrderId.HasValue)
            query = query.Where(i => i.ProductionWorkOrderId == productionWorkOrderId.Value);

        if (!string.IsNullOrWhiteSpace(inspectorUserId))
            query = query.Where(i => i.InspectorUserId == inspectorUserId);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "pwonumber" or "pwo" => sortDesc
                ? query.OrderByDescending(i => i.ProductionWorkOrder.PWONumber)
                : query.OrderBy(i => i.ProductionWorkOrder.PWONumber),
            "time" => sortDesc
                ? query.OrderByDescending(i => i.Date).ThenByDescending(i => i.Time)
                : query.OrderBy(i => i.Date).ThenBy(i => i.Time),
            _ => sortDesc
                ? query.OrderByDescending(i => i.Date).ThenByDescending(i => i.Time)
                : query.OrderBy(i => i.Date).ThenBy(i => i.Time),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<InProcessInspectionDto>
        {
            Items = _mapper.Map<List<InProcessInspectionDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InProcessInspectionDetailDto> GetByIdAsync(int id)
    {
        var inspection = await _db.InProcessInspections
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(i => i.InspectorUser)
            .Include(i => i.DFTReadings).ThenInclude(r => r.SectionProfile)
            .Include(i => i.TestResults)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"In-Process Inspection with ID {id} not found.");

        return _mapper.Map<InProcessInspectionDetailDto>(inspection);
    }

    public async Task<List<InProcessInspectionDto>> GetByPWOAsync(int pwoId)
    {
        var inspections = await _db.InProcessInspections
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(i => i.InspectorUser)
            .Include(i => i.DFTReadings)
            .Include(i => i.TestResults)
            .Where(i => i.ProductionWorkOrderId == pwoId)
            .OrderByDescending(i => i.Date).ThenByDescending(i => i.Time)
            .ToListAsync();

        return _mapper.Map<List<InProcessInspectionDto>>(inspections);
    }

    public async Task<List<DFTTrendDto>> GetDFTTrendAsync(int pwoId)
    {
        return await _db.InProcessDFTReadings
            .AsNoTracking()
            .Where(r => r.InProcessInspection.ProductionWorkOrderId == pwoId)
            .OrderBy(r => r.InProcessInspection.Date)
            .ThenBy(r => r.InProcessInspection.Time)
            .Select(r => new DFTTrendDto
            {
                Date = r.InProcessInspection.Date,
                Time = r.InProcessInspection.Time,
                AvgReading = r.AvgReading,
                MinReading = r.MinReading,
                MaxReading = r.MaxReading,
                IsWithinSpec = r.IsWithinSpec
            })
            .ToListAsync();
    }

    public async Task<InProcessInspectionDto> CreateAsync(CreateInProcessInspectionDto dto, string userId)
    {
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress to create an inspection.");

        var inspection = _mapper.Map<InProcessInspection>(dto);
        inspection.Date = dto.Date.Date;
        inspection.InspectorUserId = userId;
        inspection.CreatedBy = userId;

        // Add DFT readings with computed fields
        foreach (var readingDto in dto.DFTReadings)
        {
            var reading = _mapper.Map<InProcessDFTReading>(readingDto);
            ComputeDFTStats(reading);
            inspection.DFTReadings.Add(reading);
        }

        // Add test results
        foreach (var testDto in dto.TestResults)
        {
            var test = _mapper.Map<InProcessTestResult>(testDto);
            inspection.TestResults.Add(test);
        }

        _db.InProcessInspections.Add(inspection);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var created = await _db.InProcessInspections
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(i => i.InspectorUser)
            .Include(i => i.DFTReadings)
            .Include(i => i.TestResults)
            .FirstAsync(i => i.Id == inspection.Id);

        return _mapper.Map<InProcessInspectionDto>(created);
    }

    public async Task<InProcessInspectionDto> UpdateAsync(int id, CreateInProcessInspectionDto dto, string userId)
    {
        var inspection = await _db.InProcessInspections
            .Include(i => i.DFTReadings)
            .Include(i => i.TestResults)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"In-Process Inspection with ID {id} not found.");

        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress.");

        inspection.Date = dto.Date.Date;
        inspection.Time = dto.Time;
        inspection.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        inspection.Remarks = dto.Remarks;
        inspection.UpdatedBy = userId;
        inspection.UpdatedAt = DateTime.UtcNow;

        // Replace DFT readings
        _db.InProcessDFTReadings.RemoveRange(inspection.DFTReadings);
        foreach (var readingDto in dto.DFTReadings)
        {
            var reading = _mapper.Map<InProcessDFTReading>(readingDto);
            reading.InProcessInspectionId = id;
            ComputeDFTStats(reading);
            _db.InProcessDFTReadings.Add(reading);
        }

        // Replace test results
        _db.InProcessTestResults.RemoveRange(inspection.TestResults);
        foreach (var testDto in dto.TestResults)
        {
            var test = _mapper.Map<InProcessTestResult>(testDto);
            test.InProcessInspectionId = id;
            _db.InProcessTestResults.Add(test);
        }

        await _db.SaveChangesAsync();

        var updated = await _db.InProcessInspections
            .AsNoTracking()
            .Include(i => i.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(i => i.InspectorUser)
            .Include(i => i.DFTReadings)
            .Include(i => i.TestResults)
            .FirstAsync(i => i.Id == id);

        return _mapper.Map<InProcessInspectionDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var inspection = await _db.InProcessInspections.FindAsync(id)
            ?? throw new KeyNotFoundException($"In-Process Inspection with ID {id} not found.");

        inspection.IsDeleted = true;
        inspection.UpdatedBy = userId;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private void ComputeDFTStats(InProcessDFTReading reading)
    {
        var values = new[] { reading.S1, reading.S2, reading.S3, reading.S4, reading.S5,
                             reading.S6, reading.S7, reading.S8, reading.S9, reading.S10 }
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (values.Count > 0)
        {
            reading.MinReading = values.Min();
            reading.MaxReading = values.Max();
            reading.AvgReading = Math.Round(values.Average(), 2);
            reading.IsWithinSpec = values.All(v => v >= DFT_MIN && v <= DFT_MAX);
        }
        else
        {
            reading.MinReading = null;
            reading.MaxReading = null;
            reading.AvgReading = null;
            reading.IsWithinSpec = true;
        }
    }
}
