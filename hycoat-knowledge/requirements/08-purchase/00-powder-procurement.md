# 08-purchase/00-powder-procurement

## Feature ID
`08-purchase/00-powder-procurement`

## Feature Name
Powder Procurement — Indent → PO → GRN (API + UI)

## Dependencies
- `01-masters/00-master-data-api` — Vendor, PowderColor entities
- `04-ppc/00-production-work-order` — PWO entity (indent linked to PWO)
- `00-foundation/01-database-schema` — PowderIndent, PowderIndentLine, PurchaseOrder, POLineItem, GoodsReceivedNote, GRNLineItem, PowderStock entities

## Business Context
When a Work Order is confirmed and a PWO is created, PPC checks if the required powder is in stock. If not, they raise a **Powder Indent** to the Purchase department. Purchase then creates a **Purchase Order (PO)** to the powder vendor. When the vendor delivers, a **Goods Received Note (GRN)** is created and stock is updated. The **Powder Stock Ledger** tracks running balance per powder/color with reorder-level alerts.

This is a simple 3-document procurement cycle (Indent → PO → GRN) specific to powder coating consumables — not a general-purpose purchasing module.

**Workflow Reference:** WORKFLOWS.md → Workflow 4 — Powder Procurement.

---

## Entities (from 01-database-schema)
- **PowderIndent** — IndentNumber (IND-YYYY-NNN), Date, ProductionWorkOrderId, RequestedByUserId, Status, Notes
- **PowderIndentLine** — PowderIndentId, PowderColorId, RequiredQtyKg
- **PurchaseOrder** — PONumber (PO-YYYY-NNN), Date, VendorId, PowderIndentId, Status, Notes
- **POLineItem** — PurchaseOrderId, PowderColorId, QtyKg, RatePerKg, Amount, RequiredByDate
- **GoodsReceivedNote** — GRNNumber (GRN-YYYY-NNN), Date, PurchaseOrderId, ReceivedByUserId, Notes
- **GRNLineItem** — GoodsReceivedNoteId, PowderColorId, QtyReceivedKg, BatchCode, MfgDate, ExpiryDate
- **PowderStock** — PowderColorId (unique), CurrentStockKg, ReorderLevelKg, LastUpdated

---

## API Endpoints

### Powder Indents
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/powder-indents` | PPC, Purchase, Admin, Leader | List |
| GET | `/api/powder-indents/{id}` | All auth'd | Detail with lines |
| POST | `/api/powder-indents` | PPC, Admin, Leader | Create with lines |
| PUT | `/api/powder-indents/{id}` | PPC, Admin, Leader | Update (Requested only) |
| DELETE | `/api/powder-indents/{id}` | Admin | Soft delete |
| PATCH | `/api/powder-indents/{id}/status` | Purchase, Admin, Leader | Approve/Order/Receive |

### Purchase Orders
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/purchase-orders` | Purchase, Finance, Admin, Leader | List |
| GET | `/api/purchase-orders/{id}` | All auth'd | Detail with lines |
| POST | `/api/purchase-orders` | Purchase, Admin, Leader | Create with lines |
| PUT | `/api/purchase-orders/{id}` | Purchase, Admin, Leader | Update (Draft only) |
| DELETE | `/api/purchase-orders/{id}` | Admin | Soft delete |
| PATCH | `/api/purchase-orders/{id}/status` | Purchase, Admin, Leader | Update status |
| POST | `/api/purchase-orders/{id}/generate-pdf` | Purchase, Admin, Leader | Generate PO PDF |
| GET | `/api/purchase-orders/{id}/pdf` | All auth'd | Download PDF |

### Goods Received Notes
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/grns` | Purchase, SCM, Admin, Leader | List |
| GET | `/api/grns/{id}` | All auth'd | Detail with lines |
| POST | `/api/grns` | Purchase, SCM, Admin, Leader | Create with lines → auto-update stock |
| PUT | `/api/grns/{id}` | Purchase, Admin, Leader | Update |
| DELETE | `/api/grns/{id}` | Admin | Soft delete (reverse stock) |

### Powder Stock
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/powder-stock` | All auth'd | List all stock levels |
| GET | `/api/powder-stock/{powderColorId}` | All auth'd | Stock for specific powder |
| PUT | `/api/powder-stock/{powderColorId}/reorder-level` | Purchase, Admin | Set reorder level |
| GET | `/api/powder-stock/low-stock` | Purchase, PPC, Admin, Leader | Items below reorder level |

