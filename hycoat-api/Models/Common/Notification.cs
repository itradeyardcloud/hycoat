using HycoatApi.Models.Identity;

namespace HycoatApi.Models.Common;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string RecipientUserId { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation
    public AppUser Recipient { get; set; } = null!;
}
