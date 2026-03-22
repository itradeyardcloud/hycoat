# 02-sales/03-work-order

## Feature ID
`02-sales/03-work-order`

## Feature Name
Work Order Management (API + UI)

## Dependencies
- `02-sales/02-proforma-invoice` — PI link (optional), customer/section data flow
- `01-masters/00-master-data-api` — Customer, ProcessType, PowderColor lookups
- `00-foundation/01-database-schema` — WorkOrder entity

## Business Context
Once the customer confirms (signs PI, sends email, or issues a formal Work Order), Sales creates an internal Work Order (WO). The WO is the **central tracking entity** — it links all downstream operations: Material Inward, Production Work Order, Quality Inspection, Dispatch, and Invoice. Every operation references back to a WO.

**Workflow Reference:** WORKFLOWS.md → Workflow 1, Steps 7-9 — "Receive PI confirmation → Create internal Job/Work Order."

---

## Entities (from 01-database-schema)
- **WorkOrder** — WONumber (WO-YYYY-NNN), Date, CustomerId, ProformaInvoiceId?, ProjectName, ProcessTypeId, PowderColorId?, DispatchDate, Status, Notes

**Status Flow:**
```
Created → MaterialAwaited → MaterialReceived → InProduction → QAComplete → Dispatched → Invoiced → Closed
```
Each status represents a milestone in the order lifecycle.

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/work-orders` | Sales, PPC, SCM, Admin, Leader | List with filters |
| GET | `/api/work-orders/{id}` | All auth'd | Detail with full lifecycle status |
| POST | `/api/work-orders` | Sales, Admin, Leader | Create |
| PUT | `/api/work-orders/{id}` | Sales, Admin, Leader | Update |
| PATCH | `/api/work-orders/{id}/status` | Sales, PPC, SCM, QA, Admin, Leader | Status transition |
| DELETE | `/api/work-orders/{id}` | Admin | Soft delete |
| GET | `/api/work-orders/stats` | All auth'd | Count by status |
| GET | `/api/work-orders/{id}/timeline` | All auth'd | Full lifecycle timeline |

**Query Parameters:**
- `search` — WONumber, Customer.Name, ProjectName
- `status` — filter (supports multiple: `status=Created&status=InProduction`)
- `customerId`, `processTypeId`, `powderColorId` — filters
- `fromDate`, `toDate` — by WO date or dispatch date
- `page`, `pageSize`, `sortBy`, `sortDesc`

---

## DTOs

### CreateWorkOrderDto
```csharp
public class CreateWorkOrderDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? ProformaInvoiceId { get; set; }
    public string? ProjectName { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? Notes { get; set; }
}
```

### WorkOrderDetailDto (response — the key tracking view)
```csharp
public class WorkOrderDetailDto
{
    public int Id { get; set; }
    public string WONumber { get; set; }
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string? ProjectName { get; set; }
    public string ProcessTypeName { get; set; }
    public string? PowderColorName { get; set; }
    public string? PowderCode { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }

