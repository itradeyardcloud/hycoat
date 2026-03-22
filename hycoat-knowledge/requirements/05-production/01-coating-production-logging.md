# 05-production/01-coating-production-logging

## Feature ID
`05-production/01-coating-production-logging`

## Feature Name
Coating Production Logging (API + UI)

## Dependencies
- `04-ppc/00-production-work-order` — PWO entity
- `05-production/00-pretreatment-logging` — Pretreatment must complete before coating
- `00-foundation/01-database-schema` — ProductionLog, ProductionPhoto entities

## Business Context
After pretreatment, sections are loaded onto the conveyor for powder coating. The coating supervisor records shift parameters: conveyor speed, oven temperature, powder batch used. Every hour, a timestamped photo is taken and uploaded. The production log is essential for traceability — if a quality issue arises, the log helps trace which batch and parameters were used.

**Workflow Reference:** WORKFLOWS.md → Workflow 6 — Powder Coating Production.

---

## Entities (from 01-database-schema)
- **ProductionLog** — Date, Shift, ProductionWorkOrderId, ConveyorSpeedMtrPerMin, OvenTemperature (°C), PowderBatchNo, SupervisorUserId, Remarks
- **ProductionPhoto** — ProductionLogId, PhotoUrl, CapturedAt, UploadedByUserId, Description

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/production-logs` | Production, QA, PPC, Admin, Leader | List |
| GET | `/api/production-logs/{id}` | All auth'd | Detail with photos |
| POST | `/api/production-logs` | Production, Admin, Leader | Create |
| PUT | `/api/production-logs/{id}` | Production, Admin, Leader | Update |
| DELETE | `/api/production-logs/{id}` | Admin | Soft delete |
| GET | `/api/production-logs/by-pwo/{pwoId}` | All auth'd | Logs for a specific PWO |
| POST | `/api/production-logs/{id}/photos` | Production, Admin, Leader | Upload hourly photos |
| DELETE | `/api/production-logs/{id}/photos/{photoId}` | Production, Admin | Delete a photo |

**Query Parameters:**
- `date`, `shift`, `productionWorkOrderId`
- `search` — PWO number, Customer name
- `page`, `pageSize`

---

## DTOs

### CreateProductionLogDto
```csharp
public class CreateProductionLogDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public decimal? ConveyorSpeedMtrPerMin { get; set; }
    public decimal? OvenTemperature { get; set; }
    public string? PowderBatchNo { get; set; }
    public string? Remarks { get; set; }
}
```

### ProductionLogDetailDto
```csharp
public class ProductionLogDetailDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Shift { get; set; }
    public string PWONumber { get; set; }
    public string CustomerName { get; set; }
    public decimal? ConveyorSpeedMtrPerMin { get; set; }
    public decimal? OvenTemperature { get; set; }
    public string? PowderBatchNo { get; set; }
    public string? SupervisorName { get; set; }
    public string? Remarks { get; set; }
    public List<ProductionPhotoDto> Photos { get; set; }
}

