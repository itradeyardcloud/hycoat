# 06-quality/01-final-inspection-test-cert

## Feature ID
`06-quality/01-final-inspection-test-cert`

## Feature Name
Final Inspection & Test Certificate Generation (API + UI)

## Dependencies
- `06-quality/00-in-process-inspection` — In-process data feeds final decision
- `04-ppc/00-production-work-order` — PWO entity
- `00-foundation/01-database-schema` — FinalInspection, TestCertificate entities

## Business Context
After the coating run is complete, QA performs a final inspection using AQL (Acceptable Quality Level) sampling. The inspector selects a sample from the finished lot and checks visual quality, DFT re-measurement, and shade match. If approved, a **Test Certificate (TC)** is generated as a PDF — this is a critical document that ships with every delivery and certifies that the material meets coating standards (AAMA-2604/2605 or equivalent). The TC includes all test results (DFT, MEK, adhesion, impact, bend, hardness, boiling water) and is signed by the QA Manager.

**Workflow Reference:** WORKFLOWS.md → Workflow 7B — Final Inspection.

---

## Entities (from 01-database-schema)
- **FinalInspection** — InspectionNumber (FIR-YYYY-NNN), Date, ProductionWorkOrderId, LotQuantity, SampledQuantity, VisualCheckStatus, DFTRecheckStatus, ShadeMatchFinalStatus, OverallStatus, InspectorUserId, Remarks
- **TestCertificate** — CertificateNumber (TC-YYYY-NNN), Date, FinalInspectionId, CustomerId, WorkOrderId, ProductCode (RAL), ProjectName, LotQuantity, Warranty, test result fields (Substrate, BakingTemp, BakingTime, Color, DFT, MEK, CrossCut, ConicalMandrel, BoilingWater), FileUrl

---

## API Endpoints

### Final Inspections
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/final-inspections` | QA, SCM, Admin, Leader | List |
| GET | `/api/final-inspections/{id}` | All auth'd | Detail with linked TC |
| POST | `/api/final-inspections` | QA, Admin, Leader | Create |
| PUT | `/api/final-inspections/{id}` | QA, Admin, Leader | Update |
| DELETE | `/api/final-inspections/{id}` | Admin | Soft delete |
| GET | `/api/final-inspections/by-pwo/{pwoId}` | All auth'd | Final inspection for a PWO |

### Test Certificates
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/test-certificates` | QA, SCM, Sales, Admin, Leader | List |
| GET | `/api/test-certificates/{id}` | All auth'd | Detail |
| POST | `/api/test-certificates` | QA, Admin, Leader | Create from final inspection |
| PUT | `/api/test-certificates/{id}` | QA, Admin, Leader | Update |
| DELETE | `/api/test-certificates/{id}` | Admin | Soft delete |
| POST | `/api/test-certificates/{id}/generate-pdf` | QA, Admin, Leader | Generate PDF |
| GET | `/api/test-certificates/{id}/pdf` | All auth'd | Download PDF |
| GET | `/api/test-certificates/by-work-order/{woId}` | All auth'd | TC for a work order |

---

## DTOs

### CreateFinalInspectionDto
```csharp
public class CreateFinalInspectionDto
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int LotQuantity { get; set; }
    public int SampledQuantity { get; set; }
    public string? VisualCheckStatus { get; set; }
    public string? DFTRecheckStatus { get; set; }
    public string? ShadeMatchFinalStatus { get; set; }
    public string OverallStatus { get; set; }
    public string? Remarks { get; set; }
}
```

### CreateTestCertificateDto
```csharp
public class CreateTestCertificateDto
{
    public DateTime Date { get; set; }
    public int FinalInspectionId { get; set; }
    public int CustomerId { get; set; }
    public int WorkOrderId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProjectName { get; set; }
    public int LotQuantity { get; set; }
    public string? Warranty { get; set; }
    public string? SubstrateResult { get; set; }
    public string? BakingTempResult { get; set; }
    public string? BakingTimeResult { get; set; }
    public string? ColorResult { get; set; }
    public string? DFTResult { get; set; }
    public string? MEKResult { get; set; }
    public string? CrossCutResult { get; set; }
    public string? ConicalMandrelResult { get; set; }
    public string? BoilingWaterResult { get; set; }
}
```

