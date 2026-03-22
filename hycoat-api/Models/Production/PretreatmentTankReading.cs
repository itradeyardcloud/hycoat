namespace HycoatApi.Models.Production;

public class PretreatmentTankReading
{
    public int Id { get; set; }
    public int PretreatmentLogId { get; set; }
    public string TankName { get; set; } = string.Empty;
    public decimal? Concentration { get; set; }
    public decimal? Temperature { get; set; }
    public string? ChemicalAdded { get; set; }
    public decimal? ChemicalQty { get; set; }

    // Navigation
    public PretreatmentLog PretreatmentLog { get; set; } = null!;
}
