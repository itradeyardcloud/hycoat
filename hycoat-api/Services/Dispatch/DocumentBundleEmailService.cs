using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Dispatch;

public interface IDocumentBundleEmailService
{
    Task SendAsync(int invoiceId, List<string> recipientEmails, string? subject, string? body);
}

public class DocumentBundleEmailService : IDocumentBundleEmailService
{
    private readonly Data.AppDbContext _db;
    private readonly IInvoiceService _invoiceService;
    private readonly IDeliveryChallanService _dcService;
    private readonly IConfiguration _config;
    private readonly ILogger<DocumentBundleEmailService> _logger;

    public DocumentBundleEmailService(
        Data.AppDbContext db,
        IInvoiceService invoiceService,
        IDeliveryChallanService dcService,
        IConfiguration config,
        ILogger<DocumentBundleEmailService> logger)
    {
        _db = db;
        _invoiceService = invoiceService;
        _dcService = dcService;
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(int invoiceId, List<string> recipientEmails, string? subject, string? body)
    {
        var invoice = await _invoiceService.GetByIdAsync(invoiceId);

        var smtpHost = _config["Smtp:Host"];
        var smtpPort = _config.GetValue<int>("Smtp:Port", 587);
        var smtpUser = _config["Smtp:Username"];
        var smtpPass = _config["Smtp:Password"];
        var fromEmail = _config["Smtp:FromEmail"] ?? smtpUser;

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
        {
            _logger.LogWarning("SMTP is not configured. Email sending skipped.");
            throw new InvalidOperationException("Email service is not configured. Please set SMTP settings in appsettings.");
        }

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail!, "HyCoat Systems"),
            Subject = subject ?? $"HyCoat - Invoice {invoice.InvoiceNumber} & Dispatch Documents",
            Body = body ?? $"Dear Customer,\n\nPlease find attached your invoice and dispatch documents for {invoice.InvoiceNumber}.\n\nRegards,\nHyCoat Systems Pvt. Ltd.",
            IsBodyHtml = false
        };

        foreach (var email in recipientEmails)
            message.To.Add(email);

        // Attach Invoice PDF
        try
        {
            var invoicePdf = await _invoiceService.GeneratePdfAsync(invoiceId);
            message.Attachments.Add(new Attachment(
                new MemoryStream(invoicePdf),
                $"Invoice-{invoice.InvoiceNumber}.pdf",
                "application/pdf"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not generate invoice PDF for email bundle");
        }

        // Attach DC PDF if available
        if (invoice.DeliveryChallanId.HasValue)
        {
            try
            {
                var dcPdf = await _dcService.GeneratePdfAsync(invoice.DeliveryChallanId.Value);
                var dcNumber = invoice.DCNumber ?? "DC";
                message.Attachments.Add(new Attachment(
                    new MemoryStream(dcPdf),
                    $"DeliveryChallan-{dcNumber}.pdf",
                    "application/pdf"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate DC PDF for email bundle");
            }
        }

        // Attach Test Certificate PDF if available
        try
        {
            var tc = await _db.TestCertificates
                .Where(t => t.WorkOrderId == invoice.WorkOrderId && !string.IsNullOrEmpty(t.FileUrl))
                .FirstOrDefaultAsync();

            if (tc != null && !string.IsNullOrEmpty(tc.FileUrl))
            {
                var tcPdfPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    tc.FileUrl.TrimStart('/'));

                if (File.Exists(tcPdfPath))
                {
                    message.Attachments.Add(new Attachment(tcPdfPath));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not attach test certificate PDF for email bundle");
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Document bundle email sent for Invoice {InvoiceNumber} to {Recipients}",
            invoice.InvoiceNumber, string.Join(", ", recipientEmails));
    }
}
