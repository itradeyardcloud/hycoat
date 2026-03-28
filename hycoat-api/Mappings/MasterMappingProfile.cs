using AutoMapper;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;

namespace HycoatApi.Mappings;

public class MasterMappingProfile : Profile
{
    public MasterMappingProfile()
    {
        // Customer
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerDetailDto>()
            .ForMember(d => d.InquiryCount, opt => opt.MapFrom(s => s.Inquiries.Count))
            .ForMember(d => d.WorkOrderCount, opt => opt.MapFrom(s => s.WorkOrders.Count));
        CreateMap<Customer, LookupDto>();

        // SectionProfile
        CreateMap<CreateSectionProfileDto, SectionProfile>();
        CreateMap<UpdateSectionProfileDto, SectionProfile>();
        CreateMap<SectionProfile, SectionProfileDto>();
        CreateMap<SectionProfile, SectionProfileDetailDto>();
        CreateMap<SectionProfile, SectionProfileLookupDto>();

        // PowderColor
        CreateMap<CreatePowderColorDto, PowderColor>();
        CreateMap<UpdatePowderColorDto, PowderColor>();
        CreateMap<PowderColor, PowderColorDto>()
            .ForMember(d => d.VendorName, opt => opt.MapFrom(s => s.Vendor != null ? s.Vendor.Name : null));
        CreateMap<PowderColor, PowderColorDetailDto>()
            .ForMember(d => d.VendorName, opt => opt.MapFrom(s => s.Vendor != null ? s.Vendor.Name : null));
        CreateMap<PowderColor, PowderColorLookupDto>();

        // Vendor
        CreateMap<CreateVendorDto, Vendor>();
        CreateMap<UpdateVendorDto, Vendor>();
        CreateMap<Vendor, VendorDto>();
        CreateMap<Vendor, VendorDetailDto>();
        CreateMap<Vendor, LookupDto>();

        // ProcessType
        CreateMap<CreateProcessTypeDto, ProcessType>();
        CreateMap<UpdateProcessTypeDto, ProcessType>();
        CreateMap<ProcessType, ProcessTypeDto>();

        // ProductionUnit
        CreateMap<CreateProductionUnitDto, ProductionUnit>();
        CreateMap<UpdateProductionUnitDto, ProductionUnit>();
        CreateMap<ProductionUnit, ProductionUnitDto>();
    }
}