**Query Parameters:**
- Indents: `status`, `dateFrom`, `dateTo`, `search` (indent number, PWO number)
- POs: `status`, `vendorId`, `dateFrom`, `dateTo`, `search` (PO number, vendor name)
- GRNs: `purchaseOrderId`, `dateFrom`, `dateTo`, `search` (GRN number)
- All: `page`, `pageSize`

---

## DTOs

### CreatePowderIndentDto
```csharp
public class CreatePowderIndentDto
{
    public DateTime Date { get; set; }
    public int? ProductionWorkOrderId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePowderIndentLineDto> Lines { get; set; } = [];
}

public class CreatePowderIndentLineDto
{
    public int PowderColorId { get; set; }
    public decimal RequiredQtyKg { get; set; }
}
```

### CreatePurchaseOrderDto
```csharp
public class CreatePurchaseOrderDto
{
    public DateTime Date { get; set; }
    public int VendorId { get; set; }
    public int? PowderIndentId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePOLineItemDto> Lines { get; set; } = [];
}

public class CreatePOLineItemDto
{
    public int PowderColorId { get; set; }
    public decimal QtyKg { get; set; }
    public decimal RatePerKg { get; set; }
    public DateTime? RequiredByDate { get; set; }
}
```

### CreateGRNDto
```csharp
public class CreateGRNDto
{
    public DateTime Date { get; set; }
    public int PurchaseOrderId { get; set; }
    public string? Notes { get; set; }
    public List<CreateGRNLineItemDto> Lines { get; set; } = [];
}

public class CreateGRNLineItemDto
{
    public int PowderColorId { get; set; }
    public decimal QtyReceivedKg { get; set; }
    public string? BatchCode { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
```

### PowderStockDto
```csharp
public class PowderStockDto
{
    public int PowderColorId { get; set; }
    public string PowderCode { get; set; }
    public string ColorName { get; set; }
    public string? RALCode { get; set; }
    public decimal CurrentStockKg { get; set; }
    public decimal? ReorderLevelKg { get; set; }
    public bool IsBelowReorderLevel { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

---

## Stock Update Logic

```csharp
// On GRN creation:
foreach (var line in grn.Lines)
{
    var stock = await GetOrCreateStock(line.PowderColorId);
    stock.CurrentStockKg += line.QtyReceivedKg;
    stock.LastUpdated = DateTime.UtcNow;
}

// Update PO status:
var totalOrdered = po.Lines.Sum(l => l.QtyKg);
var totalReceived = grns.SelectMany(g => g.Lines).Sum(l => l.QtyReceivedKg);
po.Status = totalReceived >= totalOrdered ? "Received" : "PartiallyReceived";

// Update Indent status to "Received" when PO is fully received
```

---

## Validation
- Indent: `Date` required, at least 1 line, `RequiredQtyKg` > 0
- PO: `Date`, `VendorId` required, at least 1 line, `QtyKg` > 0, `RatePerKg` > 0
- PO Amount auto-computed: QtyKg × RatePerKg
- GRN: `Date`, `PurchaseOrderId` required, at least 1 line, `QtyReceivedKg` > 0
- GRN powder colors must match PO line items
- GRN total received cannot exceed PO ordered qty (warn, don't block)
- PO status must be "Draft", "Sent", "PartiallyReceived", "Received", "Cancelled"
- Indent status must be "Requested", "Approved", "Ordered", "Received"
- ReorderLevelKg ≥ 0

---

## UI Pages

### PowderIndentsPage (`/purchase/indents`)
Columns: Indent #, Date, PWO Ref, Powders (count), Total Qty (kg), Status.
Status chips: yellow=Requested, blue=Approved, orange=Ordered, green=Received.
Action: "Create PO" button for Approved indents.

### PowderIndentFormPage (`/purchase/indents/new` or `:id`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Powder Indent"                              │
├──────────────────────────────────────────────────────────┤
│ PWO Reference: [Autocomplete (optional)_________▼]       │
│ Date*: [Today]                                           │
├──────────────────────────────────────────────────────────┤
│ POWDER REQUIREMENTS                    [+ Add Line]      │
│ ┌────────────────────┬──────────────────┐                │
│ │ Powder / Color     │ Required Qty (kg)│                │
│ ├────────────────────┼──────────────────┤                │
│ │ [RAL 9016 White ▼] │ [50____________] │                │
│ │ [RAL 7035 Grey  ▼] │ [25____________] │                │
│ └────────────────────┴──────────────────┘                │
│ Notes: [________________________]                        │
│     [Cancel]                 [Submit Indent]             │
└──────────────────────────────────────────────────────────┘
```

