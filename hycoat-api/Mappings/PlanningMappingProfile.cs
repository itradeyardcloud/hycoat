using AutoMapper;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Planning;
using HycoatApi.Models.Planning;

namespace HycoatApi.Mappings;

public class PlanningMappingProfile : Profile
{
    public PlanningMappingProfile()
    {
        // ProductionWorkOrder — Create/Update
        CreateMap<CreateProductionWorkOrderDto, ProductionWorkOrder>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());
        CreateMap<UpdateProductionWorkOrderDto, ProductionWorkOrder>()
            .ForMember(d => d.LineItems, opt => opt.Ignore());

        // ProductionWorkOrder — Read (list)
        CreateMap<ProductionWorkOrder, ProductionWorkOrderDto>()
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.ProductionUnitName, opt => opt.MapFrom(s => s.ProductionUnit.Name));

        // ProductionWorkOrder — Read (detail)
        CreateMap<ProductionWorkOrder, ProductionWorkOrderDetailDto>()
            .ForMember(d => d.WONumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.ProductionUnitName, opt => opt.MapFrom(s => s.ProductionUnit.Name))
            .ForMember(d => d.ProcessTypeName, opt => opt.MapFrom(s => s.ProcessType.Name));

        // ProductionWorkOrder — Lookup
        CreateMap<ProductionWorkOrder, LookupDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.PWONumber));

        // PWOLineItem — Create
        CreateMap<CreatePWOLineItemDto, PWOLineItem>();

        // PWOLineItem — Read (detail)
        CreateMap<PWOLineItem, PWOLineItemDetailDto>()
            .ForMember(d => d.SectionNumber, opt => opt.MapFrom(s => s.SectionProfile.SectionNumber));

        // ProductionTimeCalc
        CreateMap<ProductionTimeCalc, ProductionTimeCalcDto>();

        // ProductionSchedule — Create
        CreateMap<CreateScheduleEntryDto, ProductionSchedule>();
        CreateMap<UpdateScheduleEntryDto, ProductionSchedule>();

        // ProductionSchedule — Read
        CreateMap<ProductionSchedule, ScheduleEntryDto>()
            .ForMember(d => d.ProductionUnitName, opt => opt.MapFrom(s => s.ProductionUnit.Name))
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.PowderColor, opt => opt.MapFrom(s => s.ProductionWorkOrder.ColorName))
            .ForMember(d => d.TotalTimeHrs, opt => opt.MapFrom(s => s.ProductionWorkOrder.TotalTimeHrs));
    }
}
