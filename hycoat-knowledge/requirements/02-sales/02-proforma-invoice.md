# 02-sales/02-proforma-invoice

## Feature ID
`02-sales/02-proforma-invoice`

## Feature Name
Proforma Invoice (PI) Management (API + UI + PDF)

## Dependencies
- `02-sales/01-quotation` — Quotation link (optional), PDF service pattern
- `01-masters/00-master-data-api` — Customer, SectionProfile, ProcessType lookups
- `00-foundation/01-database-schema` — ProformaInvoice, PILineItem entities

## Business Context
After the customer accepts a quotation and sends their BOM (Bill of Material) with section details, Sales prepares a Proforma Invoice (PI). The PI is the detailed commercial document with **itemized area calculations (SFT)** per section, GST breakup, packing/transport charges. This is the critical calculation document — the area formula `(PerimeterMM × LengthMM × Qty) / 92903.04` drives all downstream pricing.

**Workflow Reference:** WORKFLOWS.md → Workflow 1, Steps 4-6. Key Data Fields — PI.

---

## Entities (from 01-database-schema)
- **ProformaInvoice** — PINumber (PI-YYYY-NNN), Date, CustomerId, QuotationId?, SubTotal, PackingCharges, TransportCharges, TaxableAmount, CGST/SGST/IGST rates and amounts, GrandTotal, IsInterState, Status, FileUrl
- **PILineItem** — ProformaInvoiceId, SectionProfileId, SectionNumber, LengthMM, Quantity, PerimeterMM, AreaSqMtr, AreaSFT, RatePerSFT, Amount

**Status Flow:** `Draft` → `Sent` → `Approved` | `Rejected`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/proforma-invoices` | Sales, Admin, Leader | List |
| GET | `/api/proforma-invoices/{id}` | Sales, Admin, Leader | Detail with line items |
| POST | `/api/proforma-invoices` | Sales, Admin, Leader | Create with line items |
| PUT | `/api/proforma-invoices/{id}` | Sales, Admin, Leader | Update |
| PATCH | `/api/proforma-invoices/{id}/status` | Sales, Admin, Leader | Status change |
| DELETE | `/api/proforma-invoices/{id}` | Admin | Soft delete |
| GET | `/api/proforma-invoices/{id}/pdf` | Sales, Admin, Leader | Generate PDF |
| POST | `/api/proforma-invoices/calculate-area` | Sales, Admin, Leader | Calculate SFT for given sections (helper endpoint) |

**Query Parameters:** `search`, `status`, `customerId`, `fromDate`, `toDate`, `page`, `pageSize`, `sortBy`, `sortDesc`

---

## DTOs

### CreateProformaInvoiceDto
```csharp
public class CreateProformaInvoiceDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? QuotationId { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public bool IsInterState { get; set; }
    public string? Notes { get; set; }
    public List<CreatePILineItemDto> LineItems { get; set; }
}

public class CreatePILineItemDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal RatePerSFT { get; set; }
}
```

### PIDetailDto (response)
```csharp
public class PIDetailDto
{
    public int Id { get; set; }
    public string PINumber { get; set; }
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public int? QuotationId { get; set; }
    public string? QuotationNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTRate { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public bool IsInterState { get; set; }
    public string Status { get; set; }
    public string? FileUrl { get; set; }
    public List<PILineItemDetailDto> LineItems { get; set; }
}

public class PILineItemDetailDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal AreaSFT { get; set; }
    public decimal RatePerSFT { get; set; }
    public decimal Amount { get; set; }
}
```

### CalculateAreaRequestDto (helper)
```csharp
public class CalculateAreaRequestDto
{
    public List<AreaCalcLineDto> Lines { get; set; }
}
public class AreaCalcLineDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
}
```

---

## Area Calculation Logic (Critical Business Rule)

```
For each line item:
  1. Fetch SectionProfile.PerimeterMM from master
  2. AreaSFT = (PerimeterMM × LengthMM × Quantity) / 92903.04
  3. Amount = AreaSFT × RatePerSFT

