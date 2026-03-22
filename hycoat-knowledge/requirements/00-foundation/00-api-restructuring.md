# 00-foundation/00-api-restructuring

## Feature ID
`00-foundation/00-api-restructuring`

## Feature Name
API Project Restructuring

## Dependencies
None — this is the first step.

## Business Context
The existing `hycoat-api` project is a scaffolded .NET 10 Web API with a placeholder `Product` model and controller using the `DotnetApi` namespace. Before any features can be built, the project must be restructured with proper namespacing, folder organization, base classes, and required NuGet packages.

## Current State
- **Namespace:** `DotnetApi` (in `Program.cs` usings, `Data/AppDbContext.cs`, `Models/Product.cs`, `Controllers/ProductsController.cs`)
- **Root Namespace in .csproj:** `HycoatApi` (already set but code files use `DotnetApi`)
- **Existing packages:** EF Core 10, SQL Server, Swagger
- **Database name in connection string:** `DotnetApiDb`

## Tasks

### 1. Rename Namespace
Replace all occurrences of `DotnetApi` with `HycoatApi` across:
- `Program.cs` — `using DotnetApi.Data;` → `using HycoatApi.Data;`
- `Data/AppDbContext.cs` — `namespace DotnetApi.Data` → `namespace HycoatApi.Data`
- `Models/Product.cs` — `namespace DotnetApi.Models` → `namespace HycoatApi.Models`
- `Controllers/ProductsController.cs` — `namespace DotnetApi.Controllers` + usings

### 2. Update Connection String
In `appsettings.json`, change database name:
```json
"DefaultConnection": "Server=localhost;Database=HycoatDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### 3. Add NuGet Packages
Add these to `hycoat-api.csproj`:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.11.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="QuestPDF" Version="2025.3.1" />
```

> **Note:** Use the latest stable versions available. The versions above are approximate — check NuGet for current versions at build time.

### 4. Create Folder Structure
Create empty folders (with placeholder files or `.gitkeep` if needed):
```
hycoat-api/
├── Controllers/        (exists — keep, will be cleaned up)
├── Models/
│   ├── Identity/
│   ├── Masters/
│   ├── Sales/
│   ├── MaterialInward/
│   ├── Planning/
│   ├── Production/
│   ├── Quality/
│   ├── Dispatch/
│   ├── Purchase/
│   └── Common/
├── DTOs/
│   ├── Auth/
│   ├── Masters/
│   ├── Sales/
│   ├── MaterialInward/
│   ├── Planning/
│   ├── Production/
│   ├── Quality/
│   ├── Dispatch/
│   └── Purchase/
├── Services/
├── Data/
│   ├── Configurations/
│   └── (AppDbContext.cs exists)
├── Middleware/
├── Helpers/
└── Migrations/
```

### 5. Create Base Entity
Create `Models/Common/BaseEntity.cs`:
```csharp
namespace HycoatApi.Models.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### 6. Create API Response Wrapper
Create `DTOs/ApiResponse.cs`:
```csharp
namespace HycoatApi.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error) =>
        new() { Success = false, Errors = new List<string> { error } };

    public static ApiResponse<T> Fail(List<string> errors) =>
        new() { Success = false, Errors = errors };
}
```

Create `DTOs/PagedResponse.cs`:
```csharp
namespace HycoatApi.DTOs;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

### 7. Create Exception Handling Middleware
Create `Middleware/ExceptionHandlingMiddleware.cs`:
```csharp
namespace HycoatApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var response = new { success = false, message = "An internal error occurred.", errors = new[] { ex.Message } };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

### 8. Update Program.cs
Update `Program.cs` with the new namespace, Serilog, exception middleware, and placeholders for future services:
```csharp
using HycoatApi.Data;
using HycoatApi.Middleware;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

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

// TODO: Add Authentication (see 02-auth-system.md)
// TODO: Add FluentValidation (registered per feature)
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

// TODO: app.UseAuthentication(); (see 02-auth-system.md)
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### 9. Remove Placeholder Product Code
- **Do NOT delete** `Product.cs`, `ProductsController.cs` yet — they serve as a reference. Mark with `// TODO: Remove after masters are built` comments.
- Alternatively, keep them working so the project remains runnable during restructuring.

### 10. Add JWT Settings Placeholder
Add to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HycoatDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "CHANGE-THIS-TO-A-SECURE-SECRET-KEY-MIN-64-CHARS-LONG-FOR-HMAC-SHA256",
    "Issuer": "HycoatApi",
    "Audience": "HycoatApp",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "FileStorage": {
    "BasePath": "uploads",
    "MaxFileSizeMB": 10
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Files to Create
| File | Purpose |
|---|---|
| `Models/Common/BaseEntity.cs` | Base entity with audit fields + soft delete |
| `DTOs/ApiResponse.cs` | Standardized API response wrapper |
| `DTOs/PagedResponse.cs` | Paginated response wrapper |
| `Middleware/ExceptionHandlingMiddleware.cs` | Global exception handler |
| `Helpers/ClaimsHelper.cs` | Extract user/role/dept from JWT claims (create empty, implement in 02-auth) |

## Files to Modify
| File | Changes |
|---|---|
| `hycoat-api.csproj` | Add NuGet packages, verify RootNamespace is `HycoatApi` |
| `Program.cs` | Rename usings, add Serilog, add middleware, add AutoMapper |
| `Data/AppDbContext.cs` | Rename namespace to `HycoatApi.Data` |
| `Models/Product.cs` | Rename namespace to `HycoatApi.Models` (temporary, will be removed later) |
| `Controllers/ProductsController.cs` | Rename namespace + usings (temporary) |
| `appsettings.json` | Update DB name, add JWT + FileStorage placeholders |

## Acceptance Criteria
1. `dotnet build` succeeds with zero errors
2. All `.cs` files use `HycoatApi.*` namespaces — no `DotnetApi` references remain
3. `appsettings.json` has `HycoatDb` database name, `JwtSettings`, and `FileStorage` sections
4. Folder structure exists: `Models/{Identity,Masters,...}`, `DTOs/{Auth,Masters,...}`, `Services/`, `Middleware/`, `Helpers/`
5. `BaseEntity.cs` exists with `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`
6. `ApiResponse<T>` and `PagedResponse<T>` exist and compile
7. `ExceptionHandlingMiddleware` is registered in `Program.cs`
8. Serilog is configured and logging to console + file
9. Running `dotnet run` starts the API and Swagger UI is accessible
10. Existing Product CRUD endpoints still work (not broken by restructuring)

## Reference
- **WORKFLOWS.md:** N/A (infrastructure task)
- **Current files:** See `hycoat-api/Program.cs`, `hycoat-api/hycoat-api.csproj`, `hycoat-api/appsettings.json`
