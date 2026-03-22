# 06-quality/00-in-process-inspection

## Feature ID
`06-quality/00-in-process-inspection`

## Feature Name
In-Process Inspection (API + UI)

## Dependencies
- `04-ppc/00-production-work-order` — PWO entity
- `05-production/01-coating-production-logging` — Coating must be running
- `00-foundation/01-database-schema` — InProcessInspection, InProcessDFTReading, InProcessTestResult, PanelTest entities

## Business Context
During powder coating production, QA inspectors perform periodic quality checks at defined intervals:
- **Every 15–30 minutes**: DFT (Dry Film Thickness) measurements at 10 points per section, gloss level
- **Every 2 hours**: Adhesion cross-hatch tape peel test, MEK rub test (30 rubs), shade matching
- **Every 4 hours**: Panel tests — boiling water, impact, conical mandrel bend, pencil hardness

These real-time quality checks ensure the coating line is producing within specification. Any failure triggers immediate corrective action (adjust oven temp, conveyor speed, gun settings) before more material is ruined.

**Workflow Reference:** WORKFLOWS.md → Workflow 7A — In-Process Inspection.

---

## Entities (from 01-database-schema)
- **InProcessInspection** — Date, Time, ProductionWorkOrderId, InspectorUserId, Remarks
- **InProcessDFTReading** — InProcessInspectionId, SectionProfileId, S1–S10 (10 micron readings), MinReading, MaxReading, AvgReading, IsWithinSpec
- **InProcessTestResult** — InProcessInspectionId, TestType, InstrumentName/Make/Model, CalibrationDate, ReferenceStandard, StandardLimit, Result, Status, Remarks
- **PanelTest** — Date, ProductionWorkOrderId, 4 test results + statuses, InspectorUserId, Remarks

---

## API Endpoints

### In-Process Inspections
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/in-process-inspections` | QA, Production, PPC, Admin, Leader | List |
| GET | `/api/in-process-inspections/{id}` | All auth'd | Detail with DFT readings + test results |
| POST | `/api/in-process-inspections` | QA, Admin, Leader | Create with nested readings/results |
| PUT | `/api/in-process-inspections/{id}` | QA, Admin, Leader | Update |
| DELETE | `/api/in-process-inspections/{id}` | Admin | Soft delete |
| GET | `/api/in-process-inspections/by-pwo/{pwoId}` | All auth'd | All inspections for a PWO |
| GET | `/api/in-process-inspections/dft-trend/{pwoId}` | All auth'd | DFT readings over time for charting |

### Panel Tests
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/panel-tests` | QA, Production, Admin, Leader | List |
| GET | `/api/panel-tests/{id}` | All auth'd | Detail |
| POST | `/api/panel-tests` | QA, Admin, Leader | Create |
| PUT | `/api/panel-tests/{id}` | QA, Admin, Leader | Update |
| DELETE | `/api/panel-tests/{id}` | Admin | Soft delete |
| GET | `/api/panel-tests/by-pwo/{pwoId}` | All auth'd | Panel tests for a PWO |

**Query Parameters (inspections):**
- `date`, `productionWorkOrderId`, `inspectorUserId`
- `search` — PWO number, customer name
- `page`, `pageSize`

---

## DTOs

### CreateInProcessInspectionDto
```csharp
public class CreateInProcessInspectionDto
{
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateDFTReadingDto> DFTReadings { get; set; } = [];
    public List<CreateTestResultDto> TestResults { get; set; } = [];
}

public class CreateDFTReadingDto
{
    public int? SectionProfileId { get; set; }
    public decimal? S1 { get; set; }
    public decimal? S2 { get; set; }
    public decimal? S3 { get; set; }
    public decimal? S4 { get; set; }
    public decimal? S5 { get; set; }
    public decimal? S6 { get; set; }
    public decimal? S7 { get; set; }
    public decimal? S8 { get; set; }
    public decimal? S9 { get; set; }
    public decimal? S10 { get; set; }
}

public class CreateTestResultDto
{
    public string TestType { get; set; }
    public string? InstrumentName { get; set; }
    public string? InstrumentMake { get; set; }
    public string? InstrumentModel { get; set; }
    public DateTime? CalibrationDate { get; set; }
    public string? ReferenceStandard { get; set; }
    public string? StandardLimit { get; set; }
    public string Result { get; set; }
    public string Status { get; set; }
    public string? Remarks { get; set; }
}
```