SubTotal = Sum of all line item Amounts
TaxableAmount = SubTotal + PackingCharges + TransportCharges

If same state (IsInterState = false):
  CGST = TaxableAmount × 9%
  SGST = TaxableAmount × 9%
  IGST = 0

If inter-state (IsInterState = true):
  CGST = 0
  SGST = 0
  IGST = TaxableAmount × 18%

GrandTotal = TaxableAmount + CGST + SGST + IGST
```

**Server-side calculation only.** The UI sends raw inputs (SectionProfileId, LengthMM, Quantity, RatePerSFT). The API calculates AreaSFT, SubTotal, taxes, GrandTotal.

---

## PDF Generation (QuestPDF)

**PI PDF Layout:**
```
┌─────────────────────────────────────────────────┐
│ HYCOAT SYSTEMS                                  │
│ Address, GSTIN, Phone, Email                    │
├─────────────────────────────────────────────────┤
│ PROFORMA INVOICE                                │
│ PI No: PI-2025-001     Date: 15/01/2025        │
│ Ref: QTN-2025-001                               │
├─────────────────────────────────────────────────┤
│ To:                                             │
│ [Customer Name]      Our GSTIN: [GSTIN]        │
│ [Customer Address]   Customer GSTIN: [GSTIN]   │
├─────────────────────────────────────────────────┤
│ Sr│Section│Len(mm)│Qty│Peri(mm)│Area(SFT)│Rate│Amt│
│  1│M-1815 │ 7200  │100│ 320    │ 247.89  │₹25 │₹6k│
│  2│15940  │ 7400  │ 50│ 180    │  69.56  │₹25 │₹1k│
├─────────────────────────────────────────────────┤
│                           Subtotal: ₹XXX.XX     │
│                           Packing:  ₹XXX.XX     │
│                           Transport:₹XXX.XX     │
│                           Taxable:  ₹XXX.XX     │
│                           CGST 9%:  ₹XXX.XX     │
│                           SGST 9%:  ₹XXX.XX     │
│                           GRAND TOTAL: ₹XXX.XX  │
├─────────────────────────────────────────────────┤
│ ANNEXURE — Area Calculation                     │
│ Section M-1815: Perimeter=320mm × Length=7200mm │
│   × Qty=100 / 92903.04 = 247.89 SFT           │
│ Section 15940: ...                              │
├─────────────────────────────────────────────────┤
│ Bank Details:                                   │
│ [Bank Name, A/C No, IFSC]                      │
│ Terms & Conditions:                             │
│ - Validity: 15 days                             │
│ - Payment: 30 days from invoice                 │
│ [Authorized Signature]                          │
└─────────────────────────────────────────────────┘
```

The **annexure** (area calculation breakdown) is critical — it shows the formula per line for customer transparency.

---

## Validation
- `CustomerId` — required, must exist
- `Date` — required
- `LineItems` — at least 1 item
- Each line item: `SectionProfileId` must exist, `LengthMM` > 0, `Quantity` > 0, `RatePerSFT` > 0
- `PackingCharges`, `TransportCharges` ≥ 0
- Status transitions: Draft → Sent → Approved | Rejected

---

## UI Pages

### PIListPage (`/sales/proforma-invoices`)
Columns: PI Number, Date, Customer, Grand Total (₹), Status, PDF icon.
Filters: status, customer, date range.

### PIFormPage (`/sales/proforma-invoices/new` or `/sales/proforma-invoices/:id`)
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "New Proforma Invoice"                       │
├──────────────────────────────────────────────────────────┤
│ Quotation:  [Autocomplete — optional________________▼]   │
│ Customer*:  [Autocomplete — Customer lookup_________▼]   │
│ Date*:      [DatePicker] Inter-State: [Toggle/Checkbox]  │
├──────────────────────────────────────────────────────────┤
│ LINE ITEMS                                               │
│ ┌──┬──────────┬────────┬─────┬─────────┬───────┬──────┐ │
│ │# │Section*  │Len(mm)*│Qty* │Area(SFT)│Rate*  │Amount│ │
│ ├──┼──────────┼────────┼─────┼─────────┼───────┼──────┤ │
│ │1 │[M-1815▼] │[7200__]│[100]│ 247.89  │[25.00]│₹6,19│ │
│ │2 │[15940_▼] │[7400__]│[50_]│  69.56  │[25.00]│₹1,73│ │
│ └──┴──────────┴────────┴─────┴─────────┴───────┴──────┘ │
│ [+ Add Line Item]                                        │
├──────────────────────────────────────────────────────────┤
│ Packing Charges:    [______]                             │
│ Transport Charges:  [______]                             │
│ ────────────────────────────                             │
│ Subtotal:           ₹7,927.50                            │
│ Taxable Amount:     ₹8,127.50                            │
│ CGST (9%):          ₹  731.48                            │
│ SGST (9%):          ₹  731.48                            │
│ Grand Total:        ₹9,590.46                            │
├──────────────────────────────────────────────────────────┤
│ Notes:  [__________________________________]             │
├──────────────────────────────────────────────────────────┤
│ [Cancel] [Save Draft] [Save & Send] [Download PDF]       │
└──────────────────────────────────────────────────────────┘
```

