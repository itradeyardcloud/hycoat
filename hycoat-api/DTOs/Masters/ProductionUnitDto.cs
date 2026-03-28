namespace HycoatApi.DTOs.Masters;

public class ProductionUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TankLengthMM { get; set; }
    public decimal? TankWidthMM { get; set; }
    public decimal? TankHeightMM { get; set; }
    public decimal? BucketLengthMM { get; set; }
    public decimal? BucketWidthMM { get; set; }
    public decimal? BucketHeightMM { get; set; }
    public decimal? ConveyorLengthMtrs { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
