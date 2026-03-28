namespace HycoatApi.DTOs.Dispatch;

public class SendEmailDto
{
    public List<string> RecipientEmails { get; set; } = [];
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
