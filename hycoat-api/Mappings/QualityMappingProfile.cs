using AutoMapper;
using HycoatApi.DTOs.Quality;
using HycoatApi.Models.Quality;

namespace HycoatApi.Mappings;

public class QualityMappingProfile : Profile
{
    public QualityMappingProfile()
    {
        // ── In-Process Inspection ──

        CreateMap<CreateInProcessInspectionDto, InProcessInspection>()
            .ForMember(d => d.DFTReadings, opt => opt.Ignore())
            .ForMember(d => d.TestResults, opt => opt.Ignore());

        CreateMap<CreateDFTReadingDto, InProcessDFTReading>();
        CreateMap<CreateTestResultDto, InProcessTestResult>();

        CreateMap<InProcessDFTReading, DFTReadingDto>()
            .ForMember(d => d.SectionProfileName, opt => opt.MapFrom(s =>
                s.SectionProfile != null ? s.SectionProfile.SectionNumber : null));

        CreateMap<InProcessTestResult, TestResultDto>();

        CreateMap<InProcessInspection, InProcessInspectionDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.InspectorName, opt => opt.MapFrom(s =>
                s.InspectorUser != null ? s.InspectorUser.FullName : null))
            .ForMember(d => d.DFTAvg, opt => opt.MapFrom(s =>
                s.DFTReadings.Any() ? s.DFTReadings.Average(r => r.AvgReading) : (decimal?)null))
            .ForMember(d => d.TestCount, opt => opt.MapFrom(s => s.TestResults.Count))
            .ForMember(d => d.TestPassCount, opt => opt.MapFrom(s =>
                s.TestResults.Count(r => r.Status == "Pass")))
            .ForMember(d => d.TestFailCount, opt => opt.MapFrom(s =>
                s.TestResults.Count(r => r.Status == "Fail")))
            .ForMember(d => d.AllWithinSpec, opt => opt.MapFrom(s =>
                s.DFTReadings.All(r => r.IsWithinSpec) && s.TestResults.All(r => r.Status == "Pass")));

        CreateMap<InProcessInspection, InProcessInspectionDetailDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.InspectorName, opt => opt.MapFrom(s =>
                s.InspectorUser != null ? s.InspectorUser.FullName : null));

        // ── Panel Test ──

        CreateMap<CreatePanelTestDto, PanelTest>();

        CreateMap<PanelTest, PanelTestDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.InspectorName, opt => opt.MapFrom(s =>
                s.InspectorUser != null ? s.InspectorUser.FullName : null));

        CreateMap<PanelTest, PanelTestDetailDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.InspectorName, opt => opt.MapFrom(s =>
                s.InspectorUser != null ? s.InspectorUser.FullName : null));

        // ── Final Inspection ──

        CreateMap<CreateFinalInspectionDto, FinalInspection>();

        CreateMap<FinalInspection, FinalInspectionDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.InspectorName, opt => opt.MapFrom(s =>
                s.InspectorUser != null ? s.InspectorUser.FullName : null))
            .ForMember(d => d.HasTestCertificate, opt => opt.MapFrom(s => s.TestCertificate != null));

        CreateMap<FinalInspection, FinalInspectionDetailDto>()
            .ForMember(d => d.PWONumber, opt => opt.MapFrom(s => s.ProductionWorkOrder.PWONumber))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ProductionWorkOrder.Customer.Name))
            .ForMember(d => d.InspectorName, opt => opt.MapFrom(s =>
                s.InspectorUser != null ? s.InspectorUser.FullName : null))
            .ForMember(d => d.TestCertificateId, opt => opt.MapFrom(s =>
                s.TestCertificate != null ? s.TestCertificate.Id : (int?)null))
            .ForMember(d => d.TestCertificateNumber, opt => opt.MapFrom(s =>
                s.TestCertificate != null ? s.TestCertificate.CertificateNumber : null));

        // ── Test Certificate ──

        CreateMap<CreateTestCertificateDto, TestCertificate>();

        CreateMap<TestCertificate, TestCertificateDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WorkOrderNumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.OverallStatus, opt => opt.MapFrom(s => s.FinalInspection.OverallStatus))
            .ForMember(d => d.HasPdf, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.FileUrl)));

        CreateMap<TestCertificate, TestCertificateDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WorkOrderNumber, opt => opt.MapFrom(s => s.WorkOrder.WONumber))
            .ForMember(d => d.OverallStatus, opt => opt.MapFrom(s => s.FinalInspection.OverallStatus));
    }
}