    // Linked documents summary
    public int? ProformaInvoiceId { get; set; }
    public string? PINumber { get; set; }
    public int MaterialInwardCount { get; set; }
    public int ProductionWorkOrderCount { get; set; }
    public int DeliveryChallanCount { get; set; }
    public int InvoiceCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### WorkOrderTimelineDto
```csharp
public class WorkOrderTimelineDto
{
    public int WorkOrderId { get; set; }
    public string WONumber { get; set; }
    public List<TimelineEventDto> Events { get; set; }
}

public class TimelineEventDto
{
    public string Stage { get; set; }       // "Created", "MaterialReceived", etc.
    public string? DocumentNumber { get; set; } // e.g., "INW-2025-001"
    public DateTime? Date { get; set; }
    public string? PerformedBy { get; set; }
    public bool IsComplete { get; set; }
}
```

---

## Timeline Endpoint Logic

`GET /api/work-orders/{id}/timeline` builds a timeline by querying linked entities:

```
1. Created          — WorkOrder.CreatedAt
2. MaterialAwaited  — WO status set
3. MaterialReceived — MaterialInward record exists (latest date)
4. InProduction     — ProductionWorkOrder exists (latest PWO date)
5. QAComplete       — FinalInspection exists with status "Approved"
6. Dispatched       — DeliveryChallan exists
7. Invoiced         — Invoice exists
8. Closed           — WO status set to Closed
```

---

## Validation
- `CustomerId` — required, must exist
- `ProcessTypeId` — required, must exist
- `Date` — required
- `DispatchDate` — if provided, must be ≥ Date
- Status transitions:
  - `Created` → `MaterialAwaited`
  - `MaterialAwaited` → `MaterialReceived` (triggered by Material Inward creation)
  - `MaterialReceived` → `InProduction` (triggered by PWO creation)
  - `InProduction` → `QAComplete` (triggered by Final Inspection approval)
  - `QAComplete` → `Dispatched` (triggered by DC creation)
  - `Dispatched` → `Invoiced` (triggered by Invoice creation)
  - `Invoiced` → `Closed` (manual by Admin/Sales)
- Some transitions are **automatic** (triggered by downstream entity creation); others are manual

---

## UI Pages

### WorkOrdersPage (`/sales/work-orders`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Work Orders"            [+ New Work Order]  │
├──────────────────────────────────────────────────────────┤
│ Status Cards (colorful, horizontal scroll):              │
│ [Created:2] [MatAwait:5] [MatRecvd:3] [InProd:8] ...   │
├──────────────────────────────────────────────────────────┤
│ Filters: [Search___] [Status ▼] [Customer ▼] [Process▼] │
├──────────────────────────────────────────────────────────┤
│ DataTable:                                               │
│ WO Number │ Date  │ Customer  │ Process │ Color │ Status │
│ WO-25-008 │ 15Jan │ Ajit Coat │ PwdCoat │ RAL9007│ [InProd]│
│ ...                                                      │
└──────────────────────────────────────────────────────────┘
```

### WorkOrderFormPage (`/sales/work-orders/new` or `/sales/work-orders/:id/edit`)
```
┌──────────────────────────────────────────────────────────┐
│ Customer*:      [Autocomplete____________________▼]      │
│ Date*:          [DatePicker]  Dispatch Date: [DatePicker] │
│ PI Reference:   [Autocomplete — PI lookup________▼]      │
│ Process Type*:  [Select — Powder Coating, etc.___▼]      │
│ Powder Color:   [Autocomplete — PowderColor lookup▼]     │
│ Project Name:   [________________________]               │
│ Notes:          [__________________________________]     │
│            [Cancel]               [Create Work Order]    │
└──────────────────────────────────────────────────────────┘
```

### WorkOrderDetailPage (`/sales/work-orders/:id`) — **Key Page**
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "WO-2025-008"               Status: [InProd] │
│ Customer: Ajit Coatings │ Process: Powder Coating        │
│ Powder: RAL 9007 (YW17AN) │ Dispatch: 25 Jan 2025       │
├──────────────────────────────────────────────────────────┤
│ ORDER LIFECYCLE TIMELINE                                 │
│ ● Created (15 Jan) → ● Mat Received (17 Jan, INW-001)   │
│ → ● In Production (18 Jan, PWO-001) → ○ QA Complete     │
│ → ○ Dispatched → ○ Invoiced → ○ Closed                  │
├──────────────────────────────────────────────────────────┤
│ LINKED DOCUMENTS                                         │
│ PI: PI-2025-003  │ Material Inward: INW-2025-001        │
│ PWO: PWO-2025-002 │ Delivery Challan: —                  │
│ Invoice: —                                               │
├──────────────────────────────────────────────────────────┤
│ [Edit] [Advance Status ▼]                                │
└──────────────────────────────────────────────────────────┘
```

The **timeline** is a visual stepper (MUI `Stepper` or custom) showing order progress. The **linked documents** section has clickable links to related entities.

---

## Business Rules
1. WO number auto-generated: WO-YYYY-NNN
2. Creating a WO updates linked Inquiry to `Confirmed` (if linked via PI → Quotation → Inquiry)
3. WO is the **hub entity** — all downstream modules reference it
4. Status transitions are partially automatic (see Validation section)
5. The detail page is the **central order tracker** — most-visited page for Sales and Leaders
6. Work orders cannot be deleted once they have downstream documents
7. Admin and Leader can view all WOs; Sales sees own + team's WOs

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/WorkOrdersController.cs` | WO CRUD + timeline + stats |
| `Services/Sales/IWorkOrderService.cs` + `WorkOrderService.cs` | Business logic, timeline build |
| `DTOs/Sales/CreateWorkOrderDto.cs` | Create |
| `DTOs/Sales/WorkOrderDto.cs` | List response |
| `DTOs/Sales/WorkOrderDetailDto.cs` | Detail response |
| `DTOs/Sales/WorkOrderTimelineDto.cs` | Timeline |
| `DTOs/Sales/WorkOrderStatsDto.cs` | Status counts |
| `Validators/Sales/CreateWorkOrderValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/sales/WorkOrdersPage.jsx` | WO list + status cards |
| `src/pages/sales/WorkOrderFormPage.jsx` | Create/edit form |
| `src/pages/sales/WorkOrderDetailPage.jsx` | Detail + timeline + linked docs |
| `src/hooks/useWorkOrders.js` | React Query hooks |
| `src/services/workOrderService.js` | API calls |

## Acceptance Criteria
1. WO number auto-generated: WO-YYYY-NNN
2. Status cards show correct counts per status
3. Detail page shows visual timeline stepper
4. Linked documents section shows clickable references to related entities
5. Creating a WO optionally links to PI and auto-fills customer
6. Status transitions enforce valid paths
7. Downstream documents block deletion
8. Timeline endpoint aggregates events from Material Inward, PWO, Final Inspection, DC, Invoice
9. Mobile: detail page stacks sections vertically, timeline scrollable
10. "Advance Status" dropdown shows only valid next statuses

## Reference
- **WORKFLOWS.md:** Workflow 1, Steps 7-9
- **01-database-schema.md:** WorkOrder entity
- **order-cycle-images:** Work order format slides