**Key UI behaviors:**
- **Section Profile autocomplete**: Selecting a section auto-fills `PerimeterMM` (read-only)
- **Live area calculation**: As user types LengthMM and Quantity, AreaSFT recalculates in real-time (client-side preview). Final calculation done server-side on save.
- **Totals section**: Updates live as line items and charges change
- **Inter-state toggle**: Switches between CGST+SGST and IGST display
- "Save & Send" → status=Sent, updates linked Inquiry to `PISent`
- "Download PDF" → visible only in edit mode

---

## React Query Hooks (`src/hooks/useProformaInvoices.js`)
- `useProformaInvoices(params)` — paginated list
- `useProformaInvoice(id)` — detail
- `useCreateProformaInvoice()` — mutation
- `useUpdateProformaInvoice()` — mutation
- `useUpdatePIStatus()` — mutation
- `useDownloadPIPdf(id)` — returns blob

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/ProformaInvoicesController.cs` | PI CRUD + PDF endpoint |
| `Services/Sales/IProformaInvoiceService.cs` + `ProformaInvoiceService.cs` | Area calc, tax calc, CRUD |
| `Services/Pdf/IPIPdfService.cs` + `PIPdfService.cs` | QuestPDF with annexure |
| `DTOs/Sales/CreateProformaInvoiceDto.cs` | Create |
| `DTOs/Sales/PIDetailDto.cs` | Detail response |
| `DTOs/Sales/PIDto.cs` | List response |
| `DTOs/Sales/CalculateAreaRequestDto.cs` | Area helper |
| `Validators/Sales/CreatePIValidator.cs` | Validation |

### UI
| File | Purpose |
|---|---|
| `src/pages/sales/PIListPage.jsx` | PI list |
| `src/pages/sales/PIFormPage.jsx` | PI form with live calc |
| `src/hooks/useProformaInvoices.js` | React Query hooks |
| `src/services/piService.js` | API calls |

## Acceptance Criteria
1. PI number auto-generated: PI-YYYY-NNN
2. Area calculated as `(PerimeterMM × LengthMM × Qty) / 92903.04`
3. GST correctly calculated: CGST+SGST for same-state, IGST for inter-state
4. PDF includes annexure with area calculation breakdown per line
5. Section autocomplete fills PerimeterMM from master
6. Live total calculation on UI as user types
7. Customer address and GSTIN snapshotted into PI record
8. "Save & Send" updates linked inquiry status
9. Inter-state toggle switches tax display correctly
10. Grand total rounds to 2 decimal places

## Reference
- **WORKFLOWS.md:** Workflow 1, Steps 4-6, Key Data Fields — PI
- **01-database-schema.md:** ProformaInvoice, PILineItem entities
- **order-cycle-images:** Slides showing PI document format with annexure
