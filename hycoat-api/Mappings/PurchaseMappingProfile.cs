using AutoMapper;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Models.Purchase;

namespace HycoatApi.Mappings;

public class PurchaseMappingProfile : Profile
{
    public PurchaseMappingProfile()
    {
        // ── Powder Indent ──

        CreateMap<CreatePowderIndentDto, PowderIndent>()
            .ForMember(d => d.Lines, opt => opt.Ignore());

        CreateMap<CreatePowderIndentLineDto, PowderIndentLine>();

        CreateMap<PowderIndentLine, PowderIndentLineDto>()
            .ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor.PowderCode))
            .ForMember(d => d.ColorName, opt => opt.MapFrom(s => s.PowderColor.ColorName))
            .ForMember(d => d.RALCode, opt => opt.MapFrom(s => s.PowderColor.RALCode));

        CreateMap<PowderIndent, PowderIndentDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s =>
                s.ProductionWorkOrder != null ? s.ProductionWorkOrder.PWONumber : null))
            .ForMember(d => d.RequestedByName, opt => opt.MapFrom(s =>
                s.RequestedByUser != null ? s.RequestedByUser.FullName : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.TotalQtyKg, opt => opt.MapFrom(s => s.Lines.Sum(l => l.RequiredQtyKg)));

        CreateMap<PowderIndent, PowderIndentDetailDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s =>
                s.ProductionWorkOrder != null ? s.ProductionWorkOrder.PWONumber : null))
            .ForMember(d => d.RequestedByName, opt => opt.MapFrom(s =>
                s.RequestedByUser != null ? s.RequestedByUser.FullName : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.TotalQtyKg, opt => opt.MapFrom(s => s.Lines.Sum(l => l.RequiredQtyKg)));

        // ── Purchase Order ──

        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());

        CreateMap<CreatePOLineItemDto, POLineItem>();

        CreateMap<POLineItem, POLineItemDto>()
            .ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor.PowderCode))
            .ForMember(d => d.ColorName, opt => opt.MapFrom(s => s.PowderColor.ColorName))
            .ForMember(d => d.RALCode, opt => opt.MapFrom(s => s.PowderColor.RALCode));

        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.VendorName, opt => opt.MapFrom(s => s.Vendor.Name))
            .ForMember(d => d.IndentNumber, opt => opt.MapFrom(s =>
                s.PowderIndent != null ? s.PowderIndent.IndentNumber : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.LineItems.Count))
            .ForMember(d => d.TotalQtyKg, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.QtyKg)))
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.Amount)));

        CreateMap<PurchaseOrder, PurchaseOrderDetailDto>()
            .ForMember(d => d.VendorName, opt => opt.MapFrom(s => s.Vendor.Name))
            .ForMember(d => d.IndentNumber, opt => opt.MapFrom(s =>
                s.PowderIndent != null ? s.PowderIndent.IndentNumber : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.LineItems.Count))
            .ForMember(d => d.TotalQtyKg, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.QtyKg)))
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.Amount)))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.LineItems));

        // ── GRN ──

        CreateMap<CreateGRNDto, GoodsReceivedNote>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());

        CreateMap<CreateGRNLineItemDto, GRNLineItem>();

        CreateMap<GRNLineItem, GRNLineItemDto>()
            .ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor.PowderCode))
            .ForMember(d => d.ColorName, opt => opt.MapFrom(s => s.PowderColor.ColorName))
            .ForMember(d => d.RALCode, opt => opt.MapFrom(s => s.PowderColor.RALCode));

        CreateMap<GoodsReceivedNote, GRNDto>()
            .ForMember(d => d.PONumber, opt => opt.MapFrom(s => s.PurchaseOrder.PONumber))
            .ForMember(d => d.ReceivedByName, opt => opt.MapFrom(s =>
                s.ReceivedByUser != null ? s.ReceivedByUser.FullName : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.LineItems.Count))
            .ForMember(d => d.TotalReceivedKg, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.QtyReceivedKg)));

        CreateMap<GoodsReceivedNote, GRNDetailDto>()
            .ForMember(d => d.PONumber, opt => opt.MapFrom(s => s.PurchaseOrder.PONumber))
            .ForMember(d => d.ReceivedByName, opt => opt.MapFrom(s =>
                s.ReceivedByUser != null ? s.ReceivedByUser.FullName : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.LineItems.Count))
            .ForMember(d => d.TotalReceivedKg, opt => opt.MapFrom(s => s.LineItems.Sum(l => l.QtyReceivedKg)))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.LineItems));

        // ── Powder Stock ──

        CreateMap<PowderStock, PowderStockDto>()
            .ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor.PowderCode))
            .ForMember(d => d.ColorName, opt => opt.MapFrom(s => s.PowderColor.ColorName))
            .ForMember(d => d.RALCode, opt => opt.MapFrom(s => s.PowderColor.RALCode))
            .ForMember(d => d.IsBelowReorderLevel, opt => opt.MapFrom(s =>
                s.ReorderLevelKg.HasValue && s.CurrentStockKg < s.ReorderLevelKg.Value));
    }
}