### InProcessInspectionDetailDto
```csharp
public class InProcessInspectionDetailDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string PWONumber { get; set; }
    public string CustomerName { get; set; }
    public string? InspectorName { get; set; }
    public string? Remarks { get; set; }
    public List<DFTReadingDto> DFTReadings { get; set; }
    public List<TestResultDto> TestResults { get; set; }
}
```

### DFTTrendDto
```csharp
public class DFTTrendDto
{
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public decimal? AvgReading { get; set; }
    public decimal? MinReading { get; set; }
    public decimal? MaxReading { get; set; }
    public bool IsWithinSpec { get; set; }
}
```

### CreatePanelTestDto
```csharp
public class CreatePanelTestDto
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string? BoilingWaterResult { get; set; }
    public string? BoilingWaterStatus { get; set; }
    public string? ImpactTestResult { get; set; }
    public string? ImpactTestStatus { get; set; }
    public string? ConicalMandrelResult { get; set; }
    public string? ConicalMandrelStatus { get; set; }
    public string? PencilHardnessResult { get; set; }
    public string? PencilHardnessStatus { get; set; }
    public string? Remarks { get; set; }
}
```

---

## Validation
- `Date`, `Time`, `ProductionWorkOrderId` — required
- `ProductionWorkOrderId` — must exist and be InProgress
- DFT readings (S1–S10): each if provided must be 0–200 microns
- Min/Max/Avg computed server-side from S1–S10
- IsWithinSpec computed: true if all readings within 60–80 microns (configurable)
- TestResult.TestType must be one of: "DryFilmThickness", "Polymerisation", "Adhesion", "ShadeMatch", "GlossLevel"
- TestResult.Status must be "Pass" or "Fail"
- PanelTest statuses must be "Pass" or "Fail" if provided

---

## UI Pages

### InProcessInspectionsPage (`/quality/in-process`)
Columns: Date, Time, PWO Number, Customer, DFT Avg, Tests Count (Pass/Fail), Inspector.
Quick color indicators: green row = all pass, red = any fail.
Filters: date, PWO, status (all pass / has failures).

### InProcessInspectionFormPage (`/quality/in-process/new` or `:id`)

**Tablet-optimized form for QA inspector on the floor:**

```
┌──────────────────────────────────────────────────────────────┐
│ PageHeader: "In-Process Inspection"                          │
├──────────────────────────────────────────────────────────────┤
│ PWO*: [Autocomplete___________▼]                             │
│ Date*: [Today]  Time*: [Now ___]                             │
├──────────────────────────────────────────────────────────────┤
│ DFT READINGS (microns)              [+ Add Section Row]      │
│ ┌───────────────────────────────────────────────────────┐    │
│ │ Section: [6063T5_______▼]                             │    │
│ │ S1[65] S2[72] S3[68] S4[71] S5[69]                   │    │
│ │ S6[66] S7[73] S8[70] S9[67] S10[74]                  │    │
│ │ Min: 65  Max: 74  Avg: 69.5  ✅ Within Spec          │    │
│ └───────────────────────────────────────────────────────┘    │
│ ┌───────────────────────────────────────────────────────┐    │
│ │ Section: [________▼]                                  │    │
│ │ S1[__] S2[__] ...                                     │    │
│ └───────────────────────────────────────────────────────┘    │
├──────────────────────────────────────────────────────────────┤
│ QUALITY TESTS                                                │
│ ┌────────────┬─────────────┬──────────┬──────┐               │
│ │ Test       │ Standard    │ Result   │ P/F  │               │
│ ├────────────┼─────────────┼──────────┼──────┤               │
│ │ Adhesion   │ Cross-hatch │ [______] │ [●P] │               │
│ │ MEK Rub    │ 30 Rubs     │ [______] │ [●P] │               │
│ │ Shade      │ Visual      │ [______] │ [●P] │               │
│ │ Gloss      │ 60° meter   │ [______] │ [●P] │               │
│ └────────────┴─────────────┴──────────┴──────┘               │
│ Instrument: [Elcometer___] Make: [___] Calib: [date]         │
├──────────────────────────────────────────────────────────────┤
│ Remarks: [________________________]                          │
├──────────────────────────────────────────────────────────────┤
│     [Cancel]                 [Save Inspection]               │
└──────────────────────────────────────────────────────────────┘
```

### PanelTestFormPage (`/quality/panel-tests/new` or `:id`)

