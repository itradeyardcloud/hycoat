using AutoMapper;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Models.Sales;

namespace HycoatApi.Mappings;

public class SalesMappingProfile : Profile
{
    public SalesMappingProfile()
    {
        // Inquiry
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
    }
}
