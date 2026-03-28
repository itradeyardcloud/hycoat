using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;
using HycoatApi.Models.Quality;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Quality;

public class TestCertificateService : ITestCertificateService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public TestCertificateService(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

    public async Task<PagedResponse<TestCertificateDto>> GetAllAsync(
        string? search, DateTime? date, int? customerId,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.TestCertificates
            .AsNoTracking()
            .Include(tc => tc.Customer)
            .Include(tc => tc.WorkOrder)
            .Include(tc => tc.FinalInspection)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(tc =>
                tc.CertificateNumber.ToLower().Contains(term) ||
                tc.Customer.Name.ToLower().Contains(term) ||
                tc.WorkOrder.WONumber.ToLower().Contains(term));
        }

        if (date.HasValue)
            query = query.Where(tc => tc.Date == date.Value.Date);

        if (customerId.HasValue)
            query = query.Where(tc => tc.CustomerId == customerId.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "certificatenumber" => sortDesc
                ? query.OrderByDescending(tc => tc.CertificateNumber)
                : query.OrderBy(tc => tc.CertificateNumber),
            "customer" => sortDesc
                ? query.OrderByDescending(tc => tc.Customer.Name)
                : query.OrderBy(tc => tc.Customer.Name),
            _ => sortDesc
                ? query.OrderByDescending(tc => tc.Date)
                : query.OrderBy(tc => tc.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<TestCertificateDto>
        {
            Items = _mapper.Map<List<TestCertificateDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TestCertificateDetailDto> GetByIdAsync(int id)
    {
        var tc = await _db.TestCertificates
            .AsNoTracking()
            .Include(tc => tc.Customer)
            .Include(tc => tc.WorkOrder)
            .Include(tc => tc.FinalInspection)
            .FirstOrDefaultAsync(tc => tc.Id == id)
            ?? throw new KeyNotFoundException($"Test Certificate with ID {id} not found.");

        return _mapper.Map<TestCertificateDetailDto>(tc);
    }

    public async Task<TestCertificateDetailDto?> GetByWorkOrderAsync(int workOrderId)
    {
        var tc = await _db.TestCertificates
            .AsNoTracking()
            .Include(tc => tc.Customer)
            .Include(tc => tc.WorkOrder)
            .Include(tc => tc.FinalInspection)
            .Where(tc => tc.WorkOrderId == workOrderId)
            .FirstOrDefaultAsync();

        return tc == null ? null : _mapper.Map<TestCertificateDetailDto>(tc);
    }

    public async Task<TestCertificateDto> CreateAsync(CreateTestCertificateDto dto, string userId)
    {
        // Validate final inspection exists and is Approved
        var finalInspection = await _db.FinalInspections.FindAsync(dto.FinalInspectionId)
            ?? throw new ArgumentException("Final Inspection not found.");

        if (finalInspection.OverallStatus != "Approved")
            throw new InvalidOperationException("Test Certificate can only be created for an Approved Final Inspection.");

        // Check no existing TC for this FIR
        var existingTC = await _db.TestCertificates
            .AnyAsync(tc => tc.FinalInspectionId == dto.FinalInspectionId);

        if (existingTC)
            throw new InvalidOperationException("A Test Certificate already exists for this Final Inspection.");

        // Validate customer and work order exist
        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new ArgumentException("Customer not found.");
        var workOrder = await _db.WorkOrders.FindAsync(dto.WorkOrderId)
            ?? throw new ArgumentException("Work Order not found.");

        // Auto-generate certificate number
        var year = DateTime.UtcNow.Year;
        var lastNumber = await _db.TestCertificates
            .Where(tc => tc.CertificateNumber.StartsWith($"TC-{year}-"))
            .OrderByDescending(tc => tc.CertificateNumber)
            .Select(tc => tc.CertificateNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                sequence = lastSeq + 1;
        }

        var certificate = _mapper.Map<TestCertificate>(dto);
        certificate.CertificateNumber = $"TC-{year}-{sequence:D3}";
        certificate.Date = dto.Date.Date;
        certificate.CreatedBy = userId;

        _db.TestCertificates.Add(certificate);
        await _db.SaveChangesAsync();

        var created = await _db.TestCertificates
            .AsNoTracking()
            .Include(tc => tc.Customer)
            .Include(tc => tc.WorkOrder)
            .Include(tc => tc.FinalInspection)
            .FirstAsync(tc => tc.Id == certificate.Id);

        return _mapper.Map<TestCertificateDto>(created);
    }

    public async Task<TestCertificateDto> UpdateAsync(int id, CreateTestCertificateDto dto, string userId)
    {
        var certificate = await _db.TestCertificates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Test Certificate with ID {id} not found.");

        certificate.Date = dto.Date.Date;
        certificate.FinalInspectionId = dto.FinalInspectionId;
        certificate.CustomerId = dto.CustomerId;
        certificate.WorkOrderId = dto.WorkOrderId;
        certificate.ProductCode = dto.ProductCode;
        certificate.ProjectName = dto.ProjectName;
        certificate.LotQuantity = dto.LotQuantity;
        certificate.Warranty = dto.Warranty;
        certificate.SubstrateResult = dto.SubstrateResult;
        certificate.BakingTempResult = dto.BakingTempResult;
        certificate.BakingTimeResult = dto.BakingTimeResult;
        certificate.ColorResult = dto.ColorResult;
        certificate.DFTResult = dto.DFTResult;
        certificate.MEKResult = dto.MEKResult;
        certificate.CrossCutResult = dto.CrossCutResult;
        certificate.ConicalMandrelResult = dto.ConicalMandrelResult;
        certificate.BoilingWaterResult = dto.BoilingWaterResult;
        certificate.UpdatedBy = userId;
        certificate.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.TestCertificates
            .AsNoTracking()
            .Include(tc => tc.Customer)
            .Include(tc => tc.WorkOrder)
            .Include(tc => tc.FinalInspection)
            .FirstAsync(tc => tc.Id == id);

        return _mapper.Map<TestCertificateDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var certificate = await _db.TestCertificates.FindAsync(id)
            ?? throw new KeyNotFoundException($"Test Certificate with ID {id} not found.");

        certificate.IsDeleted = true;
        certificate.UpdatedBy = userId;
        certificate.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var tc = await _db.TestCertificates
            .Include(tc => tc.Customer)
            .Include(tc => tc.WorkOrder)
            .Include(tc => tc.FinalInspection)
            .FirstOrDefaultAsync(tc => tc.Id == id)
            ?? throw new KeyNotFoundException($"Test Certificate with ID {id} not found.");

        var pdfService = new TestCertificatePdfService();
        var pdfBytes = pdfService.Generate(_mapper.Map<TestCertificateDetailDto>(tc));

        // Save PDF
        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
            "uploads", "certificates");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{tc.CertificateNumber.Replace("/", "-")}.pdf";
        var filePath = Path.Combine(uploadsDir, fileName);
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        tc.FileUrl = $"/uploads/certificates/{fileName}";
        await _db.SaveChangesAsync();

        return pdfBytes;
    }

    public async Task<(byte[] FileBytes, string FileName)?> DownloadPdfAsync(int id)
    {
        var tc = await _db.TestCertificates
            .AsNoTracking()
            .FirstOrDefaultAsync(tc => tc.Id == id)
            ?? throw new KeyNotFoundException($"Test Certificate with ID {id} not found.");

        if (string.IsNullOrEmpty(tc.FileUrl))
            return null;

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRoot, tc.FileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(fullPath))
            return null;

        var bytes = await File.ReadAllBytesAsync(fullPath);
        var fileName = $"{tc.CertificateNumber}.pdf";
        return (bytes, fileName);
    }
}
