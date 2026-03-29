using System.Text;
using HycoatApi.Data;
using HycoatApi.Middleware;
using HycoatApi.Models.Identity;
using FluentValidation;
using HycoatApi.Services;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISectionProfileService, SectionProfileService>();
builder.Services.AddScoped<IPowderColorService, PowderColorService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IProcessTypeService, ProcessTypeService>();
builder.Services.AddScoped<IProductionUnitService, ProductionUnitService>();

// Sales Services
builder.Services.AddScoped<IInquiryService, InquiryService>();

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
builder.Services.AddScoped<ExcelExportService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHttpContextAccessor();

// Dev auth bypass — set "BypassAuth": true in appsettings.Development.json
var bypassAuth = builder.Configuration.GetValue<bool>("BypassAuth");
if (bypassAuth)
{
    Log.Warning(">>> AUTH BYPASS IS ENABLED — all endpoints are open <<<");
    builder.Services.AddSingleton<IAuthorizationHandler, DevBypassAuthorizationHandler>();
}

// TODO: Add SignalR (see 10-notifications/00-notification-system.md)

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.UseStaticFiles();

app.MapControllers();

app.Run();
