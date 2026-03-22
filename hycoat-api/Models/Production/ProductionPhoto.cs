using HycoatApi.Models.Identity;

namespace HycoatApi.Models.Production;

public class ProductionPhoto
{
    public int Id { get; set; }
    public int ProductionLogId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public string? UploadedByUserId { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ProductionLog ProductionLog { get; set; } = null!;
    public AppUser? UploadedByUser { get; set; }
}
