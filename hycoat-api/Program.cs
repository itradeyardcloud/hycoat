using HycoatApi.Data;
using HycoatApi.Middleware;
using HycoatApi.Models.Identity;
using FluentValidation;
using HycoatApi.Services.Masters;
using HycoatApi.Services.Sales;
using HycoatApi.Services.MaterialInward;
using HycoatApi.Services.Planning;
using HycoatApi.Services.Quality;
using HycoatApi.Services.Production;
using HycoatApi.Services.Dispatch;
using HycoatApi.Services.Purchase;
using HycoatApi.Services.Dashboard;
using HycoatApi.Services.Reports;
using HycoatApi.Services.Notifications;
using HycoatApi.Services.Audit;
using HycoatApi.Services.Files;
using HycoatApi.Services.Auth;
using HycoatApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/hycoat-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(doc =>
    {
        var requirement = new OpenApiSecurityRequirement();
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc);
        requirement.Add(schemeRef, new List<string>());
        return requirement;
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

// Identity — keep EF stores for FK references and UserManager usage, but no auth
builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (corsOrigins is null || corsOrigins.Length == 0)
{
    corsOrigins = new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:3000" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Azure AD Authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new JwtBearerEvents();
    var originalOnMessageReceived = options.Events.OnMessageReceived;

    options.Events.OnMessageReceived = async context =>
    {
        if (originalOnMessageReceived != null)
        {
            await originalOnMessageReceived(context);
        }

        if (!string.IsNullOrEmpty(context.Token))
        {
            return;
        }

        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
        {
            context.Token = accessToken;
        }
    };
});

// Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISectionProfileService, SectionProfileService>();
builder.Services.AddScoped<IPowderColorService, PowderColorService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IProcessTypeService, ProcessTypeService>();
builder.Services.AddScoped<IProductionUnitService, ProductionUnitService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IFileService, FileService>();

// Sales Services
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<QuotationPdfService>();
builder.Services.AddScoped<IProformaInvoiceService, ProformaInvoiceService>();
builder.Services.AddScoped<PIPdfService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();

// Material Inward Services
builder.Services.AddScoped<IMaterialInwardService, MaterialInwardService>();
builder.Services.AddScoped<IIncomingInspectionService, IncomingInspectionService>();

// Planning Services
builder.Services.AddScoped<IProductionTimeCalcService, ProductionTimeCalcService>();
builder.Services.AddScoped<IProductionWorkOrderService, ProductionWorkOrderService>();
builder.Services.AddScoped<IProductionScheduleService, ProductionScheduleService>();

// Production Services
builder.Services.AddScoped<IPretreatmentLogService, PretreatmentLogService>();
builder.Services.AddScoped<IProductionLogService, ProductionLogService>();

// Quality Services
builder.Services.AddScoped<IInProcessInspectionService, InProcessInspectionService>();
builder.Services.AddScoped<IPanelTestService, PanelTestService>();
builder.Services.AddScoped<IFinalInspectionService, FinalInspectionService>();
builder.Services.AddScoped<ITestCertificateService, TestCertificateService>();

// Dispatch Services
builder.Services.AddScoped<IPackingListService, PackingListService>();
builder.Services.AddScoped<IDeliveryChallanService, DeliveryChallanService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDocumentBundleEmailService, DocumentBundleEmailService>();

// Purchase Services
builder.Services.AddScoped<IPowderIndentService, PowderIndentService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IGRNService, GRNService>();
builder.Services.AddScoped<IPowderStockService, PowderStockService>();
builder.Services.AddScoped<PurchaseOrderPdfService>();

// Dashboard & Reports Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IYieldReportService, YieldReportService>();
builder.Services.AddScoped<ExcelExportService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// Dev auth bypass — set "BypassAuth": true in appsettings.Development.json
var bypassAuth = builder.Configuration.GetValue<bool>("BypassAuth");
if (bypassAuth)
{
    Log.Warning(">>> AUTH BYPASS IS ENABLED — all endpoints are open <<<");
    builder.Services.AddSingleton<IAuthorizationHandler, DevBypassAuthorizationHandler>();
}

var app = builder.Build();

var enableSwagger = builder.Configuration.GetValue<bool>("Swagger:Enabled") || app.Environment.IsDevelopment();
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

if (bypassAuth)
{
    app.UseMiddleware<DevAuthBypassMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

if (!bypassAuth)
{
    app.UseMiddleware<AzureAdUserProvisioningMiddleware>();
}

app.UseStaticFiles();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
