# 07-dispatch/00-packing-delivery-challan

## Feature ID
`07-dispatch/00-packing-delivery-challan`

## Feature Name
Packing List & Delivery Challan (API + UI)

## Dependencies
- `06-quality/01-final-inspection-test-cert` — Final inspection must be Approved before dispatch
- `02-sales/03-work-order` — Work Order entity
- `00-foundation/01-database-schema` — PackingList, PackingListLine, DeliveryChallan, DCLineItem entities

## Business Context
Once QA approves the final inspection, the SCM / Dispatch team prepares the shipment. The process is:
1. **Packing**: Tape each section (taping machine), bundle and tie, label each bundle
2. **Packing List**: Document section-wise quantities, bundle numbers
3. **Delivery Challan (DC)**: Legal transport document with customer details, vehicle info, material description
4. **Loading Photos**: Photograph loaded vehicle for proof

The DC is printed in triplicate: one with driver, one for customer, one retained by HyCoat. The DC number is a key reference used by the customer to acknowledge receipt.

**Workflow Reference:** WORKFLOWS.md → Workflow 8 — Dispatch.

---

## Entities (from 01-database-schema)
- **PackingList** — Date, ProductionWorkOrderId, WorkOrderId, PackingType, BundleCount, PreparedByUserId, Notes
- **PackingListLine** — PackingListId, SectionProfileId, Quantity, LengthMM, BundleNumber, Remarks
- **DeliveryChallan** — DCNumber (DC-YYYY-NNN), Date, WorkOrderId, CustomerId, CustomerAddress/GSTIN (snapshot), VehicleNumber, DriverName, LRNumber, MaterialValueApprox, Status, Notes
- **DCLineItem** — DeliveryChallanId, SectionProfileId, LengthMM, Quantity, CustomerDCRef, Remarks

---

## API Endpoints

### Packing Lists
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/packing-lists` | SCM, Admin, Leader | List |
| GET | `/api/packing-lists/{id}` | All auth'd | Detail with lines |
| POST | `/api/packing-lists` | SCM, Admin, Leader | Create with lines |
| PUT | `/api/packing-lists/{id}` | SCM, Admin, Leader | Update |
| DELETE | `/api/packing-lists/{id}` | Admin | Soft delete |
| GET | `/api/packing-lists/by-work-order/{woId}` | All auth'd | Packing list for a WO |

### Delivery Challans
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/delivery-challans` | SCM, Sales, Finance, Admin, Leader | List |
| GET | `/api/delivery-challans/{id}` | All auth'd | Detail with lines |
| POST | `/api/delivery-challans` | SCM, Admin, Leader | Create with lines |
| PUT | `/api/delivery-challans/{id}` | SCM, Admin, Leader | Update |
| DELETE | `/api/delivery-challans/{id}` | Admin | Soft delete |
| PATCH | `/api/delivery-challans/{id}/status` | SCM, Admin, Leader | Update status |
| POST | `/api/delivery-challans/{id}/generate-pdf` | SCM, Admin, Leader | Generate PDF |
| GET | `/api/delivery-challans/{id}/pdf` | All auth'd | Download PDF |
| POST | `/api/delivery-challans/{id}/loading-photos` | SCM, Admin | Upload loading photos |

**Query Parameters:**
- `date`, `customerId`, `workOrderId`, `status`
- `search` — DC number, customer name, vehicle number
- `page`, `pageSize`

---

## DTOs

### CreatePackingListDto
```csharp
public class CreatePackingListDto
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int WorkOrderId { get; set; }
    public string? PackingType { get; set; }
    public int? BundleCount { get; set; }
    public string? Notes { get; set; }
    public List<CreatePackingListLineDto> Lines { get; set; } = [];
}

public class CreatePackingListLineDto
{
    public int SectionProfileId { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public int? BundleNumber { get; set; }
    public string? Remarks { get; set; }
}
```

### CreateDeliveryChallanDto
```csharp
public class CreateDeliveryChallanDto
{
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? LRNumber { get; set; }
    public decimal? MaterialValueApprox { get; set; }
    public string? Notes { get; set; }
    public List<CreateDCLineItemDto> Lines { get; set; } = [];
}

public class CreateDCLineItemDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public string? CustomerDCRef { get; set; }
    public string? Remarks { get; set; }
}
```

