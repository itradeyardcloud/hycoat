using AutoMapper;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;
using HycoatApi.Models.Common;

namespace HycoatApi.Mappings;

public class MaterialInwardMappingProfile : Profile
{
    public MaterialInwardMappingProfile()
    {
        // MaterialInward
        CreateMap<Models.MaterialInward.MaterialInward, MaterialInwardDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder != null ? s.WorkOrder.WONumber : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count));

        CreateMap<Models.MaterialInward.MaterialInward, MaterialInwardDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder != null ? s.WorkOrder.WONumber : null))
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType != null ? s.ProcessType.Name : null))
            .ForMember(d => d.PowderColorName, opt => opt.MapFrom(s => s.PowderColor != null ? s.PowderColor.ColorName : null))
            .ForMember(d => d.ReceivedByName, opt => opt.MapFrom(s => s.ReceivedByUser != null ? s.ReceivedByUser.FullName : null))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count));

        CreateMap<Models.MaterialInward.MaterialInwardLine, MaterialInwardLineDto>()
            .ForMember(d => d.SectionNumber, opt => opt.MapFrom(s => s.SectionProfile.SectionNumber));

        // IncomingInspection
        CreateMap<Models.MaterialInward.IncomingInspection, IncomingInspectionDto>()
            .ForMember(d => d.InwardNumber, opt => opt.MapFrom(s => s.MaterialInward.InwardNumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.MaterialInward.Customer.Name))
            .ForMember(d => d.InspectedByName, opt => opt.MapFrom(s => s.InspectedByUser != null ? s.InspectedByUser.FullName : null));

        CreateMap<Models.MaterialInward.IncomingInspection, IncomingInspectionDetailDto>()
            .ForMember(d => d.InwardNumber, opt => opt.MapFrom(s => s.MaterialInward.InwardNumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.MaterialInward.Customer.Name))
            .ForMember(d => d.InspectedByName, opt => opt.MapFrom(s => s.InspectedByUser != null ? s.InspectedByUser.FullName : null));

        CreateMap<Models.MaterialInward.IncomingInspectionLine, IncomingInspectionLineDetailDto>()
            .ForMember(d => d.SectionNumber, opt => opt.MapFrom(s => s.MaterialInwardLine.SectionProfile.SectionNumber))
            .ForMember(d => d.QtyReceived, opt => opt.MapFrom(s => s.MaterialInwardLine.QtyReceived));

        // FileAttachment
        CreateMap<FileAttachment, FileAttachmentDto>();
    }
}
