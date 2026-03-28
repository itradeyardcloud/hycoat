using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Models.Dispatch;
using HycoatApi.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Dispatch;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public InvoiceService(AppDbContext db, IMapper mapper, IConfiguration config)
    {
        _db = db;
        _mapper = mapper;
        _config = config;
    }

    public async Task<PagedResponse<InvoiceDto>> GetAllAsync(
        string? search, DateTime? dateFrom, DateTime? dateTo, int? customerId, string? status,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.LineItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term) ||
                i.Customer.Name.ToLower().Contains(term) ||
                i.WorkOrder.WONumber.ToLower().Contains(term));
        }

        if (dateFrom.HasValue)
            query = query.Where(i => i.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(i => i.Date <= dateTo.Value.Date);

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "invoicenumber" => sortDesc
                ? query.OrderByDescending(i => i.InvoiceNumber)
                : query.OrderBy(i => i.InvoiceNumber),
            "customer" => sortDesc
                ? query.OrderByDescending(i => i.Customer.Name)
                : query.OrderBy(i => i.Customer.Name),
            "grandtotal" => sortDesc
                ? query.OrderByDescending(i => i.GrandTotal)
                : query.OrderBy(i => i.GrandTotal),
            "status" => sortDesc
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => sortDesc
                ? query.OrderByDescending(i => i.Date)
                : query.OrderBy(i => i.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<InvoiceDto>
        {
            Items = _mapper.Map<List<InvoiceDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InvoiceDetailDto> GetByIdAsync(int id)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.DeliveryChallan)
            .Include(i => i.LineItems).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Invoice with ID {id} not found.");

        return _mapper.Map<InvoiceDetailDto>(invoice);
    }

    public async Task<InvoiceDetailDto?> GetByWorkOrderIdAsync(int woId)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.DeliveryChallan)
            .Include(i => i.LineItems).ThenInclude(l => l.SectionProfile)
            .Where(i => i.WorkOrderId == woId)
            .OrderByDescending(i => i.Date)
            .FirstOrDefaultAsync();

        return invoice == null ? null : _mapper.Map<InvoiceDetailDto>(invoice);
    }

    public async Task<InvoiceAutoFillDto> AutoFillAsync(int woId)
    {
        var workOrder = await _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .FirstOrDefaultAsync(w => w.Id == woId)
            ?? throw new KeyNotFoundException($"Work Order with ID {woId} not found.");

        var dc = await _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.LineItems).ThenInclude(l => l.SectionProfile)
            .Where(d => d.WorkOrderId == woId)
            .OrderByDescending(d => d.Date)
            .FirstOrDefaultAsync();

        var result = new InvoiceAutoFillDto
        {
            CustomerId = workOrder.CustomerId,
            CustomerName = workOrder.Customer.Name,
            CustomerAddress = workOrder.Customer.Address,
            CustomerGSTIN = workOrder.Customer.GSTIN,
            DeliveryChallanId = dc?.Id,
            DCNumber = dc?.DCNumber,
            Lines = []
        };

        if (dc != null)
        {
            foreach (var line in dc.LineItems)
            {
                var perimeter = line.SectionProfile?.PerimeterMM ?? 0;
                var areaSFT = perimeter * line.LengthMM * line.Quantity / 92903.04m;

                result.Lines.Add(new InvoiceLineAutoFillDto
                {
                    SectionProfileId = line.SectionProfileId,
                    SectionNumber = line.SectionProfile?.SectionNumber ?? "",
                    PerimeterMM = perimeter,
                    LengthMM = line.LengthMM,
                    Quantity = line.Quantity,
                    AreaSFT = Math.Round(areaSFT, 2),
                });
            }
        }

        return result;
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, string userId)
    {
        var workOrder = await _db.WorkOrders
            .Include(w => w.Customer)
            .FirstOrDefaultAsync(w => w.Id == dto.WorkOrderId)
            ?? throw new ArgumentException("Work Order not found.");

        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new ArgumentException("Customer not found.");

        // Validate DC exists for the WO
        var hasDC = await _db.DeliveryChallans
            .AnyAsync(d => d.WorkOrderId == dto.WorkOrderId);

        if (!hasDC)
            throw new InvalidOperationException("Cannot create Invoice: A Delivery Challan must exist for this Work Order.");

        // Auto-generate invoice number
        var year = DateTime.UtcNow.Year;
        var lastNumber = await _db.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}-"))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                sequence = lastSeq + 1;
        }

        var invoice = _mapper.Map<Invoice>(dto);
        invoice.InvoiceNumber = $"INV-{year}-{sequence:D3}";
        invoice.Date = dto.Date.Date;
        invoice.Status = "Draft";
        invoice.CreatedBy = userId;

        // Snapshot customer data
        invoice.CustomerName = customer.Name;
        invoice.CustomerAddress = customer.Address;
        invoice.CustomerGSTIN = customer.GSTIN;

        // Company GSTIN from config
        invoice.OurGSTIN = _config["CompanySettings:GSTIN"] ?? "";
        invoice.BankName = _config["CompanySettings:BankName"];
        invoice.BankAccountNo = _config["CompanySettings:BankAccountNo"];
        invoice.BankIFSC = _config["CompanySettings:BankIFSC"];

        // Map and compute line items
        var lineItems = new List<InvoiceLineItem>();
        foreach (var lineDto in dto.Lines)
        {
            var lineItem = _mapper.Map<InvoiceLineItem>(lineDto);

            // Resolve section number
            var section = await _db.SectionProfiles.FindAsync(lineDto.SectionProfileId);
            lineItem.SectionNumber = section?.SectionNumber;

            // Calculate area and amount
            lineItem.AreaSFT = Math.Round(lineDto.PerimeterMM * lineDto.LengthMM * lineDto.Quantity / 92903.04m, 2);
            lineItem.Amount = Math.Round(lineItem.AreaSFT * lineDto.RatePerSFT, 2);

            lineItems.Add(lineItem);
        }

        invoice.LineItems = lineItems;

        // Calculate totals
        CalculateInvoiceTotals(invoice);

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        var created = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.LineItems)
            .FirstAsync(i => i.Id == invoice.Id);

        return _mapper.Map<InvoiceDto>(created);
    }

    public async Task<InvoiceDto> UpdateAsync(int id, CreateInvoiceDto dto, string userId)
    {
        var invoice = await _db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Invoice with ID {id} not found.");

        if (invoice.Status != "Draft")
            throw new InvalidOperationException("Only Draft invoices can be updated.");

        invoice.Date = dto.Date.Date;
        invoice.CustomerId = dto.CustomerId;
        invoice.WorkOrderId = dto.WorkOrderId;
        invoice.DeliveryChallanId = dto.DeliveryChallanId;
        invoice.HSNSACCode = dto.HSNSACCode;
        invoice.PackingCharges = dto.PackingCharges;
        invoice.TransportCharges = dto.TransportCharges;
        invoice.IsInterState = dto.IsInterState;
        invoice.CGSTRate = dto.CGSTRate;
        invoice.SGSTRate = dto.SGSTRate;
        invoice.IGSTRate = dto.IGSTRate;
        invoice.PaymentTerms = dto.PaymentTerms;
        invoice.UpdatedBy = userId;
        invoice.UpdatedAt = DateTime.UtcNow;

        // Snapshot customer
        var customer = await _db.Customers.FindAsync(dto.CustomerId);
        if (customer != null)
        {
            invoice.CustomerName = customer.Name;
            invoice.CustomerAddress = customer.Address;
            invoice.CustomerGSTIN = customer.GSTIN;
        }

        // Replace lines
        _db.InvoiceLineItems.RemoveRange(invoice.LineItems);
        var lineItems = new List<InvoiceLineItem>();
        foreach (var lineDto in dto.Lines)
        {
            var lineItem = _mapper.Map<InvoiceLineItem>(lineDto);
            var section = await _db.SectionProfiles.FindAsync(lineDto.SectionProfileId);
            lineItem.SectionNumber = section?.SectionNumber;
            lineItem.AreaSFT = Math.Round(lineDto.PerimeterMM * lineDto.LengthMM * lineDto.Quantity / 92903.04m, 2);
            lineItem.Amount = Math.Round(lineItem.AreaSFT * lineDto.RatePerSFT, 2);
            lineItems.Add(lineItem);
        }
        invoice.LineItems = lineItems;

        CalculateInvoiceTotals(invoice);

        await _db.SaveChangesAsync();

        var updated = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.LineItems)
            .FirstAsync(i => i.Id == id);

        return _mapper.Map<InvoiceDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var invoice = await _db.Invoices.FindAsync(id)
            ?? throw new KeyNotFoundException($"Invoice with ID {id} not found.");

        if (invoice.Status != "Draft")
            throw new InvalidOperationException("Only Draft invoices can be deleted.");

        invoice.IsDeleted = true;
        invoice.UpdatedBy = userId;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<InvoiceDto> UpdateStatusAsync(int id, string status, string userId)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Invoice with ID {id} not found.");

        var validTransitions = new Dictionary<string, string[]>
        {
            ["Draft"] = ["Finalized"],
            ["Finalized"] = ["Sent"],
            ["Sent"] = ["Paid"],
        };

        if (!validTransitions.TryGetValue(invoice.Status, out var allowed) || !allowed.Contains(status))
            throw new InvalidOperationException($"Cannot transition from '{invoice.Status}' to '{status}'.");

        invoice.Status = status;
        invoice.UpdatedBy = userId;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.DeliveryChallan)
            .Include(i => i.LineItems).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Invoice with ID {id} not found.");

        var pdfBytes = InvoicePdfService.Generate(invoice);

        // Store the PDF URL
        invoice = await _db.Invoices.FindAsync(id);
        if (invoice != null)
        {
            invoice.FileUrl = $"/api/invoices/{id}/pdf";
            await _db.SaveChangesAsync();
        }

        return pdfBytes;
    }

    public async Task<byte[]?> GetPdfAsync(int id)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.WorkOrder)
            .Include(i => i.DeliveryChallan)
            .Include(i => i.LineItems).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return null;

        return InvoicePdfService.Generate(invoice);
    }

    private static void CalculateInvoiceTotals(Invoice invoice)
    {
        invoice.SubTotal = invoice.LineItems.Sum(l => l.Amount);
        invoice.TaxableAmount = invoice.SubTotal + invoice.PackingCharges + invoice.TransportCharges;

        if (invoice.IsInterState)
        {
            invoice.IGSTAmount = Math.Round(invoice.TaxableAmount * invoice.IGSTRate / 100, 2);
            invoice.CGSTAmount = 0;
            invoice.SGSTAmount = 0;
        }
        else
        {
            invoice.CGSTAmount = Math.Round(invoice.TaxableAmount * invoice.CGSTRate / 100, 2);
            invoice.SGSTAmount = Math.Round(invoice.TaxableAmount * invoice.SGSTRate / 100, 2);
            invoice.IGSTAmount = 0;
        }

        var totalBeforeRound = invoice.TaxableAmount + invoice.CGSTAmount + invoice.SGSTAmount + invoice.IGSTAmount;
        invoice.RoundOff = Math.Round(totalBeforeRound) - totalBeforeRound;
        invoice.GrandTotal = Math.Round(totalBeforeRound);
        invoice.AmountInWords = NumberToWordsConverter.Convert(invoice.GrandTotal);
    }
}
