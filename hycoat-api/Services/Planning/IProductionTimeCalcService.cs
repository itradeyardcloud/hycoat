using HycoatApi.DTOs.Planning;
using HycoatApi.Models.Masters;

namespace HycoatApi.Services.Planning;

public interface IProductionTimeCalcService
{
    ProductionTimeCalcResultDto Calculate(
        List<TimeCalcLineItemDto> lineItems,
        List<SectionProfile> sectionProfiles,
        ProductionUnit productionUnit);
}
