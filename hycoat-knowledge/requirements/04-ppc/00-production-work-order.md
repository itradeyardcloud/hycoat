# 04-ppc/00-production-work-order

## Feature ID
`04-ppc/00-production-work-order`

## Feature Name
Production Work Order (PWO) Management (API + UI)

## Dependencies
- `02-sales/03-work-order` — WorkOrder entity, WO → InProduction status transition
- `03-material-inward/00-material-inward` — Material must be received before production
- `01-masters/00-master-data-api` — SectionProfile, ProcessType, PowderColor, ProductionUnit lookups
- `00-foundation/01-database-schema` — ProductionWorkOrder, PWOLineItem, ProductionTimeCalc entities

## Business Context
PPC (Production Planning, Controlling & Coordination) creates a Production Work Order (PWO) once material is received and the Work Order is confirmed. The PWO includes section details, powder specifications, pretreatment and coating time calculations, shift allocation, and special instructions. It is distributed to Production, QA, and Purchase teams.

**Workflow Reference:** WORKFLOWS.md → Workflow 3 — Production Planning (PPC).

---

## Entities (from 01-database-schema)
- **ProductionWorkOrder** — PWONumber (PWO-YYYY-NNN), Date, WorkOrderId, CustomerId, ProcessTypeId, PowderColorId?, ProductionUnitId, PowderCode, ColorName (snapshots), PreTreatmentTimeHrs, PostTreatmentTimeHrs, TotalTimeHrs, ShiftAllocation, StartDate, DispatchDate, PackingType, SpecialInstructions, Status
- **PWOLineItem** — ProductionWorkOrderId, SectionProfileId, CustomerDCNo?, Quantity, LengthMM, PerimeterMM, UnitSurfaceAreaSqMtr, TotalSurfaceAreaSqft, SpecialInstructions
- **ProductionTimeCalc** — PWOLineItemId, ThicknessMM, WidthMM, HeightMM, SpecificWeight, WeightPerMtr, TotalWeightKg, LoadsRequired, TotalTimePreTreatMins, ConveyorSpeedMtrPerMin, JigLengthMM, GapBetweenJigsMM, TotalConveyorDistanceMtrs, TotalTimePostTreatMins

**Status Flow:** `Created` → `Scheduled` → `InProgress` → `Complete`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/production-work-orders` | PPC, Production, QA, Admin, Leader | List |
| GET | `/api/production-work-orders/{id}` | PPC, Production, QA, Admin, Leader | Detail with lines + time calc |
| POST | `/api/production-work-orders` | PPC, Admin, Leader | Create with lines |
| PUT | `/api/production-work-orders/{id}` | PPC, Admin, Leader | Update |
| PATCH | `/api/production-work-orders/{id}/status` | PPC, Production, Admin, Leader | Status update |
| DELETE | `/api/production-work-orders/{id}` | Admin | Soft delete |
| POST | `/api/production-work-orders/calculate-time` | PPC, Admin, Leader | Calculate production time |
| GET | `/api/production-work-orders/{id}/pdf` | PPC, Production, Admin, Leader | Generate PWO PDF |

---

## Production Time Calculation Logic

This is the **critical calculation** that PPC currently does in Excel. The app must replicate this:

### Pre-treatment Time
```
For each line item:
  1. Get SectionProfile dimensions (ThicknessMM, WidthMM, HeightMM)
  2. SpecificWeight = 2.71 (aluminum constant)
  3. WeightPerMtr = Perimeter × Thickness × SpecificWeight / 1,000,000
     (simplification: cross-section area × density)
  4. TotalWeightKg = WeightPerMtr × (LengthMM / 1000) × Quantity
  
  5. Get ProductionUnit tank/bucket dimensions
  6. ProfilesPerBasket = floor(BucketLengthMM / (LengthMM + gap))
     × floor(BucketWidthMM / effectiveWidth)
     (rough calculation — depends on profile shape)
  7. LoadsRequired = ceil(Quantity / ProfilesPerBasket)
  8. TimePerLoad = ~25 minutes (tank sequence: degreasing → rinse → etch → ... → dry)
  9. TotalTimePreTreatMins = LoadsRequired × TimePerLoad
```

### Post-treatment (Coating) Time
```
  10. ConveyorSpeedMtrPerMin = based on ThicknessMM:
      - < 1.5mm: 1.2 m/min
      - 1.5–2.5mm: 1.0 m/min
      - > 2.5mm: 0.8 m/min
  11. JigLengthMM = LengthMM (profile hangs vertically)
  12. GapBetweenJigsMM = 500 (standard)
  13. TotalConveyorDistanceMtrs = Quantity × (JigLengthMM + GapBetweenJigsMM) / 1000
  14. TotalTimePostTreatMins = TotalConveyorDistanceMtrs / ConveyorSpeedMtrPerMin
```

### Total
```
  TotalTimeHrs = (TotalTimePreTreatMins + TotalTimePostTreatMins) / 60
