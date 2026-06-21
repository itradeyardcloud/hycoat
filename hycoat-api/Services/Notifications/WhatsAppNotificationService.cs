using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace HycoatApi.Services.Notifications;

public class WhatsAppNotificationService : IWhatsAppNotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WhatsAppCloudApiOptions _options;
    private readonly ILogger<WhatsAppNotificationService> _logger;

    public WhatsAppNotificationService(
        IHttpClientFactory httpClientFactory,
        IOptions<WhatsAppCloudApiOptions> options,
        ILogger<WhatsAppNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendStatusUpdateAsync(
        string? phoneNumber,
        string customerName,
        string orderNumber,
        string status,
        string documentType,
        string documentNumber,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.AccessToken) ||
            string.IsNullOrWhiteSpace(_options.PhoneNumberId) ||
            string.IsNullOrWhiteSpace(_options.TemplateName))
        {
            _logger.LogWarning("WhatsApp is enabled but required settings are missing.");
            return;
        }

        var normalizedPhone = NormalizePhone(phoneNumber, _options.DefaultCountryCode);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            _logger.LogInformation("Skipping WhatsApp notification because phone number is missing or invalid.");
            return;
        }

        var summary = $"Order {orderNumber}: {status} ({documentType} {documentNumber})";
        var endpoint = $"https://graph.facebook.com/{_options.ApiVersion}/{_options.PhoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = normalizedPhone,
            type = "template",
            template = new
            {
                name = _options.TemplateName,
                language = new { code = _options.LanguageCode },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = new[]
                        {
                            new { type = "text", text = summary }
                        }
                    }
                }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("MetaWhatsApp");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(_options.RequestTimeoutSeconds, 5));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

            using var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(endpoint, content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "WhatsApp send failed. StatusCode: {StatusCode}. Phone: {Phone}. Response: {Response}",
                    (int)response.StatusCode,
                    normalizedPhone,
                    responseText);
                return;
            }

            _logger.LogInformation(
                "WhatsApp sent successfully for order {OrderNumber} ({Status}) to {Phone}.",
                orderNumber,
                status,
                normalizedPhone);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "WhatsApp send exception for order {OrderNumber} ({Status}) to phone {Phone}.",
                orderNumber,
                status,
                normalizedPhone);
        }
    }

    private static string? NormalizePhone(string? phone, string? defaultCountryCode)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var trimmed = phone.Trim();
        var hasPlus = trimmed.StartsWith('+');
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(digits))
        {
            return null;
        }

        if (hasPlus)
        {
            return digits;
        }

        if (digits.Length == 10 && !string.IsNullOrWhiteSpace(defaultCountryCode))
        {
            var countryDigits = new string(defaultCountryCode.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrWhiteSpace(countryDigits))
            {
                return countryDigits + digits;
            }
        }

        return digits;
    }
}
