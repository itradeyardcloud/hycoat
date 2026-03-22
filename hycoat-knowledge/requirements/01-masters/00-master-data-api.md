# 01-masters/00-master-data-api

## Feature ID
`01-masters/00-master-data-api`

## Feature Name
Master Data — API (CRUD Controllers, Services, Validation)

## Dependencies
- `00-foundation/00-api-restructuring` — BaseEntity, folder structure, NuGet packages
- `00-foundation/01-database-schema` — Entity definitions for all master entities
- `00-foundation/02-auth-system` — JWT auth, `[Authorize]`, role claims

## Business Context
Master data (Customer, SectionProfile, PowderColor, Vendor, ProcessType, ProductionUnit) is referenced throughout the entire order lifecycle. These entities must be fully CRUD-capable with search, pagination, and validation. Only Admin and Leader roles can create/edit/delete masters; all authenticated users can read.

---

## Entities (from 01-database-schema)
- **Customer** — company name, address, GSTIN, contact
- **SectionProfile** — section number, perimeter, dimensions, drawing
- **PowderColor** — powder code, color name, RAL, vendor link
- **Vendor** — name, type, address, GSTIN, contact
- **ProcessType** — process name, default rate
- **ProductionUnit** — unit name, tank/bucket/conveyor dimensions

---

## API Endpoints

### Customer Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/customers` | All auth'd | List with search + pagination |
| GET | `/api/customers/{id}` | All auth'd | Get by ID |
| POST | `/api/customers` | Admin, Leader | Create |
| PUT | `/api/customers/{id}` | Admin, Leader | Update |
| DELETE | `/api/customers/{id}` | Admin only | Soft delete |
| GET | `/api/customers/lookup` | All auth'd | Lightweight list (Id + Name only) for dropdowns |

**Query Parameters (GET list):**
- `search` (string) — searches Name, ShortName, City, GSTIN
- `page` (int, default 1)
- `pageSize` (int, default 20, max 100)
- `sortBy` (string, default "Name")
- `sortDesc` (bool, default false)

### SectionProfile Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/section-profiles` | All auth'd | List with search + pagination |
| GET | `/api/section-profiles/{id}` | All auth'd | Get by ID |
| POST | `/api/section-profiles` | Admin, Leader | Create |
| PUT | `/api/section-profiles/{id}` | Admin, Leader | Update |
| DELETE | `/api/section-profiles/{id}` | Admin only | Soft delete |
| GET | `/api/section-profiles/lookup` | All auth'd | Id + SectionNumber for dropdowns |
| POST | `/api/section-profiles/{id}/upload-drawing` | Admin, Leader | Upload drawing file |

**Query:** `search` (SectionNumber, Type), pagination, sorting.

### PowderColor Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/powder-colors` | All auth'd | List |
| GET | `/api/powder-colors/{id}` | All auth'd | Get by ID |
| POST | `/api/powder-colors` | Admin, Leader | Create |
| PUT | `/api/powder-colors/{id}` | Admin, Leader | Update |
| DELETE | `/api/powder-colors/{id}` | Admin only | Soft delete |
| GET | `/api/powder-colors/lookup` | All auth'd | Id + PowderCode + ColorName |

**Query:** `search` (PowderCode, ColorName, RALCode, Make), `vendorId` (filter), pagination.

### Vendor Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/vendors` | All auth'd | List |
| GET | `/api/vendors/{id}` | All auth'd | Get by ID |
| POST | `/api/vendors` | Admin, Leader | Create |
| PUT | `/api/vendors/{id}` | Admin, Leader | Update |
| DELETE | `/api/vendors/{id}` | Admin only | Soft delete |
| GET | `/api/vendors/lookup` | All auth'd | Id + Name |

**Query:** `search`, `vendorType` (filter: Powder, Chemical, Consumable, Other), pagination.

### ProcessType Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/process-types` | All auth'd | List (usually small — no pagination needed) |
| GET | `/api/process-types/{id}` | All auth'd | Get by ID |
| POST | `/api/process-types` | Admin only | Create |
| PUT | `/api/process-types/{id}` | Admin only | Update |
| DELETE | `/api/process-types/{id}` | Admin only | Soft delete |

