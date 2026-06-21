namespace HycoatApi.Services.Notifications;

public interface IWhatsAppNotificationService
{
    Task SendStatusUpdateAsync(
        string? phoneNumber,
        string customerName,
        string orderNumber,
        string status,
        string documentType,
        string documentNumber,
        CancellationToken cancellationToken = default);
}
