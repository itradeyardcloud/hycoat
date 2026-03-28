using AutoMapper;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Models.Dispatch;

namespace HycoatApi.Mappings;

public class DispatchMappingProfile : Profile
{
    public DispatchMappingProfile()
    {
        // ── Packing List ──

        CreateMap<CreatePackingListDto, PackingList>()
            .ForMember(d => d.Lines, opt => opt.Ignore());

        CreateMap<CreatePackingListLineDto, PackingListLine>();

        CreateMap<PackingListLine, PackingListLineDto>()
            .ForMember(d => d.SectionNumber, opt => opt.MapFrom(s =>
                s.SectionProfile != null ? s.SectionProfile.SectionNumber : string.Empty));

        CreateMap<PackingList, PackingListDto>()
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.WorkOrder.Customer.Name))
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.PreparedByName, opt => opt.MapFrom(s =>
                s.PreparedByUser != null ? s.PreparedByUser.FullName : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.TotalQuantity, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Quantity)));

        CreateMap<PackingList, PackingListDetailDto>()
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.WorkOrder.Customer.Name))
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.PreparedByName, opt => opt.MapFrom(s =>
                s.PreparedByUser != null ? s.PreparedByUser.FullName : null));

        // ── Delivery Challan ──

        CreateMap<CreateDeliveryChallanDto, DeliveryChallan>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());

        CreateMap<CreateDCLineItemDto, DCLineItem>();

        CreateMap<DCLineItem, DCLineItemDto>()
            .ForMember(d => d.SectionNumber, opt => opt.MapFrom(s =>
                s.SectionProfile != null ? s.SectionProfile.SectionNumber : string.Empty));

        CreateMap<DeliveryChallan, DeliveryChallanDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.LineItems.Count))
            .ForMember(d => d.TotalQuantity, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.Quantity)));

        CreateMap<DeliveryChallan, DeliveryChallanDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.LineItems))
            .ForMember(d => d.LoadingPhotoUrls, opt => opt.Ignore());

        // ── Invoice ──

        CreateMap<CreateInvoiceDto, Invoice>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());

        CreateMap<CreateInvoiceLineItemDto, InvoiceLineItem>();

        CreateMap<InvoiceLineItem, InvoiceLineItemDto>();

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s =>
                s.CustomerName ?? (s.Customer != null ? s.Customer.Name : string.Empty)))
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.TotalSFT, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.AreaSFT)))
            .ForMember(d => d.HasPdf, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.FileUrl)));

        CreateMap<Invoice, InvoiceDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s =>
                s.CustomerName ?? (s.Customer != null ? s.Customer.Name : string.Empty)))
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.DCNumber, opt => opt.MapFrom(s =>
                s.DeliveryChallan != null ? s.DeliveryChallan.DCNumber : null))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.LineItems));
    }
}
