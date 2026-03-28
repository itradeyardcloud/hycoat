namespace HycoatApi.DTOs.Production;

public class PretreatmentLogDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int BasketNumber { get; set; }
    public decimal? EtchTimeMins { get; set; }
    public string? OperatorName { get; set; }
    public string? QASignOffName { get; set; }
}