```
┌──────────────────────────────────────────────────────────────┐
│ PageHeader: "Panel Test Record"                              │
├──────────────────────────────────────────────────────────────┤
│ PWO*: [Autocomplete___________▼]  Date*: [Today]             │
├──────────────────────────────────────────────────────────────┤
│ ┌────────────────────────┬──────────────┬──────┐             │
│ │ Test                   │ Result       │ P/F  │             │
│ ├────────────────────────┼──────────────┼──────┤             │
│ │ Boiling Water (1hr)    │ [No peel up] │ [●P] │             │
│ │ Impact (35 kg·cm)      │ [No crack__] │ [●P] │             │
│ │ Conical Mandrel Bend   │ [No crack__] │ [●P] │             │
│ │ Pencil Hardness        │ [H________] │ [●P] │             │
│ └────────────────────────┴──────────────┴──────┘             │
│ Remarks: [________________________]                          │
├──────────────────────────────────────────────────────────────┤
│     [Cancel]                 [Save Panel Test]               │
└──────────────────────────────────────────────────────────────┘
```

### DFT Trend Chart (`/quality/in-process?tab=trend`)
- Line chart showing DFT avg/min/max over time for a selected PWO
- Horizontal reference lines at 60 and 80 microns (spec limits)
- Points colored red if outside spec
- Use recharts AreaChart or LineChart

**Key behaviors:**
- Date/time auto-filled (now)
- DFT readings: live-computed Min/Max/Avg as inspector types
- IsWithinSpec badge shows green (60-80) or red indicator
- Test status toggles (Pass/Fail) for quick entry
- Multiple DFT reading rows for different sections in same inspection
- Inspector auto-tagged from logged-in user

---

## Business Rules
1. DFT spec range: 60–80 microns (configurable per project)
2. Min/Max/Avg computed from S1–S10 non-null values
3. IsWithinSpec = all non-null readings ≥ 60 and ≤ 80
4. Panel tests required every 4 hours during production run
5. All inspections tied to a PWO — if PWO not InProgress, block new inspections
6. Inspector auto-assigned from logged-in user
7. Failed inspections should trigger an alert (prepare for notification system)
8. DFT trend chart helps QA spot drift before it becomes a failure

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/InProcessInspectionsController.cs` | CRUD + trend endpoint |
| `Controllers/PanelTestsController.cs` | CRUD |
| `Services/Quality/IInProcessInspectionService.cs` + impl | Logic, DFT computation |
| `Services/Quality/IPanelTestService.cs` + impl | Logic |
| `DTOs/Quality/CreateInProcessInspectionDto.cs` | Create with nested |
| `DTOs/Quality/InProcessInspectionDto.cs` | List |
| `DTOs/Quality/InProcessInspectionDetailDto.cs` | Detail |
| `DTOs/Quality/DFTReadingDto.cs` | |
| `DTOs/Quality/TestResultDto.cs` | |
| `DTOs/Quality/DFTTrendDto.cs` | For chart |
| `DTOs/Quality/CreatePanelTestDto.cs` | |
| `DTOs/Quality/PanelTestDto.cs` | |
| `Validators/Quality/CreateInProcessInspectionValidator.cs` | |
| `Validators/Quality/CreatePanelTestValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/quality/InProcessInspectionsPage.jsx` | List with trend tab |
| `src/pages/quality/InProcessInspectionFormPage.jsx` | Form with DFT grid + tests |
| `src/pages/quality/PanelTestFormPage.jsx` | Panel test form |
| `src/components/quality/DFTTrendChart.jsx` | Recharts line chart |
| `src/hooks/useInProcessInspections.js` | React Query hooks |
| `src/hooks/usePanelTests.js` | React Query hooks |
| `src/services/inProcessInspectionService.js` | API calls |
| `src/services/panelTestService.js` | API calls |

## Acceptance Criteria
1. Create inspection with DFT readings (10-point grid) and quality test results in one form submission
2. Min/Max/Avg auto-computed from S1–S10 readings (client-side live + server-side verified)
3. IsWithinSpec auto-derived (60–80 microns)
4. Pass/Fail toggles for each test type
5. DFT trend chart shows readings over time with spec limit reference lines
6. Panel test form with 4 test results + pass/fail
7. PWO autocomplete shows only InProgress PWOs
8. Inspector auto-tagged
9. Failed readings/tests highlighted visually (red)
10. Multiple DFT reading rows per inspection (different sections)

## Reference
- **WORKFLOWS.md:** Workflow 7A — In-Process Inspection
- **01-database-schema.md:** InProcessInspection, InProcessDFTReading, InProcessTestResult, PanelTest entities