public class ProductionPhotoDto
{
    public int Id { get; set; }
    public string PhotoUrl { get; set; }
    public DateTime CapturedAt { get; set; }
    public string? UploadedByName { get; set; }
    public string? Description { get; set; }
}
```

---

## Photo Upload

```csharp
[HttpPost("{id}/photos")]
public async Task<ActionResult> UploadPhoto(int id, IFormFile file, [FromForm] string? description)
{
    // Validate: max 20 photos per log, each max 5MB, image types only
    // Save to: wwwroot/uploads/production-logs/{id}/
    // Create ProductionPhoto record with CapturedAt = DateTime.UtcNow
}
```

Photo is captured from mobile camera every hour during the shift. The timestamp is critical for traceability.

---

## Validation
- `Date`, `Shift` — required
- `ProductionWorkOrderId` — required, must exist and be InProgress
- `ConveyorSpeedMtrPerMin` — if provided, must be 0.1–5.0 (reasonable range)
- `OvenTemperature` — if provided, must be 150–300 (°C range for powder coating)
- One production log per PWO per date per shift (unique constraint)

---

## UI Pages

### CoatingLogsPage (`/production/coating`)
Columns: Date, Shift, PWO Number, Customer, Conveyor Speed, Oven Temp, Powder Batch, Photos count.
Filters: date, shift, PWO.

### CoatingLogFormPage (`/production/coating/new` or `:id`)

**Mobile-optimized for production supervisor:**

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Coating Log Entry"                          │
├──────────────────────────────────────────────────────────┤
│ PWO*: [Autocomplete — active PWOs only_______________▼]  │
│ Date*: [Today]  Shift*: [Day ○] [Night ○]               │
├──────────────────────────────────────────────────────────┤
│ PROCESS PARAMETERS                                       │
│ Conveyor Speed (m/min): [1.2____]                        │
│ Oven Temperature (°C):  [225____]                        │
│ Powder Batch No:        [PB-2025-0042]                   │
├──────────────────────────────────────────────────────────┤
│ HOURLY PHOTOS                                            │
│ [📷 Take Photo]                                          │
│                                                          │
│ ┌────────┐ ┌────────┐ ┌────────┐                        │
│ │ 09:00  │ │ 10:00  │ │ 11:00  │                        │
│ │ [photo]│ │ [photo]│ │ [photo]│                        │
│ │   [X]  │ │   [X]  │ │   [X]  │                        │
│ └────────┘ └────────┘ └────────┘                        │
│                                                          │
│ Photos today: 3 / recommended ≥ 8 (hourly)              │
├──────────────────────────────────────────────────────────┤
│ Remarks: [________________________]                      │
├──────────────────────────────────────────────────────────┤
│     [Cancel]                 [Save Log]                  │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- Date defaults to today, shift auto-detected by time
- PWO autocomplete shows only InProgress PWOs
- "Take Photo" button uses mobile camera (`capture="environment"`)
- Photos display with timestamp overlay and delete button
- Photo count indicator shows progress toward hourly target
- Supervisor auto-tagged from logged-in user
- Warning if photo count is less than expected for elapsed shift hours

---

## Business Rules
1. One log per PWO per date per shift
2. Photos should be taken hourly — app shows count vs. expected
3. Photo timestamps are automatically recorded
4. Supervisor auto-assigned from current logged-in user
5. Production department creates logs; QA and PPC get read access
6. These logs link with QA in-process inspections (same PWO, same shift)
7. Parameters are advisory — actual may vary from PWO targets

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/ProductionLogsController.cs` | CRUD + photo upload/delete |
| `Services/Production/IProductionLogService.cs` + impl | Logic |
| `DTOs/Production/CreateProductionLogDto.cs` | Create |
| `DTOs/Production/ProductionLogDto.cs` | List |
| `DTOs/Production/ProductionLogDetailDto.cs` | Detail with photos |
| `DTOs/Production/ProductionPhotoDto.cs` | Photo |
| `Validators/Production/CreateProductionLogValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/production/CoatingLogsPage.jsx` | List |
| `src/pages/production/CoatingLogFormPage.jsx` | Form with photo gallery |
| `src/hooks/useProductionLogs.js` | React Query hooks |
| `src/services/productionLogService.js` | API calls |

## Acceptance Criteria
1. One log per PWO per date per shift enforced
2. Photo upload from mobile camera with timestamps
3. Photo gallery shows thumbnails with time and delete option
4. Photo count indicator vs. expected hourly count
5. Parameters within reasonable ranges validated
6. Date defaults to today, shift auto-detected
7. Supervisor auto-tagged
8. PWO autocomplete shows only InProgress PWOs
9. Read access for QA and PPC
10. Photos stored with proper timestamping for traceability

## Reference
- **WORKFLOWS.md:** Workflow 6 — Powder Coating Production
- **01-database-schema.md:** ProductionLog, ProductionPhoto entities