### ProductionUnit Endpoints
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/production-units` | All auth'd | List |
| GET | `/api/production-units/{id}` | All auth'd | Get by ID |
| POST | `/api/production-units` | Admin only | Create |
| PUT | `/api/production-units/{id}` | Admin only | Update |
| DELETE | `/api/production-units/{id}` | Admin only | Soft delete |

---

## DTOs

### Request DTOs
Each entity has a `Create{Entity}Dto` and `Update{Entity}Dto`:

```csharp
// DTOs/Masters/CreateCustomerDto.cs
public class CreateCustomerDto
{
    public string Name { get; set; }
    public string? ShortName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

// DTOs/Masters/CreateSectionProfileDto.cs
public class CreateSectionProfileDto
{
    public string SectionNumber { get; set; }
    public string? Type { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal? WeightPerMeter { get; set; }
    public decimal? HeightMM { get; set; }
    public decimal? WidthMM { get; set; }
    public decimal? ThicknessMM { get; set; }
    public string? Notes { get; set; }
}

// Similar patterns for PowderColor, Vendor, ProcessType, ProductionUnit
```

### Response DTOs
```csharp
// DTOs/Masters/CustomerDto.cs
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ShortName { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// DTOs/Masters/CustomerDetailDto.cs (GET by ID — includes full address, notes, related counts)
public class CustomerDetailDto : CustomerDto
{
    public string? Address { get; set; }
    public string? Pincode { get; set; }
    public string? Notes { get; set; }
    public int InquiryCount { get; set; }
    public int WorkOrderCount { get; set; }
}

// DTOs/Common/LookupDto.cs (for dropdowns)
public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// DTOs/Common/SectionProfileLookupDto.cs
public class SectionProfileLookupDto
{
    public int Id { get; set; }
    public string SectionNumber { get; set; }
    public decimal PerimeterMM { get; set; }
}
```

### AutoMapper Profiles
```csharp
// Mappings/MasterMappingProfile.cs
public class MasterMappingProfile : Profile
{
    public MasterMappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerDetailDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        // ... similar for all masters
    }
}
```

---

## Validation (FluentValidation)

```csharp
// Validators/Masters/CreateCustomerValidator.cs
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator(AppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ShortName).MaximumLength(50);
        RuleFor(x => x.GSTIN).MaximumLength(15)
            .Matches(@"^\d{2}[A-Z]{5}\d{4}[A-Z]{1}[A-Z\d]{1}[Z]{1}[A-Z\d]{1}$")
            .When(x => !string.IsNullOrEmpty(x.GSTIN))
            .WithMessage("Invalid GSTIN format");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Pincode).MaximumLength(10)
            .Matches(@"^\d{6}$").When(x => !string.IsNullOrEmpty(x.Pincode))
            .WithMessage("Pincode must be 6 digits");
    }
}

// Validators/Masters/CreateSectionProfileValidator.cs
public class CreateSectionProfileValidator : AbstractValidator<CreateSectionProfileDto>
{
    public CreateSectionProfileValidator(AppDbContext db)
    {
        RuleFor(x => x.SectionNumber).NotEmpty().MaximumLength(50)
            .MustAsync(async (sn, ct) => !await db.SectionProfiles.AnyAsync(s => s.SectionNumber == sn && !s.IsDeleted, ct))
            .WithMessage("Section number already exists");
        RuleFor(x => x.PerimeterMM).GreaterThan(0);
    }
}
```

---

## Service Layer

Each master gets a service class:

```csharp
// Services/Masters/ICustomerService.cs
public interface ICustomerService
{
    Task<PagedResponse<CustomerDto>> GetAllAsync(string? search, int page, int pageSize, string sortBy, bool sortDesc);
    Task<CustomerDetailDto> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto);
    Task DeleteAsync(int id);
    Task<List<LookupDto>> GetLookupAsync();
}
```

Pattern: Controller → Service → DbContext (via EF Core). Service handles business logic, mapping, and validation. Controller is thin — delegates to service.

---

## Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CustomerDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDesc = false)
        => Ok(await _service.GetAllAsync(search, page, pageSize, sortBy, sortDesc));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> GetById(int id)
        => Ok(ApiResponse<CustomerDetailDto>.Success(await _service.GetByIdAsync(id)));

    [HttpPost]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(CreateCustomerDto dto)
        => CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CustomerDto>.Success(result));

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Leader")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(int id, UpdateCustomerDto dto)
        => Ok(ApiResponse<CustomerDto>.Success(await _service.UpdateAsync(id, dto)));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    { await _service.DeleteAsync(id); return NoContent(); }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetLookup()
        => Ok(await _service.GetLookupAsync());
}
```

