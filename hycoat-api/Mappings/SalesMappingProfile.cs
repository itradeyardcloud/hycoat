using AutoMapper;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Models.Sales;

namespace HycoatApi.Mappings;

public class SalesMappingProfile : Profile
{
    public SalesMappingProfile()
    {
        // ── Inquiry ──
        CreateMap<CreateInquiryDto, Inquiry>();
        CreateMap<UpdateInquiryDto, Inquiry>();
        CreateMap<Inquiry, InquiryDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType != null ? s.ProcessType.Name : null))
            .ForMember(d => d.AssignedToName, opt => opt.MapFrom(s => s.AssignedToUser != null ? s.AssignedToUser.FullName : null));
        CreateMap<Inquiry, InquiryDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType != null ? s.ProcessType.Name : null))
            .ForMember(d => d.AssignedToName, opt => opt.MapFrom(s => s.AssignedToUser != null ? s.AssignedToUser.FullName : null))
            .ForMember(d => d.QuotationCount, opt => opt.MapFrom(s => s.Quotations.Count));
        CreateMap<Inquiry, LookupDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.InquiryNumber));

        // ── Quotation ──
        CreateMap<CreateQuotationDto, Quotation>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());
        CreateMap<CreateQuotationLineItemDto, QuotationLineItem>();
        CreateMap<Quotation, QuotationDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.InquiryNumber, opt => opt.MapFrom(s => s.Inquiry != null ? s.Inquiry.InquiryNumber : null))
            .ForMember(d => d.LineItemCount, opt => opt.MapFrom(s => s.LineItems.Count));
        CreateMap<Quotation, QuotationDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.InquiryNumber, opt => opt.MapFrom(s => s.Inquiry != null ? s.Inquiry.InquiryNumber : null))
            .ForMember(d => d.LineItemCount, opt => opt.MapFrom(s => s.LineItems.Count))
            .ForMember(d => d.PreparedByName, opt => opt.MapFrom(s => s.PreparedByUser != null ? s.PreparedByUser.FullName : null))
            .ForMember(d => d.LineItems, opt => opt.MapFrom(s => s.LineItems));
        CreateMap<QuotationLineItem, QuotationLineItemDetailDto>()
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType.Name));

        // ── Proforma Invoice ──
        CreateMap<CreateProformaInvoiceDto, ProformaInvoice>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());
        CreateMap<CreatePILineItemDto, PILineItem>();
        CreateMap<ProformaInvoice, PIDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.QuotationNumber, opt => opt.MapFrom(s => s.Quotation != null ? s.Quotation.QuotationNumber : null));
        CreateMap<ProformaInvoice, PIDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.QuotationNumber, opt => opt.MapFrom(s => s.Quotation != null ? s.Quotation.QuotationNumber : null))
            .ForMember(d => d.PreparedByName, opt => opt.MapFrom(s => s.PreparedByUser != null ? s.PreparedByUser.FullName : null))
            .ForMember(d => d.LineItems, opt => opt.MapFrom(s => s.LineItems));
        CreateMap<PILineItem, PILineItemDetailDto>()
            .ForMember(d => d.SectionNumber, opt => opt.MapFrom(s => s.SectionProfile.SectionNumber));

        // ── Work Order ──
        CreateMap<CreateWorkOrderDto, WorkOrder>();
        CreateMap<WorkOrder, WorkOrderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType.Name))
            .ForMember(d => d.PowderColorName, opt => opt.MapFrom(s => s.PowderColor != null ? s.PowderColor.ColorName : null))
            .ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor != null ? s.PowderColor.PowderCode : null));
        CreateMap<WorkOrder, WorkOrderDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType.Name))
            .ForMember(d => d.PowderColorName, opt => opt.MapFrom(s => s.PowderColor != null ? s.PowderColor.ColorName : null))
            .ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor != null ? s.PowderColor.PowderCode : null))
            .ForMember(d => d.PINumber, opt => opt.MapFrom(s => s.ProformaInvoice != null ? s.ProformaInvoice.PINumber : null))
            .ForMember(d => d.MaterialInwardCount, opt => opt.MapFrom(s => s.MaterialInwards.Count))
            .ForMember(d => d.ProductionWorkOrderCount, opt => opt.MapFrom(s => s.ProductionWorkOrders.Count))
            .ForMember(d => d.DeliveryChallanCount, opt => opt.MapFrom(s => s.DeliveryChallans.Count))
            .ForMember(d => d.InvoiceCount, opt => opt.MapFrom(s => s.Invoices.Count));
        CreateMap<WorkOrder, LookupDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.WONumber));
    }
}
