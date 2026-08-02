using HycoatApi.Models.Common;

namespace HycoatApi.Models.Masters;

public class ProfileDiagram : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Family { get; set; }
    public string? Series { get; set; }
    public string? Category { get; set; }
    public string? CategoryLabel { get; set; }
    public string? System { get; set; }
    public string? ImageUrl { get; set; }
    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}