### PurchaseOrdersPage (`/purchase/orders`)
Columns: PO #, Date, Vendor, Powders, Total Qty (kg), Total Amount, Status.

### PurchaseOrderFormPage (`/purchase/orders/new` or `:id`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Purchase Order"                             │
├──────────────────────────────────────────────────────────┤
│ Vendor*: [Autocomplete (vendors)_____________▼]          │
│ Indent Ref: [IND-2025-008 ▼] (optional, auto-fills)     │
│ Date*: [Today]                                           │
├──────────────────────────────────────────────────────────┤
│ ORDER LINES                            [+ Add Line]      │
│ ┌────────────────┬────────┬──────────┬──────────┬───────┐│
│ │ Powder/Color   │ Qty(kg)│ Rate/kg  │ Amount   │ By    ││
│ ├────────────────┼────────┼──────────┼──────────┼───────┤│
│ │ RAL9016 White  │ 50     │ 350.00   │ 17,500   │ [date]││
│ │ RAL7035 Grey   │ 25     │ 380.00   │  9,500   │ [date]││
│ └────────────────┴────────┴──────────┴──────────┴───────┘│
│ Total Amount: ₹ 27,000                                   │
│ Notes: [________________________]                        │
│ [Cancel]    [Save Draft]    [Save & Generate PDF]        │
└──────────────────────────────────────────────────────────┘
```

### GRNsPage (`/purchase/grns`)
Columns: GRN #, Date, PO Ref, Powders, Total Received (kg).

### GRNFormPage (`/purchase/grns/new` or `:id`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Goods Received Note"                        │
├──────────────────────────────────────────────────────────┤
│ Purchase Order*: [PO-2025-012 ▼] (auto-fills lines)     │
│ Date*: [Today]                                           │
├──────────────────────────────────────────────────────────┤
│ RECEIVED ITEMS                                           │
│ ┌────────────────┬──────────┬──────────┬──────┬────────┐ │
│ │ Powder/Color   │ Ord.(kg) │ Rcvd(kg) │Batch │ Expiry │ │
│ ├────────────────┼──────────┼──────────┼──────┼────────┤ │
│ │ RAL9016 White  │ 50       │ [50____] │[B042]│[date__]│ │
│ │ RAL7035 Grey   │ 25       │ [25____] │[B043]│[date__]│ │
│ └────────────────┴──────────┴──────────┴──────┴────────┘ │
│ Notes: [________________________]                        │
│     [Cancel]                 [Save GRN]                  │
└──────────────────────────────────────────────────────────┘
```

### PowderStockPage (`/purchase/stock`)

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Powder Stock Ledger"                        │
│ [Show Low Stock Only] toggle                             │
├──────────────────────────────────────────────────────────┤
│ ┌──────────┬────────────┬───────────┬──────────┬───────┐ │
│ │ Code     │ Color      │ Stock(kg) │ Reorder  │ Alert │ │
│ ├──────────┼────────────┼───────────┼──────────┼───────┤ │
│ │ RAL 9016 │ White      │ 45.0      │ 30.0     │       │ │
│ │ RAL 7035 │ Grey       │ 8.5       │ 20.0     │  ⚠️  │ │
│ │ RAL 3020 │ Red        │ 0.0       │ 15.0     │  🔴  │ │
│ └──────────┴────────────┴───────────┴──────────┴───────┘ │
│                                                          │
│ Click row → Edit reorder level                           │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- "Create PO" from indent auto-fills PO lines with indent quantities
- Selecting PO in GRN form auto-fills expected lines with ordered quantities
- GRN submission auto-updates powder stock
- PO status auto-updated based on received quantities
- Low stock alerts shown on stock page and dashboard
- Auto-numbering: IND-YYYY-NNN, PO-YYYY-NNN, GRN-YYYY-NNN
- PO → PDF for sending to vendor

