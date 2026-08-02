namespace HycoatApi.DTOs.Masters;

public class UpdateProfileDiagramDto
{
    public string Code { get; set; } = string.Empty;
    public string? Family { get; set; }
    public string? Series { get; set; }
    public string? Category { get; set; }
    public string? CategoryLabel { get; set; }
    public string? System { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}
