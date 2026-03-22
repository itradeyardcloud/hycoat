# 03-material-inward/01-incoming-inspection

## Feature ID
`03-material-inward/01-incoming-inspection`

## Feature Name
Incoming Material Inspection (API + UI)

## Dependencies
- `03-material-inward/00-material-inward` — MaterialInward entity, inward lines
- `00-foundation/01-database-schema` — IncomingInspection, IncomingInspectionLine entities

## Business Context
After material is received and logged, QA Lab conducts an incoming inspection. They open 2-3 bundles, check for watermarks, scratches, dents, and dimensional accuracy. Each material line is inspected individually. If defects are found, the customer is informed and buffing charges may apply. The inspection result (Pass/Fail/Conditional) determines whether material proceeds to production.

**Workflow Reference:** WORKFLOWS.md → Workflow 2 — QA: Incoming Material Inspection.

---

## Entities (from 01-database-schema)
- **IncomingInspection** — InspectionNumber (IIR-YYYY-NNN), Date, MaterialInwardId, InspectedByUserId, OverallStatus, Remarks
- **IncomingInspectionLine** — IncomingInspectionId, MaterialInwardLineId, WatermarkOk?, ScratchOk?, DentOk?, DimensionalCheckOk?, BuffingRequired, BuffingCharge?, Status, Remarks

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/incoming-inspections` | QA, Admin, Leader | List |
| GET | `/api/incoming-inspections/{id}` | QA, SCM, Admin, Leader | Detail with lines |
| POST | `/api/incoming-inspections` | QA, Admin, Leader | Create |
| PUT | `/api/incoming-inspections/{id}` | QA, Admin, Leader | Update |
| DELETE | `/api/incoming-inspections/{id}` | Admin | Soft delete |
| POST | `/api/incoming-inspections/{id}/photos` | QA | Upload defect photos |

**Query Parameters:**
- `search` — InspectionNumber, Customer.Name
- `overallStatus` — Pass, Fail, Conditional
- `materialInwardId` — filter
- `fromDate`, `toDate`
- `page`, `pageSize`

---

## DTOs

### CreateIncomingInspectionDto
```csharp
public class CreateIncomingInspectionDto
{
    public DateTime Date { get; set; }
    public int MaterialInwardId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateIncomingInspectionLineDto> Lines { get; set; }
}

public class CreateIncomingInspectionLineDto
{
    public int MaterialInwardLineId { get; set; }
    public bool? WatermarkOk { get; set; }
    public bool? ScratchOk { get; set; }
    public bool? DentOk { get; set; }
    public bool? DimensionalCheckOk { get; set; }
    public bool BuffingRequired { get; set; }
    public decimal? BuffingCharge { get; set; }
    public string Status { get; set; }      // "Pass", "Fail", "Conditional"
    public string? Remarks { get; set; }
}
```

### IncomingInspectionDetailDto
```csharp
public class IncomingInspectionDetailDto
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; }
    public DateTime Date { get; set; }
    public int MaterialInwardId { get; set; }
    public string InwardNumber { get; set; }
    public string CustomerName { get; set; }
    public string? InspectedByName { get; set; }
    public string OverallStatus { get; set; }
    public string? Remarks { get; set; }
    public List<IncomingInspectionLineDetailDto> Lines { get; set; }
}

