# 02-sales/00-inquiry

## Feature ID
`02-sales/00-inquiry`

## Feature Name
Inquiry Management (API + UI)

## Dependencies
- `00-foundation/00-api-restructuring` — Base project structure
- `00-foundation/01-database-schema` — Inquiry entity
- `00-foundation/02-auth-system` — JWT, roles
- `01-masters/00-master-data-api` — Customer, ProcessType lookups
- `01-masters/01-master-data-ui` — Customer, ProcessType hooks/services
- `00-foundation/05-app-shell-layout` — Layout, DataTable, routes

## Business Context
Sales receives customer inquiries via email, phone, WhatsApp, or walk-in. Each inquiry is logged, assigned to a sales person, and tracked through its lifecycle. Inquiries are the starting point of the entire order cycle. An inquiry eventually leads to a Quotation (next feature).

**Workflow Reference:** WORKFLOWS.md → Workflow 1 — Sales & Order Acquisition, Step 1.

---

## Entities (from 01-database-schema)
- **Inquiry** — InquiryNumber (auto: INQ-YYYY-NNN), Date, CustomerId, ProjectName, Source, ContactPerson/Email/Phone, ProcessTypeId, Description, Status, AssignedToUserId

**Status Flow:** `New` → `QuotationSent` → `BOMReceived` → `PISent` → `Confirmed` → `Closed` | `Lost`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/inquiries` | Sales, Admin, Leader | List with search + filter + pagination |
| GET | `/api/inquiries/{id}` | Sales, Admin, Leader | Get detail |
| POST | `/api/inquiries` | Sales, Admin, Leader | Create |
| PUT | `/api/inquiries/{id}` | Sales, Admin, Leader | Update |
| PATCH | `/api/inquiries/{id}/status` | Sales, Admin, Leader | Update status only |
| DELETE | `/api/inquiries/{id}` | Admin | Soft delete |
| GET | `/api/inquiries/stats` | Sales, Admin, Leader | Count by status for dashboard cards |

**Query Parameters (GET list):**
- `search` — InquiryNumber, Customer.Name, ProjectName
- `status` — filter by status string
- `customerId` — filter by customer
- `fromDate`, `toDate` — date range filter
- `assignedToUserId` — filter by sales person
- `page`, `pageSize`, `sortBy`, `sortDesc`

**Stats Response:**
```json
{
  "new": 5,
  "quotationSent": 12,
  "bomReceived": 3,
  "piSent": 7,
  "confirmed": 25,
  "lost": 2,
  "total": 54
}
```

---

## DTOs

### CreateInquiryDto
```csharp
public class CreateInquiryDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string? ProjectName { get; set; }
    public string Source { get; set; }          // "Email", "Phone", "WhatsApp", "Walk-in", "Tender"
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int? ProcessTypeId { get; set; }
    public string? Description { get; set; }
    public string? AssignedToUserId { get; set; }
}
```

### InquiryDto (list response)
```csharp
public class InquiryDto
{
    public int Id { get; set; }
    public string InquiryNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public string? ProjectName { get; set; }
    public string Source { get; set; }
    public string? ProcessTypeName { get; set; }
    public string Status { get; set; }
    public string? AssignedToName { get; set; }
}
```

### InquiryDetailDto (single response)
```csharp
public class InquiryDetailDto : InquiryDto
{
    public int CustomerId { get; set; }
    public int? ProcessTypeId { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Description { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int QuotationCount { get; set; }      // Number of linked quotations
}
```

### UpdateStatusDto
```csharp
public class UpdateStatusDto
{
    public string Status { get; set; }
}
```

---

## Validation
- `CustomerId` — required, must exist
- `Source` — required, must be one of: Email, Phone, WhatsApp, Walk-in, Tender
- `Date` — required, not in the future
- `Status` transitions — validate allowed transitions:
  - `New` → `QuotationSent` | `Lost`
  - `QuotationSent` → `BOMReceived` | `Lost`
  - `BOMReceived` → `PISent` | `Lost`
  - `PISent` → `Confirmed` | `Lost`
  - `Confirmed` → `Closed`
  - `Lost` → (terminal)
  - `Closed` → (terminal)

---

## Auto-Number Generation
```csharp
// InquiryNumber format: INQ-2025-001
// Logic: Find max InquiryNumber for current year → increment counter
// Thread-safe: Use database sequence or MAX query inside transaction
```

---

## UI Pages

### InquiriesPage (`/sales/inquiries`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Inquiries"                 [+ New Inquiry]  │
├──────────────────────────────────────────────────────────┤
│ Status Cards (horizontal scroll on mobile):              │
│ [New: 5] [QTN Sent: 12] [BOM Rcvd: 3] [PI Sent: 7] ... │
├──────────────────────────────────────────────────────────┤
│ Filters: [Search____] [Status ▼] [Customer ▼] [Date ▼]  │
├──────────────────────────────────────────────────────────┤
│ DataTable:                                               │
│ # │ Inquiry No │ Date    │ Customer │ Project │ Status │⋮│
│ 1 │ INQ-25-005 │ 15 Jan  │ Ajit Co  │ Pune Pr │ [New] │⋮│
│ 2 │ INQ-25-004 │ 14 Jan  │ XYZ Ltd  │ Mumbai  │ [PI] │⋮│
│ ...                                                      │
├──────────────────────────────────────────────────────────┤
│ Pagination                                               │
└──────────────────────────────────────────────────────────┘
```

**Status cards:** Clickable — clicking a card filters the table to that status. Uses `GET /api/inquiries/stats`.

### InquiryFormPage (`/sales/inquiries/new` or `/sales/inquiries/:id`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "New Inquiry" / "Edit INQ-2025-005"          │
├──────────────────────────────────────────────────────────┤
│ Customer*:      [Autocomplete — Customer lookup______▼]  │
│ Date*:          [DatePicker__________]                    │
│ Source*:        [Select: Email/Phone/WhatsApp/Walk-in ▼] │
│ Project Name:   [________________________]               │
│ Process Type:   [Autocomplete — ProcessType lookup___▼]  │
├──────────────────────────────────────────────────────────┤
│ Contact Person: [________________________]               │
│ Contact Email:  [________________________]               │
│ Contact Phone:  [________________________]               │
├──────────────────────────────────────────────────────────┤
│ Assigned To:    [Select — Sales users______________▼]    │
│ Description:    [Multiline textarea_________________]    │
│                 [__________________________________]     │
├──────────────────────────────────────────────────────────┤
│           [Cancel]                      [Save Inquiry]   │
└──────────────────────────────────────────────────────────┘
```

**Edit mode additions:**
- Show `InquiryNumber` (read-only), current `Status` chip
- Status action buttons: e.g., if status is "New", show [Mark as Quotation Sent] button
- Show linked quotation count with link to quotations filtered by this inquiry

---

## React Query Hooks (`src/hooks/useInquiries.js`)
- `useInquiries(params)` — paginated list
- `useInquiry(id)` — single detail
- `useInquiryStats()` — status counts
- `useCreateInquiry()` — mutation
- `useUpdateInquiry()` — mutation
- `useUpdateInquiryStatus()` — mutation (PATCH)
- `useDeleteInquiry()` — mutation

---

## Business Rules
1. InquiryNumber is auto-generated on create — never user-editable
2. New inquiries start in `New` status
3. Status transitions are enforced (see flow above)
4. Sales department users see only their assigned inquiries (filter by `AssignedToUserId`). Leader/Admin see all.
5. Deleting an inquiry with linked quotations should be blocked (return 400)
6. Customer autocomplete field auto-fills contact details from Customer master if available

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/InquiriesController.cs` | Inquiry API endpoints |
| `Services/Sales/IInquiryService.cs` + `InquiryService.cs` | Business logic |
| `DTOs/Sales/CreateInquiryDto.cs` | Create request |
| `DTOs/Sales/UpdateInquiryDto.cs` | Update request |
| `DTOs/Sales/InquiryDto.cs` | List response |
| `DTOs/Sales/InquiryDetailDto.cs` | Detail response |
| `DTOs/Sales/InquiryStatsDto.cs` | Stats response |
| `DTOs/Common/UpdateStatusDto.cs` | Status patch |
| `Validators/Sales/CreateInquiryValidator.cs` | FluentValidation |

### UI
| File | Purpose |
|---|---|
| `src/pages/sales/InquiriesPage.jsx` | Inquiry list + status cards |
| `src/pages/sales/InquiryFormPage.jsx` | Create/edit form |
| `src/hooks/useInquiries.js` | React Query hooks |
| `src/services/inquiryService.js` | API calls |

## Acceptance Criteria
1. Inquiry number auto-generated as INQ-YYYY-NNN
2. List page shows paginated inquiries with search and status/customer/date filters
3. Status stat cards render with correct counts and act as filters
4. Create form validates required fields (Customer, Date, Source)
5. Edit form loads existing data and shows status chip
6. Status transitions enforced — invalid transitions return 400
7. Customer autocomplete populates contact fields
8. Mobile: list renders as cards; form is single-column
9. Toast on success/error
10. Linked quotation count visible on detail view

## Reference
- **WORKFLOWS.md:** Workflow 1 — Sales & Order Acquisition, Steps 1-2
- **01-database-schema.md:** Inquiry entity
- **order-cycle-images:** Slides showing inquiry/quotation flow
