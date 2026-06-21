namespace HycoatApi.Services.Notifications;

public class WhatsAppCloudApiOptions
{
    public bool Enabled { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v20.0";
    public string TemplateName { get; set; } = "order_status_update";
    public string LanguageCode { get; set; } = "en";
    public string DefaultCountryCode { get; set; } = "91";
    public int RequestTimeoutSeconds { get; set; } = 15;
}
