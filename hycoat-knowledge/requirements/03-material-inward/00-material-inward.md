# 03-material-inward/00-material-inward

## Feature ID
`03-material-inward/00-material-inward`

## Feature Name
Material Inward Register (API + UI)

## Dependencies
- `02-sales/03-work-order` — WorkOrder entity, WO lookup, status transition (WO → MaterialReceived)
- `01-masters/00-master-data-api` — Customer, SectionProfile lookups
- `00-foundation/01-database-schema` — MaterialInward, MaterialInwardLine entities

## Business Context
When a customer's vehicle arrives with aluminum sections, SCM (Supply Chain Management / Stores) receives the material, photographs the vehicle and bundles, unloads, and fills the inward register. Each line records the section, quantity as per customer's DC, actual quantity received, and any discrepancy (excess/shortage). After logging, QA is notified for incoming inspection.

**Workflow Reference:** WORKFLOWS.md → Workflow 2 — Material Inward (SCM).

---

## Entities (from 01-database-schema)
- **MaterialInward** — InwardNumber (INW-YYYY-NNN), Date, CustomerId, WorkOrderId?, CustomerDCNumber, CustomerDCDate, VehicleNumber, UnloadingLocation, ProcessTypeId?, PowderColorId?, ReceivedByUserId, Status, Notes
- **MaterialInwardLine** — MaterialInwardId, SectionProfileId, LengthMM, QtyAsPerDC, QtyReceived, WeightKg?, Discrepancy (computed), Remarks

**Status Flow:** `Received` → `InspectionPending` → `Inspected` → `Stored`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/material-inwards` | SCM, Admin, Leader | List with pagination |
| GET | `/api/material-inwards/{id}` | SCM, QA, Admin, Leader | Detail with lines |
| POST | `/api/material-inwards` | SCM, Admin, Leader | Create with lines |
| PUT | `/api/material-inwards/{id}` | SCM, Admin, Leader | Update |
| PATCH | `/api/material-inwards/{id}/status` | SCM, QA, Admin, Leader | Status update |
| DELETE | `/api/material-inwards/{id}` | Admin | Soft delete |
| POST | `/api/material-inwards/{id}/photos` | SCM, Admin, Leader | Upload photos (vehicle, bundles) |
| GET | `/api/material-inwards/{id}/photos` | All auth'd | Get uploaded photos |

**Query Parameters:**
- `search` — InwardNumber, Customer.Name, CustomerDCNumber
- `status` — filter
- `customerId`, `workOrderId` — filters
- `fromDate`, `toDate`
- `page`, `pageSize`, `sortBy`, `sortDesc`

---

## DTOs

### CreateMaterialInwardDto
```csharp
public class CreateMaterialInwardDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? WorkOrderId { get; set; }
    public string? CustomerDCNumber { get; set; }
    public DateTime? CustomerDCDate { get; set; }
    public string? VehicleNumber { get; set; }
    public string? UnloadingLocation { get; set; }
    public int? ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public string? Notes { get; set; }
    public List<CreateMaterialInwardLineDto> Lines { get; set; }
}

public class CreateMaterialInwardLineDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int QtyAsPerDC { get; set; }
    public int QtyReceived { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Remarks { get; set; }
}
```

### MaterialInwardDetailDto
```csharp
public class MaterialInwardDetailDto
{
    public int Id { get; set; }
    public string InwardNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public int CustomerId { get; set; }
    public int? WorkOrderId { get; set; }
    public string? WONumber { get; set; }
    public string? CustomerDCNumber { get; set; }
    public DateTime? CustomerDCDate { get; set; }
    public string? VehicleNumber { get; set; }
    public string? UnloadingLocation { get; set; }
    public string? ProcessTypeName { get; set; }
    public string? PowderColorName { get; set; }
    public string? ReceivedByName { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }
    public List<MaterialInwardLineDto> Lines { get; set; }
    public List<FileAttachmentDto> Photos { get; set; }
}