### DeliveryChallanDetailDto
```csharp
public class DeliveryChallanDetailDto
{
    public int Id { get; set; }
    public string DCNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public string WorkOrderNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? LRNumber { get; set; }
    public decimal? MaterialValueApprox { get; set; }
    public string Status { get; set; }
    public List<DCLineItemDto> Lines { get; set; }
    public List<string> LoadingPhotoUrls { get; set; }
}
```

---

## PDF Generation (QuestPDF)

Delivery Challan PDF:

```
┌──────────────────────────────────────────────────────────┐
│                HYCOAT SYSTEMS PVT. LTD.                  │
│                DELIVERY CHALLAN                          │
│                                                          │
│ DC No: DC-2025-035          Date: 16-Jun-2025            │
│ Customer: ABC Extrusions    GSTIN: 24ABCDE1234F1Z5      │
│ Address: Plot 42, GIDC, Ahmedabad                        │
│ Vehicle: GJ-01-AB-1234      Driver: Ramesh Kumar         │
│ LR No: LR-98765            Inward DC Ref: CUST-DC-042   │
├──────────────────────────────────────────────────────────┤
│ Sr │ Section │ Length(mm) │ Qty │ Remarks                │
│  1 │ 6063T5  │ 4000       │ 200 │                        │
│  2 │ 6061T6  │ 3500       │ 150 │                        │
│  3 │ 6005A   │ 5000       │ 100 │                        │
│    │         │            │     │                        │
│    │ Total Quantity:      │ 450 │                        │
├──────────────────────────────────────────────────────────┤
│ Material Value (Approx): ₹ 2,50,000                     │
│ Bundles: 12                                              │
├──────────────────────────────────────────────────────────┤
│ Prepared By: ________     Checked By: ________           │
│ Received By: ________     Date: ________                 │
└──────────────────────────────────────────────────────────┘
```

---

## Validation
- PackingList: `Date`, `ProductionWorkOrderId`, `WorkOrderId` required; at least 1 line
- DC: `Date`, `WorkOrderId`, `CustomerId` required; at least 1 line
- DC can only be created if final inspection is Approved for the WO
- Status must be "Created", "Dispatched", or "Delivered"
- VehicleNumber format: basic pattern (alphanumeric with hyphens)
- Loading photos: max 10 per DC, each max 5MB, image types only

---

## UI Pages

### DeliveryChallansPage (`/dispatch/challans`)
Columns: DC #, Date, Customer, WO #, Vehicle, Bundles, Status.
Status chips: blue=Created, orange=Dispatched, green=Delivered.
Quick action: "Mark Dispatched" button for Created DCs.
Filters: date range, customer, status.

