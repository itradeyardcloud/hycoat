using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Sales;

public class ProformaInvoiceService : IProformaInvoiceService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly PIPdfService _pdfService;

    private const decimal GST_RATE = 0.09m; // 9% each for CGST/SGST
    private const decimal IGST_RATE = 0.18m; // 18% IGST
    private const decimal SQ_MM_TO_SQ_FT = 92903.04m;

    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["Draft"] = ["Sent"],
        ["Sent"] = ["Accepted", "Rejected"],
    };

    public ProformaInvoiceService(AppDbContext db, IMapper mapper, PIPdfService pdfService)
    {
        _db = db;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    public async Task<PagedResponse<PIDto>> GetAllAsync(
        string? search, string? status, int? customerId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.ProformaInvoices
            .AsNoTracking()
            .Include(pi => pi.Customer)
            .Include(pi => pi.Quotation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(pi =>
                pi.PINumber.ToLower().Contains(term) ||
                pi.Customer.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(pi => pi.Status == status);

        if (customerId.HasValue)
            query = query.Where(pi => pi.CustomerId == customerId.Value);

        if (fromDate.HasValue)
            query = query.Where(pi => pi.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(pi => pi.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "pinumber" => sortDesc ? query.OrderByDescending(pi => pi.PINumber) : query.OrderBy(pi => pi.PINumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(pi => pi.Customer.Name) : query.OrderBy(pi => pi.Customer.Name),
            "grandtotal" => sortDesc ? query.OrderByDescending(pi => pi.GrandTotal) : query.OrderBy(pi => pi.GrandTotal),
            "status" => sortDesc ? query.OrderByDescending(pi => pi.Status) : query.OrderBy(pi => pi.Status),
            _ => sortDesc ? query.OrderByDescending(pi => pi.Date) : query.OrderBy(pi => pi.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pi => new PIDto
            {
                Id = pi.Id,
                PINumber = pi.PINumber,
                Date = pi.Date,
                CustomerName = pi.Customer.Name,
                QuotationNumber = pi.Quotation != null ? pi.Quotation.QuotationNumber : null,
                GrandTotal = pi.GrandTotal,
                Status = pi.Status
            })
            .ToListAsync();

        return new PagedResponse<PIDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PIDetailDto> GetByIdAsync(int id)
    {
        var pi = await _db.ProformaInvoices
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Quotation)
            .Include(p => p.PreparedByUser)
            .Include(p => p.LineItems).ThenInclude(li => li.SectionProfile)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Proforma Invoice with ID {id} not found.");

        return _mapper.Map<PIDetailDto>(pi);
    }

    public async Task<PIDto> CreateAsync(CreateProformaInvoiceDto dto, string userId)
    {
        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new ArgumentException("Customer not found.");

        if (dto.QuotationId.HasValue)
        {
            var qtnExists = await _db.Quotations.AnyAsync(q => q.Id == dto.QuotationId.Value);
            if (!qtnExists)
                throw new ArgumentException("Quotation not found.");
        }

        // Fetch all section profiles needed
        var sectionIds = dto.LineItems.Select(li => li.SectionProfileId).Distinct().ToList();
        var sections = await _db.SectionProfiles
            .Where(sp => sectionIds.Contains(sp.Id))
            .ToDictionaryAsync(sp => sp.Id);

        if (sections.Count != sectionIds.Count)
            throw new ArgumentException("One or more section profiles not found.");

        var pi = _mapper.Map<ProformaInvoice>(dto);
        pi.PINumber = await GenerateNumberAsync();
        pi.Status = "Draft";
        pi.PreparedByUserId = userId;
        pi.CreatedBy = userId;

        // Snapshot customer data
        pi.CustomerAddress = $"{customer.Address}, {customer.City}, {customer.State} {customer.Pincode}".Trim(' ', ',');
        pi.CustomerGSTIN = customer.GSTIN;

        // Calculate line items
        decimal subTotal = 0;
        foreach (var lineDto in dto.LineItems)
        {
            var section = sections[lineDto.SectionProfileId];
            var line = new PILineItem
            {
                SectionProfileId = lineDto.SectionProfileId,
                SectionNumber = section.SectionNumber,
                LengthMM = lineDto.LengthMM,
                Quantity = lineDto.Quantity,
                PerimeterMM = section.PerimeterMM,
                RatePerSFT = lineDto.RatePerSFT,
            };
            CalculateLineArea(line);
            subTotal += line.Amount;
            pi.LineItems.Add(line);
        }

        // Calculate totals
        CalculateTotals(pi, subTotal);

        _db.ProformaInvoices.Add(pi);
        await _db.SaveChangesAsync();

        var created = await _db.ProformaInvoices
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Quotation)
            .FirstAsync(p => p.Id == pi.Id);

        return _mapper.Map<PIDto>(created);
    }

    public async Task<PIDto> UpdateAsync(int id, CreateProformaInvoiceDto dto, string userId)
    {
        var pi = await _db.ProformaInvoices
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Proforma Invoice with ID {id} not found.");

        if (pi.Status != "Draft")
            throw new InvalidOperationException("Only Draft proforma invoices can be edited.");

        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new ArgumentException("Customer not found.");

        if (dto.QuotationId.HasValue)
        {
            var qtnExists = await _db.Quotations.AnyAsync(q => q.Id == dto.QuotationId.Value);
            if (!qtnExists)
                throw new ArgumentException("Quotation not found.");
        }

        var sectionIds = dto.LineItems.Select(li => li.SectionProfileId).Distinct().ToList();
        var sections = await _db.SectionProfiles
            .Where(sp => sectionIds.Contains(sp.Id))
            .ToDictionaryAsync(sp => sp.Id);

        if (sections.Count != sectionIds.Count)
            throw new ArgumentException("One or more section profiles not found.");

        pi.Date = dto.Date;
        pi.CustomerId = dto.CustomerId;
        pi.QuotationId = dto.QuotationId;
        pi.PackingCharges = dto.PackingCharges;
        pi.TransportCharges = dto.TransportCharges;
        pi.IsInterState = dto.IsInterState;
        pi.Notes = dto.Notes;
        pi.CustomerAddress = $"{customer.Address}, {customer.City}, {customer.State} {customer.Pincode}".Trim(' ', ',');
        pi.CustomerGSTIN = customer.GSTIN;
        pi.UpdatedBy = userId;
        pi.UpdatedAt = DateTime.UtcNow;

        // Clear and re-add line items
        pi.LineItems.Clear();
        decimal subTotal = 0;
        foreach (var lineDto in dto.LineItems)
        {
            var section = sections[lineDto.SectionProfileId];
            var line = new PILineItem
            {
                SectionProfileId = lineDto.SectionProfileId,
                SectionNumber = section.SectionNumber,
                LengthMM = lineDto.LengthMM,
                Quantity = lineDto.Quantity,
                PerimeterMM = section.PerimeterMM,
                RatePerSFT = lineDto.RatePerSFT,
            };
            CalculateLineArea(line);
            subTotal += line.Amount;
            pi.LineItems.Add(line);
        }

        CalculateTotals(pi, subTotal);

        await _db.SaveChangesAsync();

        var updated = await _db.ProformaInvoices
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Quotation)
            .FirstAsync(p => p.Id == id);

        return _mapper.Map<PIDto>(updated);
    }

    public async Task UpdateStatusAsync(int id, UpdatePIStatusDto dto, string userId)
    {
        var pi = await _db.ProformaInvoices.FindAsync(id)
            ?? throw new KeyNotFoundException($"Proforma Invoice with ID {id} not found.");

        if (!StatusTransitions.TryGetValue(pi.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Cannot transition from '{pi.Status}' to '{dto.Status}'.");

        pi.Status = dto.Status;
        pi.UpdatedBy = userId;
        pi.UpdatedAt = DateTime.UtcNow;

        // When PI is sent, update linked inquiry to PISent (via quotation chain)
        if (dto.Status == "Sent" && pi.QuotationId.HasValue)
        {
            var quotation = await _db.Quotations.FindAsync(pi.QuotationId.Value);
            if (quotation?.InquiryId != null)
            {
                var inquiry = await _db.Inquiries.FindAsync(quotation.InquiryId.Value);
                if (inquiry != null && inquiry.Status == "BOMReceived")
                {
                    inquiry.Status = "PISent";
                    inquiry.UpdatedBy = userId;
                    inquiry.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var pi = await _db.ProformaInvoices.FindAsync(id)
            ?? throw new KeyNotFoundException($"Proforma Invoice with ID {id} not found.");

        pi.IsDeleted = true;
        pi.UpdatedBy = userId;
        pi.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var pi = await _db.ProformaInvoices
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Quotation)
            .Include(p => p.PreparedByUser)
            .Include(p => p.LineItems).ThenInclude(li => li.SectionProfile)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Proforma Invoice with ID {id} not found.");

        var detail = _mapper.Map<PIDetailDto>(pi);
        var pdf = _pdfService.Generate(detail);

        var uploadsDir = Path.Combine("wwwroot", "uploads", "proforma-invoices");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{pi.PINumber}.pdf";
        var filePath = Path.Combine(uploadsDir, fileName);
        await File.WriteAllBytesAsync(filePath, pdf);

        pi = await _db.ProformaInvoices.FindAsync(id);
        if (pi != null)
        {
            pi.FileUrl = $"/uploads/proforma-invoices/{fileName}";
            await _db.SaveChangesAsync();
        }

        return pdf;
    }

    public async Task<CalculateAreaResponseDto> CalculateAreaAsync(CalculateAreaRequestDto dto)
    {
        var sectionIds = dto.Lines.Select(l => l.SectionProfileId).Distinct().ToList();
        var sections = await _db.SectionProfiles
            .Where(sp => sectionIds.Contains(sp.Id))
            .ToDictionaryAsync(sp => sp.Id);

        var response = new CalculateAreaResponseDto();
        foreach (var line in dto.Lines)
        {
            if (!sections.TryGetValue(line.SectionProfileId, out var section))
                throw new ArgumentException($"Section profile with ID {line.SectionProfileId} not found.");

            var areaSFT = (section.PerimeterMM * line.LengthMM * line.Quantity) / SQ_MM_TO_SQ_FT;
            response.Lines.Add(new AreaCalcResultDto
            {
                SectionProfileId = section.Id,
                SectionNumber = section.SectionNumber,
                PerimeterMM = section.PerimeterMM,
                LengthMM = line.LengthMM,
                Quantity = line.Quantity,
                AreaSFT = Math.Round(areaSFT, 2)
            });
        }
        response.TotalAreaSFT = response.Lines.Sum(l => l.AreaSFT);
        return response;
    }

    public async Task<List<LookupDto>> GetLookupAsync()
    {
        return await _db.ProformaInvoices
            .AsNoTracking()
            .Where(pi => pi.Status != "Rejected")
            .OrderByDescending(pi => pi.Date)
            .Select(pi => new LookupDto { Id = pi.Id, Name = pi.PINumber })
            .ToListAsync();
    }

    private async Task<string> GenerateNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PI-{year}-";

        var lastNumber = await _db.ProformaInvoices
            .Where(pi => pi.PINumber.StartsWith(prefix))
            .OrderByDescending(pi => pi.PINumber)
            .Select(pi => pi.PINumber)
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

    private static void CalculateLineArea(PILineItem line)
    {
        line.AreaSFT = Math.Round((line.PerimeterMM * line.LengthMM * line.Quantity) / SQ_MM_TO_SQ_FT, 2);
        line.AreaSqMtr = Math.Round(line.AreaSFT * 0.092903m, 4);
        line.Amount = Math.Round(line.AreaSFT * line.RatePerSFT, 2);
    }

    private static void CalculateTotals(ProformaInvoice pi, decimal subTotal)
    {
        pi.SubTotal = Math.Round(subTotal, 2);
        pi.TaxableAmount = Math.Round(pi.SubTotal + pi.PackingCharges + pi.TransportCharges, 2);

        if (pi.IsInterState)
        {
            pi.CGSTRate = 0;
            pi.CGSTAmount = 0;
            pi.SGSTRate = 0;
            pi.SGSTAmount = 0;
            pi.IGSTRate = 18;
            pi.IGSTAmount = Math.Round(pi.TaxableAmount * IGST_RATE, 2);
        }
        else
        {
            pi.CGSTRate = 9;
            pi.CGSTAmount = Math.Round(pi.TaxableAmount * GST_RATE, 2);
            pi.SGSTRate = 9;
            pi.SGSTAmount = Math.Round(pi.TaxableAmount * GST_RATE, 2);
            pi.IGSTRate = 0;
            pi.IGSTAmount = 0;
        }

        pi.GrandTotal = Math.Round(pi.TaxableAmount + pi.CGSTAmount + pi.SGSTAmount + pi.IGSTAmount, 2);
    }
}
