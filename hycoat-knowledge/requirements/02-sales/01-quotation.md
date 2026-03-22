# 02-sales/01-quotation

## Feature ID
`02-sales/01-quotation`

## Feature Name
Quotation Management (API + UI)

## Dependencies
- `02-sales/00-inquiry` — Inquiry entity, inquiry status transitions
- `01-masters/00-master-data-api` — Customer, ProcessType lookups
- `00-foundation/01-database-schema` — Quotation, QuotationLineItem entities

## Business Context
After receiving an inquiry, Sales prepares a quotation with generic process rates (rate per SFT for powder coating, anodizing, etc.). Quotations can be created with or without an inquiry. The quotation includes line items per process type with warranty and micron range. Once accepted, the flow moves to BOM receipt and PI.

**Workflow Reference:** WORKFLOWS.md → Workflow 1, Steps 2-3. Key Data Fields — Quotation.

---

## Entities (from 01-database-schema)
- **Quotation** — QuotationNumber (auto: QTN-YYYY-NNN), Date, InquiryId?, CustomerId, ValidityDays, Status, Notes, FileUrl, PreparedByUserId
- **QuotationLineItem** — QuotationId, ProcessTypeId, Description, RatePerSFT, WarrantyYears, MicronRange

**Status Flow:** `Draft` → `Sent` → `Accepted` | `Rejected` | `Expired`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/quotations` | Sales, Admin, Leader | List with pagination |
| GET | `/api/quotations/{id}` | Sales, Admin, Leader | Detail with line items |
| POST | `/api/quotations` | Sales, Admin, Leader | Create with line items |
| PUT | `/api/quotations/{id}` | Sales, Admin, Leader | Update with line items |
| PATCH | `/api/quotations/{id}/status` | Sales, Admin, Leader | Status update |
| DELETE | `/api/quotations/{id}` | Admin | Soft delete |
| GET | `/api/quotations/{id}/pdf` | Sales, Admin, Leader | Generate/download PDF |

**Query Parameters:**
- `search` — QuotationNumber, Customer.Name
- `status` — filter
- `customerId`, `inquiryId` — filters
- `fromDate`, `toDate`
- `page`, `pageSize`, `sortBy`, `sortDesc`

---

## DTOs

### CreateQuotationDto
```csharp
public class CreateQuotationDto
{
    public DateTime Date { get; set; }
    public int? InquiryId { get; set; }
    public int CustomerId { get; set; }
    public int ValidityDays { get; set; } = 30;
    public string? Notes { get; set; }
    public List<QuotationLineItemDto> LineItems { get; set; }
}

