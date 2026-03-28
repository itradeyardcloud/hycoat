using AutoMapper;
using HycoatApi.DTOs.Production;
using HycoatApi.Models.Production;

namespace HycoatApi.Mappings;

public class ProductionMappingProfile : Profile
{
    public ProductionMappingProfile()
    {
        // ── Pretreatment Log ──

        CreateMap<CreatePretreatmentLogDto, PretreatmentLog>()
            .ForMember(d => d.TankReadings, opt => opt.Ignore());

        CreateMap<TankReadingDto, PretreatmentTankReading>();

        CreateMap<PretreatmentTankReading, TankReadingDto>();

        CreateMap<PretreatmentLog, PretreatmentLogDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.OperatorName, opt => opt.MapFrom(s =>
                s.OperatorUser != null ? s.OperatorUser.FullName : null))
            .ForMember(d => d.QASignOffName, opt => opt.MapFrom(s =>
                s.QASignOffUser != null ? s.QASignOffUser.FullName : null));

        CreateMap<PretreatmentLog, PretreatmentLogDetailDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.OperatorName, opt => opt.MapFrom(s =>
                s.OperatorUser != null ? s.OperatorUser.FullName : null))
            .ForMember(d => d.QASignOffName, opt => opt.MapFrom(s =>
                s.QASignOffUser != null ? s.QASignOffUser.FullName : null));

        // ── Production (Coating) Log ──

        CreateMap<CreateProductionLogDto, ProductionLog>();

        CreateMap<ProductionLog, ProductionLogDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.SupervisorName, opt => opt.MapFrom(s =>
                s.SupervisorUser != null ? s.SupervisorUser.FullName : null))
            .ForMember(d => d.PhotoCount, opt => opt.MapFrom(s => s.Photos.Count));

        CreateMap<ProductionLog, ProductionLogDetailDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.SupervisorName, opt => opt.MapFrom(s =>
                s.SupervisorUser != null ? s.SupervisorUser.FullName : null))
            .ForMember(d => d.PhotoCount, opt => opt.MapFrom(s => s.Photos.Count));

        CreateMap<ProductionPhoto, ProductionPhotoDto>()
            .ForMember(d => d.UploadedByName, opt => opt.MapFrom(s =>
                s.UploadedByUser != null ? s.UploadedByUser.FullName : null));
    }
}
