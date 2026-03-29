using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Sales;

public class QuotationService : IQuotationService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly QuotationPdfService _pdfService;

    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["Draft"] = ["Sent"],
        ["Sent"] = ["Accepted", "Rejected"],
        // Accepted, Rejected, Expired are terminal
    };

    public QuotationService(AppDbContext db, IMapper mapper, QuotationPdfService pdfService)
    {
        _db = db;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    public async Task<PagedResponse<QuotationDto>> GetAllAsync(
        string? search, string? status, int? customerId, int? inquiryId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.Inquiry)
            .Include(q => q.LineItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(q =>
                q.QuotationNumber.ToLower().Contains(term) ||
                q.Customer.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(q => q.Status == status);

        if (customerId.HasValue)
            query = query.Where(q => q.CustomerId == customerId.Value);

        if (inquiryId.HasValue)
            query = query.Where(q => q.InquiryId == inquiryId.Value);

        if (fromDate.HasValue)
            query = query.Where(q => q.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(q => q.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "quotationnumber" => sortDesc ? query.OrderByDescending(q => q.QuotationNumber) : query.OrderBy(q => q.QuotationNumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(q => q.Customer.Name) : query.OrderBy(q => q.Customer.Name),
            "status" => sortDesc ? query.OrderByDescending(q => q.Status) : query.OrderBy(q => q.Status),
            _ => sortDesc ? query.OrderByDescending(q => q.Date) : query.OrderBy(q => q.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuotationDto
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                Date = q.Date,
                CustomerName = q.Customer.Name,
                InquiryNumber = q.Inquiry != null ? q.Inquiry.InquiryNumber : null,
                Status = IsExpired(q) ? "Expired" : q.Status,
                ValidityDays = q.ValidityDays,
                LineItemCount = q.LineItems.Count
            })
            .ToListAsync();

        return new PagedResponse<QuotationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<QuotationDetailDto> GetByIdAsync(int id)
    {
        var quotation = await _db.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.Inquiry)
            .Include(q => q.PreparedByUser)
            .Include(q => q.LineItems).ThenInclude(li => li.ProcessType)
            .FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new KeyNotFoundException($"Quotation with ID {id} not found.");

        var dto = _mapper.Map<QuotationDetailDto>(quotation);

        // Check-on-read expiry
        if (IsExpired(quotation))
            dto.Status = "Expired";

        return dto;
    }

    public async Task<QuotationDto> CreateAsync(CreateQuotationDto dto, string userId)
    {
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        if (dto.InquiryId.HasValue)
        {
            var inquiryExists = await _db.Inquiries.AnyAsync(i => i.Id == dto.InquiryId.Value);
            if (!inquiryExists)
                throw new ArgumentException("Inquiry not found.");
        }

        // Validate all process types exist
        var processTypeIds = dto.LineItems.Select(li => li.ProcessTypeId).Distinct().ToList();
        var existingPTCount = await _db.ProcessTypes.CountAsync(pt => processTypeIds.Contains(pt.Id));
        if (existingPTCount != processTypeIds.Count)
            throw new ArgumentException("One or more process types not found.");

        var quotation = _mapper.Map<Quotation>(dto);
        quotation.QuotationNumber = await GenerateNumberAsync();
        quotation.Status = "Draft";
        quotation.PreparedByUserId = userId;
        quotation.CreatedBy = userId;

        foreach (var lineDto in dto.LineItems)
        {
            var line = _mapper.Map<QuotationLineItem>(lineDto);
            quotation.LineItems.Add(line);
        }

        _db.Quotations.Add(quotation);
        await _db.SaveChangesAsync();

        var created = await _db.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.Inquiry)
            .Include(q => q.LineItems)
            .FirstAsync(q => q.Id == quotation.Id);

        return _mapper.Map<QuotationDto>(created);
    }

    public async Task<QuotationDto> UpdateAsync(int id, CreateQuotationDto dto, string userId)
    {
        var quotation = await _db.Quotations
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new KeyNotFoundException($"Quotation with ID {id} not found.");

        if (quotation.Status != "Draft")
            throw new InvalidOperationException("Only Draft quotations can be edited.");

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        if (dto.InquiryId.HasValue)
        {
            var inquiryExists = await _db.Inquiries.AnyAsync(i => i.Id == dto.InquiryId.Value);
            if (!inquiryExists)
                throw new ArgumentException("Inquiry not found.");
        }

        var processTypeIds = dto.LineItems.Select(li => li.ProcessTypeId).Distinct().ToList();
        var existingPTCount = await _db.ProcessTypes.CountAsync(pt => processTypeIds.Contains(pt.Id));
        if (existingPTCount != processTypeIds.Count)
            throw new ArgumentException("One or more process types not found.");

        quotation.Date = dto.Date;
        quotation.InquiryId = dto.InquiryId;
        quotation.CustomerId = dto.CustomerId;
        quotation.ValidityDays = dto.ValidityDays;
        quotation.Notes = dto.Notes;
        quotation.UpdatedBy = userId;
        quotation.UpdatedAt = DateTime.UtcNow;

        // Clear and re-add line items
        quotation.LineItems.Clear();
        foreach (var lineDto in dto.LineItems)
        {
            var line = _mapper.Map<QuotationLineItem>(lineDto);
            quotation.LineItems.Add(line);
        }

        await _db.SaveChangesAsync();

        var updated = await _db.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.Inquiry)
            .Include(q => q.LineItems)
            .FirstAsync(q => q.Id == id);

        return _mapper.Map<QuotationDto>(updated);
    }

    public async Task UpdateStatusAsync(int id, UpdateQuotationStatusDto dto, string userId)
    {
        var quotation = await _db.Quotations.FindAsync(id)
            ?? throw new KeyNotFoundException($"Quotation with ID {id} not found.");

        if (!StatusTransitions.TryGetValue(quotation.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Cannot transition from '{quotation.Status}' to '{dto.Status}'.");

        quotation.Status = dto.Status;
        quotation.UpdatedBy = userId;
        quotation.UpdatedAt = DateTime.UtcNow;

        // When quotation is sent, update linked inquiry status
        if (dto.Status == "Sent" && quotation.InquiryId.HasValue)
        {
            var inquiry = await _db.Inquiries.FindAsync(quotation.InquiryId.Value);
            if (inquiry != null && inquiry.Status == "New")
            {
                inquiry.Status = "QuotationSent";
                inquiry.UpdatedBy = userId;
                inquiry.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var quotation = await _db.Quotations.FindAsync(id)
            ?? throw new KeyNotFoundException($"Quotation with ID {id} not found.");

        quotation.IsDeleted = true;
        quotation.UpdatedBy = userId;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var quotation = await _db.Quotations
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.Inquiry)
            .Include(q => q.PreparedByUser)
            .Include(q => q.LineItems).ThenInclude(li => li.ProcessType)
            .FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new KeyNotFoundException($"Quotation with ID {id} not found.");

        var detail = _mapper.Map<QuotationDetailDto>(quotation);
        var pdf = _pdfService.Generate(detail);

        // Save PDF to disk
        var uploadsDir = Path.Combine("wwwroot", "uploads", "quotations");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{quotation.QuotationNumber}.pdf";
        var filePath = Path.Combine(uploadsDir, fileName);
        await File.WriteAllBytesAsync(filePath, pdf);

        // Update FileUrl
        quotation = await _db.Quotations.FindAsync(id);
        if (quotation != null)
        {
            quotation.FileUrl = $"/uploads/quotations/{fileName}";
            await _db.SaveChangesAsync();
        }

        return pdf;
    }

    public async Task<List<LookupDto>> GetLookupAsync()
    {
        return await _db.Quotations
            .AsNoTracking()
            .Where(q => q.Status != "Rejected" && q.Status != "Expired")
            .OrderByDescending(q => q.Date)
            .Select(q => new LookupDto { Id = q.Id, Name = q.QuotationNumber })
            .ToListAsync();
    }

    private async Task<string> GenerateNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"QTN-{year}-";

        var lastNumber = await _db.Quotations
            .Where(q => q.QuotationNumber.StartsWith(prefix))
            .OrderByDescending(q => q.QuotationNumber)
            .Select(q => q.QuotationNumber)
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

    private static bool IsExpired(Quotation q)
    {
        return q.Status == "Sent" && q.Date.AddDays(q.ValidityDays) < DateTime.UtcNow;
    }
}
