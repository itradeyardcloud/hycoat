using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;

namespace HycoatApi.Services.Dispatch;

public interface IInvoiceService
{
    Task<PagedResponse<InvoiceDto>> GetAllAsync(
        string? search, DateTime? dateFrom, DateTime? dateTo, int? customerId, string? status,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<InvoiceDetailDto> GetByIdAsync(int id);
    Task<InvoiceDetailDto?> GetByWorkOrderIdAsync(int woId);
    Task<InvoiceAutoFillDto> AutoFillAsync(int woId);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, string userId);
    Task<InvoiceDto> UpdateAsync(int id, CreateInvoiceDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<InvoiceDto> UpdateStatusAsync(int id, string status, string userId);
    Task<byte[]> GeneratePdfAsync(int id);
    Task<byte[]?> GetPdfAsync(int id);
}