public class IncomingInspectionLineDetailDto
{
    public int Id { get; set; }
    public string SectionNumber { get; set; }
    public int QtyReceived { get; set; }
    public bool? WatermarkOk { get; set; }
    public bool? ScratchOk { get; set; }
    public bool? DentOk { get; set; }
    public bool? DimensionalCheckOk { get; set; }
    public bool BuffingRequired { get; set; }
    public decimal? BuffingCharge { get; set; }
    public string Status { get; set; }
    public string? Remarks { get; set; }
}
```

---

## Validation
- `MaterialInwardId` — required, must exist, must not already have an inspection
- `Date` — required
- `Lines` — must have at least 1 line (typically one per material inward line)
- Each line: `Status` must be "Pass", "Fail", or "Conditional"
- If `BuffingRequired` = true, `BuffingCharge` may be populated (≥ 0)
- OverallStatus auto-derived: if any line is "Fail" → overall "Fail"; if any "Conditional" → "Conditional"; else "Pass"

---

## UI Pages

### IncomingInspectionsPage (`/material-inward/inspections`)
Columns: Inspection No, Date, Inward No, Customer, Overall Status (chip), Inspector.
Filters: status, date range.

### IncomingInspectionFormPage (`/material-inward/inspections/new` or `:id`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "New Incoming Inspection"                    │
├──────────────────────────────────────────────────────────┤
│ Material Inward*: [Autocomplete — inward lookup______▼]  │
│ Date*:            [DatePicker]                            │
│ Customer:         [Auto-filled from inward — read-only]  │
│ Inward Number:    [Auto-filled — read-only]              │
├──────────────────────────────────────────────────────────┤
│ INSPECTION LINES (auto-populated from inward lines)      │
│                                                          │
│ ┌─ Section: M-1815 (98 pcs) ──────────────────────────┐ │
│ │ Watermark:  [✓ OK] [✗ Defect]                       │ │
│ │ Scratch:    [✓ OK] [✗ Defect]                       │ │
│ │ Dent:       [✓ OK] [✗ Defect]                       │ │
│ │ Dimensional:[✓ OK] [✗ Defect]                       │ │
│ │ ─────────────────────────────                       │ │
│ │ Buffing Req: [☐ No] [☑ Yes]  Charge: [₹____]      │ │
│ │ Status:      [Pass ▼]                               │ │
│ │ Remarks:     [________________________]             │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                          │
│ ┌─ Section: 15940 (50 pcs) ───────────────────────────┐ │
│ │ (same structure)                                     │ │
│ └─────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────┤
│ DEFECT PHOTOS                                            │
│ [📷 Upload Photo]                                        │
│ [thumbnail1] [thumbnail2]                                │
├──────────────────────────────────────────────────────────┤
│ Overall Status: [Auto-derived: Pass/Fail/Conditional]    │
│ Remarks:        [__________________________________]     │
├──────────────────────────────────────────────────────────┤
│     [Cancel]              [Save Inspection Report]       │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- Selecting a Material Inward auto-populates inspection lines from inward lines
- Each check (Watermark, Scratch, Dent, Dimensional) is a toggle button pair (OK / Defect)
- Null = not checked; true = OK; false = defect found
- Buffing Required toggle shows charge field when enabled
- Overall Status auto-derives from individual line statuses
- On save: MaterialInward status → `Inspected`
- Mobile: Each section renders as a card with toggle buttons (easy for QA inspector on tablet)

---

## Business Rules
1. InspectionNumber auto-generated: IIR-YYYY-NNN
2. One inspection per Material Inward (validate uniqueness)
3. Inspection lines auto-populated from MaterialInward lines (user doesn't manually add)
4. OverallStatus derived: any "Fail" → "Fail"; any "Conditional" → "Conditional"; all "Pass" → "Pass"
5. Creating inspection updates MaterialInward status to `Inspected` (and then `Stored` after storage)
6. Defect photos link via FileAttachment (EntityType="IncomingInspection")
7. If overall status is "Fail", the linked WO should NOT auto-advance — requires manual resolution
8. QA department access for create/edit; SCM gets read access

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/IncomingInspectionsController.cs` | CRUD + photos |
| `Services/MaterialInward/IIncomingInspectionService.cs` + `IncomingInspectionService.cs` | |
| `DTOs/MaterialInward/CreateIncomingInspectionDto.cs` | |
| `DTOs/MaterialInward/IncomingInspectionDto.cs` | List |
| `DTOs/MaterialInward/IncomingInspectionDetailDto.cs` | Detail |
| `Validators/MaterialInward/CreateIncomingInspectionValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/material-inward/IncomingInspectionsPage.jsx` | List |
| `src/pages/material-inward/IncomingInspectionFormPage.jsx` | Form with checklist |
| `src/hooks/useIncomingInspections.js` | React Query hooks |
| `src/services/incomingInspectionService.js` | API calls |

## Acceptance Criteria
1. Inspection number auto-generated: IIR-YYYY-NNN
2. Selecting an inward auto-populates inspection lines
3. Each parameter is a toggle (OK/Defect/Not Checked)
4. Overall status auto-derives from individual line statuses
5. One inspection per inward (duplicate blocked)
6. Defect photos uploadable from mobile camera
7. MaterialInward status updates on save
8. Buffing charge field shows only when buffing is required
9. Mobile: card layout per section line with large toggle buttons
10. Cannot create inspection for non-existent or already-inspected inward

## Reference
- **WORKFLOWS.md:** Workflow 2 — QA: Incoming Material Inspection
- **01-database-schema.md:** IncomingInspection, IncomingInspectionLine entities
