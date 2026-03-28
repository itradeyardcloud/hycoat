namespace HycoatApi.DTOs.Quality;

public class TestCertificateDto
{
    public int Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? ProjectName { get; set; }
    public int LotQuantity { get; set; }
    public string? Warranty { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public bool HasPdf { get; set; }
}