public class QuotationLineItemDto
{
    public int ProcessTypeId { get; set; }
    public string? Description { get; set; }
    public decimal RatePerSFT { get; set; }
    public int? WarrantyYears { get; set; }
    public string? MicronRange { get; set; }
}
```

### QuotationDetailDto (response)
```csharp
public class QuotationDetailDto
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; }
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int? InquiryId { get; set; }
    public string? InquiryNumber { get; set; }
    public int ValidityDays { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }
    public string? FileUrl { get; set; }
    public string? PreparedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuotationLineItemDto> LineItems { get; set; }
}
```

---

## PDF Generation (QuestPDF)

Generate quotation PDF at `GET /api/quotations/{id}/pdf`:

**Layout:**
```
┌─────────────────────────────────────────────┐
│ HYCOAT SYSTEMS                              │
│ [Company Address, GSTIN]                    │
├─────────────────────────────────────────────┤
│ QUOTATION                                   │
│ No: QTN-2025-001    Date: 15/01/2025       │
│ To: [Customer Name, Address]                │
├─────────────────────────────────────────────┤
│ Dear Sir/Madam,                             │
│ We are pleased to quote the following...    │
├─────────────────────────────────────────────┤
│ Sr │ Process     │ Rate/SFT │ Warranty │    │
│  1 │ Powder Coat │ ₹25.00   │ 15 yrs  │    │
│  2 │ Anodizing   │ ₹35.00   │ —       │    │
├─────────────────────────────────────────────┤
│ Terms & Conditions:                         │
│ - Validity: 30 days                         │
│ - GST extra                                 │
│ - Packing charges extra                     │
├─────────────────────────────────────────────┤
│ [Authorized Signature]                      │
└─────────────────────────────────────────────┘
```

Save PDF to `wwwroot/uploads/quotations/QTN-YYYY-NNN.pdf` and store path in `Quotation.FileUrl`.

---

## Validation
- `CustomerId` — required, must exist
- `Date` — required
- `LineItems` — at least 1 item required
- `RatePerSFT` — must be > 0
- `ProcessTypeId` — must exist
- Status transitions:
  - `Draft` → `Sent` | (edit allowed)
  - `Sent` → `Accepted` | `Rejected`
  - After 30 days (or ValidityDays) → auto-mark `Expired`
  - `Accepted` / `Rejected` / `Expired` → terminal

---

## UI Pages

### QuotationsPage (`/sales/quotations`)
Standard list with columns: QTN Number, Date, Customer, Status, Validity, PDF icon.
Filters: status, customer, date range.

### QuotationFormPage (`/sales/quotations/new` or `/sales/quotations/:id`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "New Quotation" / "Edit QTN-2025-001"        │
├──────────────────────────────────────────────────────────┤
│ Inquiry:        [Autocomplete — optional____________▼]   │
│ Customer*:      [Autocomplete — auto-fills from inquiry] │
│ Date*:          [DatePicker__________]                    │
│ Validity (days): [30____]                                │
├──────────────────────────────────────────────────────────┤
│ LINE ITEMS                                               │
│ ┌──┬──────────────┬───────────┬──────────┬────────┬───┐ │
│ │# │ Process Type* │ Rate/SFT* │ Warranty │ Micron │ X │ │
│ ├──┼──────────────┼───────────┼──────────┼────────┼───┤ │
│ │1 │ [Powder Coat▼]│ [25.00___]│ [15_____]│ [60-80]│ 🗑│ │
│ │2 │ [Anodizing__▼]│ [35.00___]│ [_______]│ [_____]│ 🗑│ │
│ └──┴──────────────┴───────────┴──────────┴────────┴───┘ │
│ [+ Add Line Item]                                        │
├──────────────────────────────────────────────────────────┤
│ Notes:         [__________________________________]      │
├──────────────────────────────────────────────────────────┤
│ [Cancel] [Save Draft] [Save & Send] [Download PDF]       │
└──────────────────────────────────────────────────────────┘
```

**Behaviors:**
- Selecting an Inquiry auto-fills Customer
- "Save Draft" → status=Draft
- "Save & Send" → status=Sent, also updates Inquiry status to `QuotationSent`
- "Download PDF" → calls `GET /api/quotations/{id}/pdf` (only visible in edit mode)
- Line items use `react-hook-form` `useFieldArray`
- Process Type select → auto-fills `DefaultRatePerSFT` from ProcessType master

---

## Business Rules
1. QuotationNumber auto-generated: QTN-YYYY-NNN
2. Sending a quotation updates linked Inquiry status to `QuotationSent`
3. Process type `DefaultRatePerSFT` auto-populates but is editable
4. PDF generated server-side via QuestPDF
5. Expired quotations (past validity date) shown with "Expired" badge
6. Cannot edit Sent/Accepted/Rejected quotations (read-only mode)

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/QuotationsController.cs` | Quotation CRUD + PDF |
| `Services/Sales/IQuotationService.cs` + `QuotationService.cs` | Business logic |
| `Services/Pdf/IQuotationPdfService.cs` + `QuotationPdfService.cs` | QuestPDF generation |
| `DTOs/Sales/CreateQuotationDto.cs` | Create |
| `DTOs/Sales/QuotationDto.cs` | List response |
| `DTOs/Sales/QuotationDetailDto.cs` | Detail response |
| `Validators/Sales/CreateQuotationValidator.cs` | Validation |
| `Mappings/SalesMappingProfile.cs` | AutoMapper (add to existing or create) |

### UI
| File | Purpose |
|---|---|
| `src/pages/sales/QuotationsPage.jsx` | List page |
| `src/pages/sales/QuotationFormPage.jsx` | Form with line items |
| `src/hooks/useQuotations.js` | React Query hooks |
| `src/services/quotationService.js` | API calls |

## Acceptance Criteria
1. QTN number auto-generated
2. Line items support add/remove with useFieldArray
3. Process type selection auto-fills default rate
4. Inquiry link auto-fills customer
5. "Save & Send" transitions quotation to Sent and inquiry to QuotationSent
6. PDF generation returns valid PDF with correct layout
7. Expired quotations show badge
8. Cannot edit non-Draft quotations
9. Mobile: line items render as stacked cards

## Reference
- **WORKFLOWS.md:** Workflow 1, Steps 2-3, Key Data Fields — Quotation
- **01-database-schema.md:** Quotation, QuotationLineItem entities
- **order-cycle-images:** Quotation document format slides