### PackingListFormPage (`/dispatch/packing/new` or `:id`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Packing List"                               │
├──────────────────────────────────────────────────────────┤
│ Work Order*: [Autocomplete___________▼]                  │
│ Date*: [Today]  Packing Type: [MERO TAPE___▼]            │
├──────────────────────────────────────────────────────────┤
│ PACKING LINES                          [+ Add Line]      │
│ ┌──────────┬───────────┬─────┬────────┬────────┐         │
│ │ Section  │ Length(mm) │ Qty │ Bundle#│ Remark │         │
│ ├──────────┼───────────┼─────┼────────┼────────┤         │
│ │ [______▼]│ [4000____]│[200]│ [1-4 ]│ [____] │         │
│ │ [______▼]│ [3500____]│[150]│ [5-8 ]│ [____] │         │
│ └──────────┴───────────┴─────┴────────┴────────┘         │
│ Total Bundles: [12]                                      │
├──────────────────────────────────────────────────────────┤
│ Notes: [________________________]                        │
│     [Cancel]                 [Save Packing List]         │
└──────────────────────────────────────────────────────────┘
```

### DeliveryChallanFormPage (`/dispatch/challans/new` or `:id`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Delivery Challan"                           │
├──────────────────────────────────────────────────────────┤
│ Work Order*: [Autocomplete___▼]  Customer auto-fills     │
│ Date*: [Today]                                           │
│ Customer: ABC Extrusions (auto)  GSTIN: 24ABCDE... (auto)│
├──────────────────────────────────────────────────────────┤
│ TRANSPORT DETAILS                                        │
│ Vehicle Number: [GJ-01-AB-1234]                          │
│ Driver Name:    [Ramesh Kumar__]                         │
│ LR Number:      [LR-98765______]                         │
│ Material Value (Approx): [₹ 2,50,000]                   │
├──────────────────────────────────────────────────────────┤
│ LINE ITEMS               (auto-filled from packing list) │
│ ┌──────────┬───────────┬─────┬─────────────┬────────┐    │
│ │ Section  │ Length(mm) │ Qty │ Cust DC Ref │ Remark │    │
│ ├──────────┼───────────┼─────┼─────────────┼────────┤    │
│ │ 6063T5   │ 4000      │ 200 │ [CUST-042_] │ [____] │    │
│ │ 6061T6   │ 3500      │ 150 │ [CUST-042_] │ [____] │    │
│ └──────────┴───────────┴─────┴─────────────┴────────┘    │
├──────────────────────────────────────────────────────────┤
│ LOADING PHOTOS                                           │
│ [📷 Upload Photo]                                        │
│ [photo1.jpg] [photo2.jpg]                                │
├──────────────────────────────────────────────────────────┤
│ [Cancel]    [Save Draft]    [Save & Generate PDF]        │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- Selecting WO auto-fills customer name, address, GSTIN
- DC lines can auto-populate from packing list
- CustomerDCRef = customer's original inward DC number
- "Save & Generate PDF" creates DC then generates PDF immediately
- Status progression: Created → Dispatched → Delivered
- Loading photos uploaded with mobile camera
- DC number auto-generated (DC-YYYY-NNN)

---

## Business Rules
1. DC can only be created for WOs with Approved final inspection
2. DC number auto-generated: DC-YYYY-NNN
3. Customer address/GSTIN snapshotted at DC creation (won't change if customer updates later)
4. DC lines can be auto-populated from packing list lines
5. Status flow: Created → Dispatched (when vehicle leaves) → Delivered (when customer confirms)
6. Loading photos serve as proof of dispatch
7. DC is part of the dispatch document bundle (DC + Invoice + TC + Annexure)
8. Packing list is internal; DC is the legal transport document

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/PackingListsController.cs` | CRUD |
| `Controllers/DeliveryChallansController.cs` | CRUD + PDF + photos + status |
| `Services/Dispatch/IPackingListService.cs` + impl | Logic |
| `Services/Dispatch/IDeliveryChallanService.cs` + impl | Logic |
| `Services/Dispatch/DeliveryChallanPdfService.cs` | QuestPDF |
| `DTOs/Dispatch/CreatePackingListDto.cs` | |
| `DTOs/Dispatch/PackingListDto.cs` | |
| `DTOs/Dispatch/PackingListDetailDto.cs` | |
| `DTOs/Dispatch/CreateDeliveryChallanDto.cs` | |
| `DTOs/Dispatch/DeliveryChallanDto.cs` | |
| `DTOs/Dispatch/DeliveryChallanDetailDto.cs` | |
| `Validators/Dispatch/CreatePackingListValidator.cs` | |
| `Validators/Dispatch/CreateDeliveryChallanValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/dispatch/DeliveryChallansPage.jsx` | List |
| `src/pages/dispatch/PackingListFormPage.jsx` | Packing form |
| `src/pages/dispatch/DeliveryChallanFormPage.jsx` | DC form |
| `src/hooks/usePackingLists.js` | React Query hooks |
| `src/hooks/useDeliveryChallans.js` | React Query hooks |
| `src/services/packingListService.js` | API calls |
| `src/services/deliveryChallanService.js` | API calls |

## Acceptance Criteria
1. Packing list form with dynamic line items
2. DC form with auto-filled customer data from WO
3. DC lines auto-populated from packing list
4. DC PDF generated with professional layout
5. Loading photo upload from mobile camera
6. Status progression: Created → Dispatched → Delivered
7. DC blocked if final inspection not Approved for the WO
8. DC number auto-generated (DC-YYYY-NNN)
9. Customer address/GSTIN snapshotted
10. Filters and search work on DC list

## Reference
- **WORKFLOWS.md:** Workflow 8 — Dispatch
- **01-database-schema.md:** PackingList, PackingListLine, DeliveryChallan, DCLineItem entities