### TestCertificateDetailDto
```csharp
public class TestCertificateDetailDto
{
    public int Id { get; set; }
    public string CertificateNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public string WorkOrderNumber { get; set; }
    public string? ProductCode { get; set; }
    public string? ProjectName { get; set; }
    public int LotQuantity { get; set; }
    public string? Warranty { get; set; }
    // Test results
    public string? SubstrateResult { get; set; }
    public string? BakingTempResult { get; set; }
    public string? BakingTimeResult { get; set; }
    public string? ColorResult { get; set; }
    public string? DFTResult { get; set; }
    public string? MEKResult { get; set; }
    public string? CrossCutResult { get; set; }
    public string? ConicalMandrelResult { get; set; }
    public string? BoilingWaterResult { get; set; }
    // PDF
    public string? FileUrl { get; set; }
    // Final inspection summary
    public string OverallStatus { get; set; }
}
```

---

## PDF Generation (QuestPDF)

Test Certificate PDF layout:

```
┌──────────────────────────────────────────────────────────┐
│                    [HyCoat Logo]                         │
│              HYCOAT SYSTEMS PVT. LTD.                    │
│              TEST CERTIFICATE                            │
│                                                          │
│ Certificate No: TC-2025-042     Date: 15-Jun-2025        │
│ Customer: ABC Extrusions        WO: WO-2025-018         │
│ Project: Vista Tower             RAL: 9016 (White)       │
│ Lot Qty: 450 nos                Warranty: 25 Years       │
├──────────────────────────────────────────────────────────┤
│ TEST RESULTS                                             │
│ ┌──────────────────┬─────────────────┬──────────┐        │
│ │ Parameter        │ Result          │ Std Ref  │        │
│ ├──────────────────┼─────────────────┼──────────┤        │
│ │ Substrate        │ Al Profiles     │          │        │
│ │ Baking Temp      │ 200°C           │          │        │
│ │ Baking Time      │ 10 Minutes      │          │        │
│ │ Color            │ Close to std    │          │        │
│ │ DFT              │ 76.2±2.5 µm     │ 60-80µm  │        │
│ │ MEK Resistance   │ Pass (30 Rubs)  │ AAMA2604 │        │
│ │ Cross-Cut Adhesion│ No peel up     │ ISO 2409 │        │
│ │ Conical Mandrel  │ No Crack        │ ASTM D522│        │
│ │ Boiling Water    │ No peel up (1hr)│          │        │
│ └──────────────────┴─────────────────┴──────────┘        │
├──────────────────────────────────────────────────────────┤
│ Overall Status: ✅ APPROVED                               │
│ QA Manager: ___________________  Date: __________        │
│ Company Seal                                             │
└──────────────────────────────────────────────────────────┘
```

Implementation: `Services/Quality/TestCertificatePdfService.cs` using QuestPDF `Document.Create()`.

---

## Validation
- FinalInspection: `Date`, `ProductionWorkOrderId`, `OverallStatus` required
- `OverallStatus` must be "Approved", "Rejected", or "Rework"
- `SampledQuantity` must be ≤ `LotQuantity`
- Individual check statuses: "Pass" or "Fail"
- TestCertificate: `FinalInspectionId`, `CustomerId`, `WorkOrderId` required
- TC can only be created for an Approved final inspection
- `Warranty` must be "15 Years" or "25 Years" if provided

---

## UI Pages

### FinalInspectionsPage (`/quality/final-inspections`)
Columns: Inspection #, Date, PWO #, Customer, Lot Qty, Sampled, Visual, DFT, Shade, Overall Status.
Status chips: green=Approved, red=Rejected, yellow=Rework.
Action button: "Issue TC" (visible when Approved and no TC exists).