---

## File Upload (Section Profile Drawing)

```csharp
[HttpPost("{id}/upload-drawing")]
[Authorize(Roles = "Admin,Leader")]
public async Task<ActionResult<ApiResponse<string>>> UploadDrawing(int id, IFormFile file)
{
    // Validate file: image or PDF, max 10MB
    // Save to: wwwroot/uploads/drawings/{id}_{timestamp}.{ext}
    // Update SectionProfile.DrawingFileUrl
    // Return the URL
}
```

**Allowed file types:** `.jpg`, `.jpeg`, `.png`, `.pdf`
**Max size:** 10MB

---

## Files to Create
| File | Purpose |
|---|---|
| `Controllers/CustomersController.cs` | Customer CRUD |
| `Controllers/SectionProfilesController.cs` | SectionProfile CRUD + drawing upload |
| `Controllers/PowderColorsController.cs` | PowderColor CRUD |
| `Controllers/VendorsController.cs` | Vendor CRUD |
| `Controllers/ProcessTypesController.cs` | ProcessType CRUD |
| `Controllers/ProductionUnitsController.cs` | ProductionUnit CRUD |
| `Services/Masters/ICustomerService.cs` + `CustomerService.cs` | Business logic |
| `Services/Masters/ISectionProfileService.cs` + `SectionProfileService.cs` | |
| `Services/Masters/IPowderColorService.cs` + `PowderColorService.cs` | |
| `Services/Masters/IVendorService.cs` + `VendorService.cs` | |
| `Services/Masters/IProcessTypeService.cs` + `ProcessTypeService.cs` | |
| `Services/Masters/IProductionUnitService.cs` + `ProductionUnitService.cs` | |
| `DTOs/Masters/Create{Entity}Dto.cs` | One per entity |
| `DTOs/Masters/Update{Entity}Dto.cs` | One per entity |
| `DTOs/Masters/{Entity}Dto.cs` | Response DTOs |
| `DTOs/Masters/{Entity}DetailDto.cs` | Detail response (where applicable) |
| `DTOs/Common/LookupDto.cs` | Dropdown lookup |
| `Validators/Masters/Create{Entity}Validator.cs` | FluentValidation |
| `Mappings/MasterMappingProfile.cs` | AutoMapper profile |

## Files to Modify
| File | Changes |
|---|---|
| `Program.cs` | Register services (DI), AutoMapper profile, FluentValidation |

## Acceptance Criteria
1. All 6 master entities have full CRUD endpoints
2. GET list returns paginated `PagedResponse` with search filter
3. Lookup endpoints return lightweight Id+Name for dropdowns
4. GSTIN validation follows Indian format regex
5. SectionNumber uniqueness enforced
6. Drawing upload saves file and updates `DrawingFileUrl`
7. Soft delete sets `IsDeleted = true` (does not physically delete)
8. Only Admin/Leader can create/update; only Admin can delete
9. All auth'd users can read
10. FluentValidation errors return 400 with structured error response
11. AutoMapper correctly maps entities ↔ DTOs
12. Swagger shows all endpoints with descriptions

## Reference
- **01-database-schema.md:** Customer, SectionProfile, PowderColor, Vendor, ProcessType, ProductionUnit field definitions
- **WORKFLOWS.md:** Module 1 — Masters