```

> **Note:** The above is simplified. The actual calculation may vary per production unit. Provide constants as configurable values.

---

## DTOs

### CreateProductionWorkOrderDto
```csharp
public class CreateProductionWorkOrderDto
{
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public int ProductionUnitId { get; set; }
    public string ShiftAllocation { get; set; }   // "Day", "Night", "Both"
    public DateTime? StartDate { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? PackingType { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<CreatePWOLineItemDto> LineItems { get; set; }
}

public class CreatePWOLineItemDto
{
    public int SectionProfileId { get; set; }
    public string? CustomerDCNo { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public string? SpecialInstructions { get; set; }
}
```

### ProductionWorkOrderDetailDto
```csharp
public class ProductionWorkOrderDetailDto
{
    public int Id { get; set; }
    public string PWONumber { get; set; }
    public DateTime Date { get; set; }
    public string WONumber { get; set; }
    public string CustomerName { get; set; }
    public string ProcessTypeName { get; set; }
    public string? PowderCode { get; set; }
    public string? ColorName { get; set; }
    public string ProductionUnitName { get; set; }
    public decimal? PreTreatmentTimeHrs { get; set; }
    public decimal? PostTreatmentTimeHrs { get; set; }
    public decimal? TotalTimeHrs { get; set; }
    public string ShiftAllocation { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? PackingType { get; set; }
    public string? SpecialInstructions { get; set; }
    public string Status { get; set; }
    public List<PWOLineItemDetailDto> LineItems { get; set; }
}

public class PWOLineItemDetailDto
{
    public int Id { get; set; }
    public string SectionNumber { get; set; }
    public string? CustomerDCNo { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal TotalSurfaceAreaSqft { get; set; }
    public string? SpecialInstructions { get; set; }
    public ProductionTimeCalcDto? TimeCalc { get; set; }
}
```

---

## Validation
- `WorkOrderId` — required, must exist, WO status must be ≥ MaterialReceived
- `ProductionUnitId` — required, must exist
- `ProcessTypeId` — required
- `LineItems` — at least 1
- Each line: `SectionProfileId` required, `Quantity` > 0, `LengthMM` > 0

---

## UI Pages

### ProductionWorkOrdersPage (`/ppc/work-orders`)
Columns: PWO Number, Date, WO Number, Customer, Powder Color, Unit, Shift, Status.
Filters: status, work order, date range.

### PWOFormPage (`/ppc/work-orders/new` or `:id`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "New Production Work Order"                  │
├──────────────────────────────────────────────────────────┤
│ Work Order*:       [Autocomplete — WO lookup_________▼]  │
│ Customer:          [Auto-filled from WO — read-only]     │
│ Date*:             [DatePicker]                           │
│ Production Unit*:  [Select — Unit-1, Unit-2__________▼]  │
│ Process Type:      [Auto from WO — read-only]            │
│ Powder Color:      [Auto from WO or manual___________▼]  │
├──────────────────────────────────────────────────────────┤
│ Shift: [Day ○] [Night ○] [Both ○]                       │
│ Start Date: [DatePicker]  Dispatch: [DatePicker]         │
│ Packing Type: [________________]                         │
├──────────────────────────────────────────────────────────┤
│ LINE ITEMS                                               │
│ ┌──┬──────────┬────────┬─────┬─────────┬───────────────┐│
│ │# │Section*  │Len(mm)*│Qty* │Area(SFT)│ Instructions  ││
│ ├──┼──────────┼────────┼─────┼─────────┼───────────────┤│
│ │1 │[M-1815▼] │[7200__]│[100]│ 247.89  │ [MILL FINISH] ││
│ └──┴──────────┴────────┴─────┴─────────┴───────────────┘│
│ [+ Add Line]                                             │
├──────────────────────────────────────────────────────────┤
│ TIME CALCULATION (auto-calculated or [Recalculate] btn)  │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Pre-treatment: 4.2 hrs │ Post-treatment: 6.8 hrs   │ │
│ │ Total estimated time: 11.0 hrs                      │ │
│ │ Loads required: 8 baskets                           │ │
│ └─────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────┤
│ Special Instructions: [__________________________________]│
├──────────────────────────────────────────────────────────┤
│     [Cancel]                 [Create PWO]                │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- WO selection auto-fills customer, process type, powder color
- Time calculation section auto-updates when line items change
- "Recalculate" button triggers `POST /api/production-work-orders/calculate-time`
- Creating PWO updates WO status to `InProduction`

---

## Business Rules
1. PWONumber auto-generated: PWO-YYYY-NNN
2. Creating a PWO updates linked WO status to `InProduction`
3. Powder code/color name snapshotted from master into PWO (for historical reference)
4. Time calculation is server-side — UI shows results
5. One or multiple PWOs can exist per WO (if order is split across units/shifts)
6. PPC department access for create/edit; Production and QA get read access

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/ProductionWorkOrdersController.cs` | CRUD + time calc + PDF |
| `Services/Planning/IProductionWorkOrderService.cs` + impl | Logic |
| `Services/Planning/IProductionTimeCalcService.cs` + impl | Time calculation engine |
| `DTOs/Planning/CreateProductionWorkOrderDto.cs` | Create |
| `DTOs/Planning/ProductionWorkOrderDto.cs` | List |
| `DTOs/Planning/ProductionWorkOrderDetailDto.cs` | Detail |
| `DTOs/Planning/ProductionTimeCalcDto.cs` | Time calc results |
| `Validators/Planning/CreatePWOValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/ppc/ProductionWorkOrdersPage.jsx` | List |
| `src/pages/ppc/PWOFormPage.jsx` | Form with time calc |
| `src/hooks/useProductionWorkOrders.js` | React Query hooks |
| `src/services/productionWorkOrderService.js` | API calls |

## Acceptance Criteria
1. PWO number auto-generated: PWO-YYYY-NNN
2. WO selection auto-fills related fields
3. Production time calculated server-side (pre-treatment + post-treatment)
4. Time calculation results display in dedicated section
5. Creating PWO updates WO status to InProduction
6. Shift allocation (Day/Night/Both) saved
7. Powder code and color name snapshotted
8. Surface area auto-calculated per line
9. Multiple PWOs per WO supported
10. Read access for Production and QA teams

## Reference
- **WORKFLOWS.md:** Workflow 3 — Production Planning (PPC), Key Data Fields — PWO
- **01-database-schema.md:** ProductionWorkOrder, PWOLineItem, ProductionTimeCalc entities
