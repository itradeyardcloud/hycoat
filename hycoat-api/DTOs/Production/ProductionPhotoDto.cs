namespace HycoatApi.DTOs.Production;

public class ProductionPhotoDto
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
    public string? UploadedByName { get; set; }
    public string? Description { get; set; }
}