### FinalInspectionFormPage (`/quality/final-inspections/new` or `:id`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Final Inspection Report"                    │
├──────────────────────────────────────────────────────────┤
│ PWO*: [Autocomplete — completed PWOs___________▼]        │
│ Date*: [Today]                                           │
│ Lot Quantity*: [450]   Sampled Quantity*: [30]            │
├──────────────────────────────────────────────────────────┤
│ INSPECTION CHECKS                                        │
│ Visual Check:        [Pass ●] [Fail ○]                   │
│ DFT Re-check:        [Pass ●] [Fail ○]                   │
│ Shade Match Final:   [Pass ●] [Fail ○]                   │
├──────────────────────────────────────────────────────────┤
│ Overall Status*: [Approved ▼]                            │
│ Remarks: [________________________]                      │
├──────────────────────────────────────────────────────────┤
│ Summary: 3/3 checks passed → Overall: Approved           │
│                                                          │
│     [Cancel]                 [Save Inspection]           │
└──────────────────────────────────────────────────────────┘
```

### TestCertificateFormPage (`/quality/test-certificates/new` or `:id`)

Pre-filled from final inspection + in-process inspection data:

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Test Certificate"                           │
├──────────────────────────────────────────────────────────┤
│ Final Inspection*: [FIR-2025-042_▼] (auto-filled)        │
│ Customer*: [ABC Extrusions___]  WO*: [WO-2025-018]      │
│ Date*: [Today]                                           │
│ Product Code (RAL): [9016]  Project: [Vista Tower]       │
│ Lot Qty: [450]  Warranty: [25 Years ▼]                   │
├──────────────────────────────────────────────────────────┤
│ TEST RESULTS                                             │
│ Substrate:          [Aluminium Profiles]                 │
│ Baking Temperature: [200°C______________]                │
│ Baking Time:        [10 Minutes_________]                │
│ Color:              [Close to standard__]                │
│ DFT:                [76.2 +/- 2.5 micron]                │
│ MEK Resistance:     [Pass__]                             │
│ Cross-Cut Adhesion: [No peel up_________]                │
│ Conical Mandrel:    [No Crack___________]                │
│ Boiling Water:      [No peel up_________]                │
├──────────────────────────────────────────────────────────┤
│ [Cancel]    [Save Draft]    [Generate PDF & Save]        │
└──────────────────────────────────────────────────────────┘
```

### TestCertificatePDFViewer (`/quality/test-certificates/:id/preview`)
- Embedded PDF viewer (iframe or react-pdf)
- Download button
- Print button

**Key behaviors:**
- "Issue TC" button on final inspection list navigates to TC form pre-filled with data
- TC form auto-fills test results from latest in-process inspection readings (DFT avg, etc.)
- "Generate PDF" calls API endpoint, then shows preview
- Inspector auto-tagged
- Auto-numbering: FIR-YYYY-NNN, TC-YYYY-NNN

---

## Business Rules
1. Only one final inspection per PWO (unless previous was Rework — new FIR allowed)
2. TC can only be issued for Approved final inspections
3. TC number auto-generated (TC-YYYY-NNN)
4. FIR number auto-generated (FIR-YYYY-NNN)
5. TC PDF stored server-side at FileUrl for later retrieval (dispatch bundle)
6. TC test results default-filled from in-process inspection aggregated data
7. Warranty options: "15 Years", "25 Years" (based on powder spec)
8. TC is required before dispatch can proceed

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/FinalInspectionsController.cs` | CRUD |
| `Controllers/TestCertificatesController.cs` | CRUD + PDF |
| `Services/Quality/IFinalInspectionService.cs` + impl | Logic |
| `Services/Quality/ITestCertificateService.cs` + impl | Logic + auto-fill |
| `Services/Quality/TestCertificatePdfService.cs` | QuestPDF generation |
| `DTOs/Quality/CreateFinalInspectionDto.cs` | |
| `DTOs/Quality/FinalInspectionDto.cs` | |
| `DTOs/Quality/FinalInspectionDetailDto.cs` | |
| `DTOs/Quality/CreateTestCertificateDto.cs` | |
| `DTOs/Quality/TestCertificateDto.cs` | |
| `DTOs/Quality/TestCertificateDetailDto.cs` | |
| `Validators/Quality/CreateFinalInspectionValidator.cs` | |
| `Validators/Quality/CreateTestCertificateValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/quality/FinalInspectionsPage.jsx` | List |
| `src/pages/quality/FinalInspectionFormPage.jsx` | Form |
| `src/pages/quality/TestCertificateFormPage.jsx` | Form with auto-fill |
| `src/pages/quality/TestCertificatePreviewPage.jsx` | PDF viewer |
| `src/hooks/useFinalInspections.js` | React Query hooks |
| `src/hooks/useTestCertificates.js` | React Query hooks |
| `src/services/finalInspectionService.js` | API calls |
| `src/services/testCertificateService.js` | API calls |

## Acceptance Criteria
1. Final inspection form with Pass/Fail toggles for 3 checks
2. OverallStatus defaults based on check results (all pass → Approved)
3. "Issue TC" button appears on Approved inspections that don't have a TC
4. TC form pre-fills from inspection data + in-process inspection readings
5. PDF generated via QuestPDF with professional layout
6. PDF stored and downloadable
7. Auto-numbering: FIR-YYYY-NNN, TC-YYYY-NNN
8. TC blocked if final inspection not Approved
9. Only one FIR per PWO (unless Rework)
10. TC is a prerequisite for dispatch — verified by dispatch feature

## Reference
- **WORKFLOWS.md:** Workflow 7B — Final Inspection
- **01-database-schema.md:** FinalInspection, TestCertificate entities
