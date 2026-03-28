namespace HycoatApi.DTOs.Production;

public class ProductionLogDetailDto : ProductionLogDto
{
    public int ProductionWorkOrderId { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductionPhotoDto> Photos { get; set; } = new();
}
