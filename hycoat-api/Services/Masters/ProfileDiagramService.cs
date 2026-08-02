using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using HycoatApi.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Masters;

public class ProfileDiagramService : IProfileDiagramService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blob;

    public ProfileDiagramService(AppDbContext db, IMapper mapper, IBlobStorageService blob)
    {
        _db = db;
        _mapper = mapper;
        _blob = blob;
    }

    public async Task<PagedResponse<ProfileDiagramDto>> GetAllAsync(
        string? search, int page, int pageSize, string sortBy, bool sortDesc,
        CancellationToken ct = default)
    {
        var query = _db.ProfileDiagrams.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToUpper();
            query = query.Where(p =>
                p.Code.ToUpper().Contains(s) ||
                (p.Family != null && p.Family.ToUpper().Contains(s)) ||
                (p.Series != null && p.Series.ToUpper().Contains(s)) ||
                (p.Category != null && p.Category.ToUpper().Contains(s)));
        }

        var total = await query.CountAsync(ct);

        query = (sortBy?.ToLower() ?? "code", sortDesc) switch
        {
            ("family", false) => query.OrderBy(p => p.Family).ThenBy(p => p.Code),
            ("family", true) => query.OrderByDescending(p => p.Family).ThenBy(p => p.Code),
            ("series", false) => query.OrderBy(p => p.Series).ThenBy(p => p.Code),
            ("series", true) => query.OrderByDescending(p => p.Series).ThenBy(p => p.Code),
            ("sortorder", false) => query.OrderBy(p => p.SortOrder).ThenBy(p => p.Code),
            ("sortorder", true) => query.OrderByDescending(p => p.SortOrder).ThenBy(p => p.Code),
            ("createdat", false) => query.OrderBy(p => p.CreatedAt),
            ("createdat", true) => query.OrderByDescending(p => p.CreatedAt),
            (_, false) => query.OrderBy(p => p.Code),
            (_, true) => query.OrderByDescending(p => p.Code),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<ProfileDiagramDto>
        {
            Items = _mapper.Map<List<ProfileDiagramDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProfileDiagramDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ProfileDiagrams.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Profile diagram {id} not found.");

        return _mapper.Map<ProfileDiagramDetailDto>(entity);
    }

    public async Task<List<ProfileDiagramDto>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken ct = default)
    {
        var upperCodes = codes
            .Select(c => c.Trim().ToUpper())
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        if (upperCodes.Count == 0)
            return new List<ProfileDiagramDto>();

        var items = await _db.ProfileDiagrams.AsNoTracking()
            .Where(p => upperCodes.Contains(p.Code.ToUpper()))
            .OrderBy(p => p.Code)
            .ToListAsync(ct);

        return _mapper.Map<List<ProfileDiagramDto>>(items);
    }

    public async Task<ProfileDiagramDto> CreateAsync(
        CreateProfileDiagramDto dto, string userId, CancellationToken ct = default)
    {
        var code = dto.Code.Trim().ToUpper();

        if (await _db.ProfileDiagrams.AnyAsync(p => p.Code == code, ct))
            throw new InvalidOperationException($"A profile diagram with code '{code}' already exists.");

        var entity = _mapper.Map<ProfileDiagram>(dto);
        entity.Code = code;
        entity.CreatedBy = userId;
        entity.CreatedAt = DateTime.UtcNow;

        _db.ProfileDiagrams.Add(entity);
        await _db.SaveChangesAsync(ct);

        return _mapper.Map<ProfileDiagramDto>(entity);
    }

    public async Task<ProfileDiagramDto> UpdateAsync(
        int id, UpdateProfileDiagramDto dto, string userId, CancellationToken ct = default)
    {
        var entity = await _db.ProfileDiagrams.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Profile diagram {id} not found.");

        var newCode = dto.Code.Trim().ToUpper();
        if (newCode != entity.Code &&
            await _db.ProfileDiagrams.AnyAsync(p => p.Code == newCode && p.Id != id, ct))
            throw new InvalidOperationException($"A profile diagram with code '{newCode}' already exists.");

        _mapper.Map(dto, entity);
        entity.Code = newCode;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<ProfileDiagramDto>(entity);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken ct = default)
    {
        var entity = await _db.ProfileDiagrams.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Profile diagram {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<string> UploadImageAsync(
        int id, IFormFile file, string userId, CancellationToken ct = default)
    {
        var entity = await _db.ProfileDiagrams.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Profile diagram {id} not found.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".png" or ".jpg" or ".jpeg"))
            throw new InvalidOperationException("Only PNG and JPEG images are supported.");

        if (file.Length > 20 * 1024 * 1024)
            throw new InvalidOperationException("Image file must be smaller than 20 MB.");

        if (!string.IsNullOrEmpty(entity.ImageUrl))
            await _blob.DeleteIfExistsAsync(entity.ImageUrl, ct);

        var blobName = $"profile-diagrams/{entity.Code.ToUpper()}{ext}";
        var result = await _blob.UploadAsync(file, blobName, ct);

        entity.ImageUrl = result.BlobUrl;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return result.BlobUrl;
    }

    public async Task<byte[]> DownloadCombinedPdfAsync(
        IEnumerable<string> codes, CancellationToken ct = default)
    {
        var diagrams = await GetByCodesAsync(codes, ct);

        if (diagrams.Count == 0)
            throw new InvalidOperationException("No matching profile diagrams found for the requested codes.");

        // Pre-load all image bytes
        var pages = new List<(ProfileDiagramDto Diagram, byte[]? ImageBytes)>();
        foreach (var d in diagrams)
        {
            byte[]? imageBytes = null;
            if (!string.IsNullOrEmpty(d.ImageUrl))
            {
                var dl = await _blob.DownloadAsync(d.ImageUrl, ct);
                if (dl != null)
                {
                    using var ms = new MemoryStream();
                    await dl.Stream.CopyToAsync(ms, ct);
                    imageBytes = ms.ToArray();
                }
            }
            pages.Add((d, imageBytes));
        }

        var document = Document.Create(container =>
        {
            foreach (var (diagram, imageBytes) in pages)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.MarginHorizontal(20);
                    page.MarginVertical(16);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(h =>
                    {
                        h.Item().AlignCenter()
                            .Text("HYCOAT SYSTEMS — Profile Diagram Catalog")
                            .Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                        h.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(8).Column(col =>
                    {
                        col.Item().AlignCenter().Text(diagram.Code)
                            .Bold().FontSize(16);

                        if (!string.IsNullOrEmpty(diagram.System))
                            col.Item().PaddingTop(2).AlignCenter()
                                .Text(diagram.System)
                                .FontSize(9).FontColor(Colors.Grey.Darken1);

                        if (!string.IsNullOrEmpty(diagram.Category))
                            col.Item().AlignCenter()
                                .Text(diagram.Category)
                                .FontSize(9).FontColor(Colors.Grey.Medium);

                        col.Item().PaddingTop(10).AlignCenter().Element(c =>
                        {
                            if (imageBytes != null && imageBytes.Length > 0)
                                c.Image(imageBytes).FitArea();
                            else
                                c.PaddingTop(40).AlignCenter()
                                    .Text("(No image uploaded for this profile)")
                                    .FontSize(12).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Footer().Column(f =>
                    {
                        f.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                        f.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text(
                                $"DTS REF DOC No: 549-R1  |  All dimensions in mm")
                                .FontSize(7).FontColor(Colors.Grey.Darken1);
                            row.ConstantItem(80).AlignRight().Text(t =>
                            {
                                t.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Darken1);
                                t.Span(" / ").FontSize(7).FontColor(Colors.Grey.Darken1);
                                t.TotalPages().FontSize(7).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });
                });
            }
        });

        return document.GeneratePdf();
    }
}