public class MaterialInwardLineDto
{
    public int Id { get; set; }
    public string SectionNumber { get; set; }
    public decimal LengthMM { get; set; }
    public int QtyAsPerDC { get; set; }
    public int QtyReceived { get; set; }
    public decimal? WeightKg { get; set; }
    public int Discrepancy { get; set; }           // QtyReceived - QtyAsPerDC
    public string? Remarks { get; set; }
}
```

---

## Photo Upload

Photos are stored via the `FileAttachment` entity (EntityType="MaterialInward", EntityId=inward ID):

```csharp
[HttpPost("{id}/photos")]
[Authorize(Roles = "Admin,Leader")]  // + SCM department check
public async Task<ActionResult> UploadPhotos(int id, List<IFormFile> files)
{
    // Validate: max 10 photos per inward, each max 5MB, image types only
    // Save to: wwwroot/uploads/material-inward/{id}/
    // Create FileAttachment records
}
```

This is commonly used from mobile — SCM staff photograph the arriving vehicle and bundles directly from the app.

---

## Validation
- `CustomerId` — required, must exist
- `Date` — required
- `Lines` — at least 1 line required
- Each line: `SectionProfileId` required, `QtyAsPerDC` > 0, `QtyReceived` ≥ 0
- `Discrepancy` — auto-computed: `QtyReceived - QtyAsPerDC`
- If `WorkOrderId` provided, must reference existing WO with status allowing material receipt

---

## UI Pages

### MaterialInwardsPage (`/material-inward/inwards`)
Columns: Inward No, Date, Customer, WO Number, Vehicle No, Status, Has Photos icon.
Filters: status, customer, WO, date range.

### MaterialInwardFormPage (`/material-inward/inwards/new` or `:id`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "New Material Inward"                        │
├──────────────────────────────────────────────────────────┤
│ Work Order:        [Autocomplete — WO lookup_________▼]  │
│ Customer*:         [Auto-filled from WO / manual_____▼]  │
│ Date*:             [DatePicker]                           │
│ Customer DC No:    [________________]                    │
│ Customer DC Date:  [DatePicker]                           │
│ Vehicle Number:    [________________]                    │
│ Unloading Location:[________________]                    │
├──────────────────────────────────────────────────────────┤
│ MATERIAL LINES                                           │
│ ┌──┬──────────┬────────┬──────┬──────┬──────┬─────────┐ │
│ │# │Section*  │Len(mm) │DC Qty│Rcvd* │Disc  │Remarks  │ │
│ ├──┼──────────┼────────┼──────┼──────┼──────┼─────────┤ │
│ │1 │[M-1815▼] │[7200__]│[100_]│[98__]│ -2 🔴│[Damaged]│ │
│ │2 │[15940_▼] │[7400__]│[50__]│[50__]│  0 🟢│[_______]│ │
│ └──┴──────────┴────────┴──────┴──────┴──────┴─────────┘ │
│ [+ Add Line]                                             │
├──────────────────────────────────────────────────────────┤
│ PHOTOS (Mobile: camera button)                           │
│ [📷 Take/Upload Photo]                                   │
│ [thumbnail1] [thumbnail2] [thumbnail3]                   │
├──────────────────────────────────────────────────────────┤
│ Notes:      [__________________________________]         │
├──────────────────────────────────────────────────────────┤
│     [Cancel]                 [Save Material Inward]      │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- Selecting a WO auto-fills Customer, Process Type, Powder Color
- Discrepancy column auto-calculates (red if negative, green if zero, orange if positive)
- Photo upload supports mobile camera (`accept="image/*" capture="environment"`)
- Photo thumbnails show uploaded images with delete (X) button
- On save: status = Received, WO status → MaterialReceived

---

## Business Rules
1. InwardNumber auto-generated: INW-YYYY-NNN
2. Creating a Material Inward updates linked WO status to `MaterialReceived`
3. Discrepancy is computed, not manually entered
4. Negative discrepancy (shortage) → highlighted red → flag for customer notification
5. Photos stored via FileAttachment entity
6. SCM department access; QA gets read access for inspection reference
7. Cannot delete if IncomingInspection exists for this inward

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/MaterialInwardsController.cs` | CRUD + photo upload |
| `Services/MaterialInward/IMaterialInwardService.cs` + `MaterialInwardService.cs` | Logic |
| `DTOs/MaterialInward/CreateMaterialInwardDto.cs` | Create |
| `DTOs/MaterialInward/MaterialInwardDto.cs` | List |
| `DTOs/MaterialInward/MaterialInwardDetailDto.cs` | Detail |
| `Validators/MaterialInward/CreateMaterialInwardValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/material-inward/MaterialInwardsPage.jsx` | List |
| `src/pages/material-inward/MaterialInwardFormPage.jsx` | Form with lines + photos |
| `src/hooks/useMaterialInwards.js` | React Query hooks |
| `src/services/materialInwardService.js` | API calls |

## Acceptance Criteria
1. Inward number auto-generated: INW-YYYY-NNN
2. WO selection auto-fills customer and related fields
3. Discrepancy auto-calculated per line (color-coded)
4. Photo upload works from mobile camera
5. Photo thumbnails display with delete option
6. Creating inward updates linked WO to MaterialReceived
7. Cannot delete inward with existing inspections
8. Shortage lines highlighted for visibility
9. Mobile form is single-column with camera-friendly photo button
10. All lines visible in a scrollable table/card view

## Reference
- **WORKFLOWS.md:** Workflow 2 — Material Inward
- **01-database-schema.md:** MaterialInward, MaterialInwardLine entities
- **order-cycle-images:** Inward register document format
