namespace HycoatApi.DTOs.Masters;

public class ProfileDiagramDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Family { get; set; }
    public string? Series { get; set; }
    public string? Category { get; set; }
    public string? System { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProfileDiagramDetailDto : ProfileDiagramDto
{
    public string? CategoryLabel { get; set; }
    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }
    public string? Notes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
