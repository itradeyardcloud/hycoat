using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Sales;

public class InquiryService : IInquiryService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    // Allowed status transitions: current → [allowed next statuses]
    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["New"] = ["QuotationSent", "Lost"],
        ["QuotationSent"] = ["BOMReceived", "Lost"],
        ["BOMReceived"] = ["PISent", "Lost"],
        ["PISent"] = ["Confirmed", "Lost"],
        ["Confirmed"] = ["Closed"],
        // Lost and Closed are terminal
    };

    public InquiryService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<InquiryDto>> GetAllAsync(
        string? search, string? status, int? customerId,
        string? assignedToUserId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.Inquiries
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.ProcessType)
            .Include(i => i.AssignedToUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.InquiryNumber.ToLower().Contains(term) ||
                i.Customer.Name.ToLower().Contains(term) ||
                (i.ProjectName != null && i.ProjectName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(assignedToUserId))
            query = query.Where(i => i.AssignedToUserId == assignedToUserId);

        if (fromDate.HasValue)
            query = query.Where(i => i.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "inquirynumber" => sortDesc ? query.OrderByDescending(i => i.InquiryNumber) : query.OrderBy(i => i.InquiryNumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(i => i.Customer.Name) : query.OrderBy(i => i.Customer.Name),
            "status" => sortDesc ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            "source" => sortDesc ? query.OrderByDescending(i => i.Source) : query.OrderBy(i => i.Source),
            _ => sortDesc ? query.OrderByDescending(i => i.Date) : query.OrderBy(i => i.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InquiryDto
            {
                Id = i.Id,
                InquiryNumber = i.InquiryNumber,
                Date = i.Date,
                CustomerName = i.Customer.Name,
                ProjectName = i.ProjectName,
                Source = i.Source,
                ProcessTypeName = i.ProcessType != null ? i.ProcessType.Name : null,
                Status = i.Status,
                AssignedToName = i.AssignedToUser != null ? i.AssignedToUser.FullName : null
            })
            .ToListAsync();

        return new PagedResponse<InquiryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InquiryDetailDto> GetByIdAsync(int id)
    {
        var inquiry = await _db.Inquiries
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.ProcessType)
            .Include(i => i.AssignedToUser)
            .Include(i => i.Quotations)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Inquiry with ID {id} not found.");

        return _mapper.Map<InquiryDetailDto>(inquiry);
    }

    public async Task<InquiryDto> CreateAsync(CreateInquiryDto dto, string userId)
    {
        // Validate customer exists
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        // Validate process type if provided
        if (dto.ProcessTypeId.HasValue)
        {
            var ptExists = await _db.ProcessTypes.AnyAsync(pt => pt.Id == dto.ProcessTypeId.Value);
            if (!ptExists)
                throw new ArgumentException("Process type not found.");
        }

        var inquiry = _mapper.Map<Inquiry>(dto);
        inquiry.InquiryNumber = await GenerateInquiryNumberAsync();
        inquiry.Status = "New";
        inquiry.CreatedBy = userId;

        _db.Inquiries.Add(inquiry);
        await _db.SaveChangesAsync();

        // Reload with navigation properties for response mapping
        var created = await _db.Inquiries
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.ProcessType)
            .Include(i => i.AssignedToUser)
            .FirstAsync(i => i.Id == inquiry.Id);

        return _mapper.Map<InquiryDto>(created);
    }

    public async Task<InquiryDto> UpdateAsync(int id, UpdateInquiryDto dto, string userId)
    {
        var inquiry = await _db.Inquiries.FindAsync(id)
            ?? throw new KeyNotFoundException($"Inquiry with ID {id} not found.");

        // Validate customer exists
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        // Validate process type if provided
        if (dto.ProcessTypeId.HasValue)
        {
            var ptExists = await _db.ProcessTypes.AnyAsync(pt => pt.Id == dto.ProcessTypeId.Value);
            if (!ptExists)
                throw new ArgumentException("Process type not found.");
        }

        _mapper.Map(dto, inquiry);
        inquiry.UpdatedBy = userId;
        inquiry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updated = await _db.Inquiries
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.ProcessType)
            .Include(i => i.AssignedToUser)
            .FirstAsync(i => i.Id == id);

        return _mapper.Map<InquiryDto>(updated);
    }

    public async Task UpdateStatusAsync(int id, UpdateInquiryStatusDto dto, string userId)
    {
        var inquiry = await _db.Inquiries.FindAsync(id)
            ?? throw new KeyNotFoundException($"Inquiry with ID {id} not found.");

        // Validate status transition
        if (!StatusTransitions.TryGetValue(inquiry.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Cannot transition from '{inquiry.Status}' to '{dto.Status}'.");

        inquiry.Status = dto.Status;
        inquiry.UpdatedBy = userId;
        inquiry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var inquiry = await _db.Inquiries.FindAsync(id)
            ?? throw new KeyNotFoundException($"Inquiry with ID {id} not found.");

        inquiry.IsDeleted = true;
        inquiry.UpdatedBy = userId;
        inquiry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<InquiryStatsDto> GetStatsAsync()
    {
        var counts = await _db.Inquiries
            .AsNoTracking()
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new InquiryStatsDto
        {
            New = counts.FirstOrDefault(c => c.Status == "New")?.Count ?? 0,
            QuotationSent = counts.FirstOrDefault(c => c.Status == "QuotationSent")?.Count ?? 0,
            BOMReceived = counts.FirstOrDefault(c => c.Status == "BOMReceived")?.Count ?? 0,
            PISent = counts.FirstOrDefault(c => c.Status == "PISent")?.Count ?? 0,
            Confirmed = counts.FirstOrDefault(c => c.Status == "Confirmed")?.Count ?? 0,
            Lost = counts.FirstOrDefault(c => c.Status == "Lost")?.Count ?? 0,
            Closed = counts.FirstOrDefault(c => c.Status == "Closed")?.Count ?? 0,
            Total = counts.Sum(c => c.Count)
        };
    }

    public async Task<List<LookupDto>> GetLookupAsync()
    {
        return await _db.Inquiries
            .AsNoTracking()
            .Where(i => i.Status != "Lost" && i.Status != "Closed")
            .OrderByDescending(i => i.Date)
            .Select(i => new LookupDto { Id = i.Id, Name = i.InquiryNumber })
            .ToListAsync();
    }

    private async Task<string> GenerateInquiryNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INQ-{year}-";

        var lastNumber = await _db.Inquiries
            .IgnoreQueryFilters()
            .Where(i => i.InquiryNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InquiryNumber)
            .Select(i => i.InquiryNumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                nextSeq = lastSeq + 1;
        }

        return $"{prefix}{nextSeq:D3}";
    }
}