---

## Business Rules
1. Indent → Approve → Create PO → Send to Vendor → Receive GRN → Stock Updated
2. GRN creation automatically increases PowderStock.CurrentStockKg
3. PO status auto-transitions: Draft → Sent → PartiallyReceived → Received
4. Indent status auto-transitions: Requested → Approved → Ordered → Received
5. Low stock alert when CurrentStockKg < ReorderLevelKg
6. GRN lines must reference powder colors from the linked PO
7. Batch code and expiry tracked for powder traceability
8. One PowderStock record per PowderColor (upsert on first GRN)
9. PO PDF generated for vendor communication
10. Auto-numbering: IND-YYYY-NNN, PO-YYYY-NNN, GRN-YYYY-NNN

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/PowderIndentsController.cs` | CRUD + status |
| `Controllers/PurchaseOrdersController.cs` | CRUD + PDF + status |
| `Controllers/GRNsController.cs` | CRUD with stock update |
| `Controllers/PowderStockController.cs` | Read + reorder level |
| `Services/Purchase/IPowderIndentService.cs` + impl | Logic |
| `Services/Purchase/IPurchaseOrderService.cs` + impl | Logic |
| `Services/Purchase/IGRNService.cs` + impl | Logic + stock update |
| `Services/Purchase/IPowderStockService.cs` + impl | Stock queries, low-stock |
| `Services/Purchase/PurchaseOrderPdfService.cs` | QuestPDF |
| `DTOs/Purchase/CreatePowderIndentDto.cs` | |
| `DTOs/Purchase/PowderIndentDto.cs` | |
| `DTOs/Purchase/PowderIndentDetailDto.cs` | |
| `DTOs/Purchase/CreatePurchaseOrderDto.cs` | |
| `DTOs/Purchase/PurchaseOrderDto.cs` | |
| `DTOs/Purchase/PurchaseOrderDetailDto.cs` | |
| `DTOs/Purchase/CreateGRNDto.cs` | |
| `DTOs/Purchase/GRNDto.cs` | |
| `DTOs/Purchase/GRNDetailDto.cs` | |
| `DTOs/Purchase/PowderStockDto.cs` | |
| `Validators/Purchase/CreatePowderIndentValidator.cs` | |
| `Validators/Purchase/CreatePurchaseOrderValidator.cs` | |
| `Validators/Purchase/CreateGRNValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/purchase/PowderIndentsPage.jsx` | List |
| `src/pages/purchase/PowderIndentFormPage.jsx` | Form |
| `src/pages/purchase/PurchaseOrdersPage.jsx` | List |
| `src/pages/purchase/PurchaseOrderFormPage.jsx` | Form |
| `src/pages/purchase/GRNsPage.jsx` | List |
| `src/pages/purchase/GRNFormPage.jsx` | Form |
| `src/pages/purchase/PowderStockPage.jsx` | Stock ledger |
| `src/hooks/usePowderIndents.js` | React Query hooks |
| `src/hooks/usePurchaseOrders.js` | React Query hooks |
| `src/hooks/useGRNs.js` | React Query hooks |
| `src/hooks/usePowderStock.js` | React Query hooks |
| `src/services/powderIndentService.js` | API calls |
| `src/services/purchaseOrderService.js` | API calls |
| `src/services/grnService.js` | API calls |
| `src/services/powderStockService.js` | API calls |

## Acceptance Criteria
1. Full Indent → PO → GRN flow with status transitions
2. "Create PO" from indent auto-fills PO lines
3. GRN auto-fills from PO lines with ordered quantities
4. GRN submission auto-updates powder stock ledger
5. PO status auto-transitions based on received quantities
6. Low stock alert on stock page (items below reorder level)
7. Reorder level editable per powder
8. PO PDF generated for vendor
9. Batch code and expiry tracking on GRN
10. Auto-numbering: IND, PO, GRN with YYYY-NNN format
11. Each document links to the prior one (GRN → PO → Indent)

## Reference
- **WORKFLOWS.md:** Workflow 4 — Powder Procurement
- **01-database-schema.md:** PowderIndent, PurchaseOrder, GoodsReceivedNote, PowderStock + line entities
