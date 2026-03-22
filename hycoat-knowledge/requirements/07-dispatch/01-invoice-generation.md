# 07-dispatch/01-invoice-generation

## Feature ID
`07-dispatch/01-invoice-generation`

## Feature Name
Invoice Generation with Annexure (API + UI)

## Dependencies
- `07-dispatch/00-packing-delivery-challan` — DC entity, dispatch flow
- `02-sales/03-work-order` — Work Order entity
- `01-masters/00-master-data-api` — SectionProfile (perimeter for area calc)
- `00-foundation/01-database-schema` — Invoice, InvoiceLineItem entities

## Business Context
The invoice is the billing document for the job work. It includes:
- Section-wise area calculation: **Perimeter (mm) × Length (mm) × Qty ÷ 92903.04 = SFT**
- Rate per SFT × Area = Line amount
- Packing charges, transport charges
- GST calculation (CGST+SGST for intra-state, IGST for inter-state)
- An **Annexure** showing the detailed perimeter-based area calculation breakdown per section

The invoice is generated as a PDF, printed in triplicate, and emailed to the customer along with the DC, Test Certificate, and Packing List as a single document bundle.

**Workflow Reference:** WORKFLOWS.md → Workflow 8 — Dispatch.

---

## Entities (from 01-database-schema)
- **Invoice** — InvoiceNumber (INV-YYYY-NNN), Date, CustomerId, WorkOrderId, DeliveryChallanId, customer snapshots, OurGSTIN, HSNSACCode, SubTotal, PackingCharges, TransportCharges, TaxableAmount, CGST/SGST/IGST rates and amounts, GrandTotal, RoundOff, AmountInWords, IsInterState, PaymentTerms, Bank details, Status, FileUrl
- **InvoiceLineItem** — InvoiceId, SectionProfileId, SectionNumber (snapshot), DCNumber, Color, MicronRange, PerimeterMM, LengthMM, Quantity, AreaSFT (calculated), RatePerSFT, Amount

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/invoices` | Finance, Sales, SCM, Admin, Leader | List |
| GET | `/api/invoices/{id}` | All auth'd | Detail with lines |
| POST | `/api/invoices` | Finance, Admin, Leader | Create with lines |
| PUT | `/api/invoices/{id}` | Finance, Admin, Leader | Update (Draft only) |
| DELETE | `/api/invoices/{id}` | Admin | Soft delete (Draft only) |
| PATCH | `/api/invoices/{id}/status` | Finance, Admin, Leader | Update status |
| POST | `/api/invoices/{id}/generate-pdf` | Finance, Admin, Leader | Generate invoice + annexure PDF |
| GET | `/api/invoices/{id}/pdf` | All auth'd | Download PDF |
| POST | `/api/invoices/{id}/send-email` | Finance, Sales, Admin, Leader | Email document bundle to customer |
| GET | `/api/invoices/by-work-order/{woId}` | All auth'd | Invoice for a WO |
| GET | `/api/invoices/auto-fill/{woId}` | Finance, Admin, Leader | Pre-fill invoice data from WO + DC |

**Query Parameters:**
- `dateFrom`, `dateTo`, `customerId`, `status`
- `search` — invoice number, customer name, WO number
- `page`, `pageSize`

---

## DTOs

### CreateInvoiceDto
```csharp
public class CreateInvoiceDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int WorkOrderId { get; set; }
    public int? DeliveryChallanId { get; set; }
    public string? HSNSACCode { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public bool IsInterState { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal IGSTRate { get; set; }
    public string? PaymentTerms { get; set; }
    public List<CreateInvoiceLineItemDto> Lines { get; set; } = [];
}

public class CreateInvoiceLineItemDto
{
    public int SectionProfileId { get; set; }
    public string? DCNumber { get; set; }
    public string? Color { get; set; }
    public string? MicronRange { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal RatePerSFT { get; set; }
}
```

### InvoiceDetailDto
```csharp
public class InvoiceDetailDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public string OurGSTIN { get; set; }
    public string WorkOrderNumber { get; set; }
    public string? HSNSACCode { get; set; }
    public decimal SubTotal { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public decimal TaxableAmount { get; set; }
    public bool IsInterState { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTRate { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal RoundOff { get; set; }
    public string? AmountInWords { get; set; }
    public string? PaymentTerms { get; set; }
    public string Status { get; set; }
    public string? FileUrl { get; set; }
    public List<InvoiceLineItemDto> Lines { get; set; }
}
```

### InvoiceAutoFillDto
```csharp
public class InvoiceAutoFillDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public int? DeliveryChallanId { get; set; }
    public string? DCNumber { get; set; }
    public List<InvoiceLineAutoFillDto> Lines { get; set; }
}

public class InvoiceLineAutoFillDto
{
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal AreaSFT { get; set; }
    public string? Color { get; set; }
}
```

---

## Area Calculation (Server-Side Verification)

```csharp
// Per line item:
line.AreaSFT = (line.PerimeterMM * line.LengthMM * line.Quantity) / 92903.04m;
line.Amount = Math.Round(line.AreaSFT * line.RatePerSFT, 2);

// Totals:
invoice.SubTotal = lines.Sum(l => l.Amount);
invoice.TaxableAmount = invoice.SubTotal + invoice.PackingCharges + invoice.TransportCharges;

if (invoice.IsInterState)
{
    invoice.IGSTAmount = Math.Round(invoice.TaxableAmount * invoice.IGSTRate / 100, 2);
    invoice.CGSTAmount = 0;
    invoice.SGSTAmount = 0;
}
else
{
    invoice.CGSTAmount = Math.Round(invoice.TaxableAmount * invoice.CGSTRate / 100, 2);
    invoice.SGSTAmount = Math.Round(invoice.TaxableAmount * invoice.SGSTRate / 100, 2);
    invoice.IGSTAmount = 0;
}

invoice.GrandTotal = invoice.TaxableAmount + invoice.CGSTAmount + invoice.SGSTAmount + invoice.IGSTAmount;
invoice.RoundOff = Math.Round(invoice.GrandTotal) - invoice.GrandTotal;
invoice.GrandTotal = Math.Round(invoice.GrandTotal);
invoice.AmountInWords = NumberToWordsConverter.Convert(invoice.GrandTotal);
```

---

## PDF Generation (QuestPDF)

### Invoice PDF

```
┌──────────────────────────────────────────────────────────┐
│                HYCOAT SYSTEMS PVT. LTD.                  │
│    Address | GSTIN: 24XXXXX | HSN: 99889                 │
│                     TAX INVOICE                          │
│                                                          │
│ Invoice No: INV-2025-042    Date: 16-Jun-2025            │
│ Customer: ABC Extrusions    GSTIN: 24ABCDE1234F1Z5      │
│ Address: Plot 42, GIDC      WO Ref: WO-2025-018         │
├──────────────────────────────────────────────────────────┤
│ Sr │ Section │ Color │ Qty │ Area(SFT) │ Rate │ Amount   │
│  1 │ 6063T5  │ RAL9016│ 200 │   860.2  │ 28   │ 24,086  │
│  2 │ 6061T6  │ RAL9016│ 150 │   548.7  │ 28   │ 15,364  │
│  3 │ 6005A   │ RAL7035│ 100 │   430.1  │ 30   │ 12,903  │
│    │         │        │     │          │      │         │
│    │         │        │ Sub Total:            │ 52,353  │
│    │         │        │ Packing Charges:      │  2,000  │
│    │         │        │ Transport Charges:    │  3,500  │
│    │         │        │ Taxable Amount:       │ 57,853  │
│    │         │        │ CGST @ 9%:            │  5,207  │
│    │         │        │ SGST @ 9%:            │  5,207  │
│    │         │        │ Round Off:            │   -0.14 │
│    │         │        │ GRAND TOTAL:          │ 68,267  │
│    │         │        │                       │         │
│    │ Amount in Words: Sixty-Eight Thousand Two Hundred   │
│    │ Sixty-Seven Rupees Only                             │
├──────────────────────────────────────────────────────────┤
│ Bank: [Bank Name] | A/C: [Account No] | IFSC: [Code]    │
│ Terms: Payment within 30 days                            │
├──────────────────────────────────────────────────────────┤
│ For HYCOAT SYSTEMS PVT. LTD.                             │
│ Authorised Signatory                                     │
└──────────────────────────────────────────────────────────┘
```

### Annexure PDF (Area Calculation Breakdown)

```
┌──────────────────────────────────────────────────────────┐
│                     ANNEXURE                             │
│ Invoice Ref: INV-2025-042                                │
│                                                          │
│ Sr │ Section │ Perimeter(mm) │ Length(mm) │ Qty │  SFT   │
│  1 │ 6063T5  │ 200           │ 4000       │ 200 │ 860.2  │
│  2 │ 6061T6  │ 180           │ 3500       │ 150 │ 548.7  │
│  3 │ 6005A   │ 160           │ 5000       │ 100 │ 430.1  │
│    │         │               │            │     │        │
│    │ Formula: (Perimeter × Length × Qty) ÷ 92903.04      │
│    │ Total Area: 1,839.0 SFT                             │
└──────────────────────────────────────────────────────────┘
```

Both pages combined into a single PDF document.

---

## Email Document Bundle

```csharp
POST /api/invoices/{id}/send-email
{
    "recipientEmails": ["customer@abc.com"],
    "subject": "HyCoat - Invoice INV-2025-042 & Dispatch Documents",
    "body": "Please find attached your invoice and dispatch documents."
}
```

Attachments gathered automatically:
1. Invoice PDF (with annexure)
2. Delivery Challan PDF
3. Test Certificate PDF
4. Packing List (if PDF exists)

All documents retrieved by WorkOrderId linkage.

---

## Validation
- `Date`, `CustomerId`, `WorkOrderId` required
- At least 1 line item
- PerimeterMM > 0, LengthMM > 0, Quantity > 0, RatePerSFT > 0
- GST rates: 0–28%
- IsInterState: if true, IGST used; if false, CGST+SGST used
- Status must be "Draft", "Finalized", "Sent", "Paid"
- Updates only allowed in Draft status
- Invoice can only be created for WOs with an existing DC

---

## UI Pages

### InvoicesPage (`/dispatch/invoices`)
Columns: Invoice #, Date, Customer, WO #, Total SFT, Grand Total, Status.
Status chips: gray=Draft, blue=Finalized, green=Sent, purple=Paid.
Filters: date range, customer, status.
Action: "Send Email" button for Finalized invoices.

### InvoiceFormPage (`/dispatch/invoices/new` or `:id`)

```
┌──────────────────────────────────────────────────────────────┐
│ PageHeader: "Tax Invoice"                                    │
├──────────────────────────────────────────────────────────────┤
│ Work Order*: [Autocomplete___▼]  [Auto-Fill] button          │
│ Customer: ABC Extrusions (auto)  GSTIN: 24ABCDE... (auto)   │
│ Date*: [Today]  DC Ref: [DC-2025-035 ▼]                     │
│ HSN/SAC: [99889]  Inter-State: [No ○] [Yes ○]               │
├──────────────────────────────────────────────────────────────┤
│ LINE ITEMS                                  [+ Add Line]     │
│ ┌────────┬───────┬──────────┬────────┬─────┬───────┬───────┐ │
│ │Section │Color  │Perim(mm) │Len(mm) │ Qty │ SFT   │Rate/SF│ │
│ ├────────┼───────┼──────────┼────────┼─────┼───────┼───────┤ │
│ │6063T5  │RAL9016│200       │4000    │ 200 │ 860.2 │ 28.00 │ │
│ │6061T6  │RAL9016│180       │3500    │ 150 │ 548.7 │ 28.00 │ │
│ └────────┴───────┴──────────┴────────┴─────┴───────┴───────┘ │
│                                                              │
│ ┌────────────────────────────────────┐                        │
│ │ Sub Total:        ₹ 52,353.00     │                        │
│ │ Packing Charges:  ₹  2,000.00     │                        │
│ │ Transport Charges:₹  3,500.00     │                        │
│ │ Taxable Amount:   ₹ 57,853.00     │                        │
│ │ CGST @ [9]%:      ₹  5,206.77     │                        │
│ │ SGST @ [9]%:      ₹  5,206.77     │                        │
│ │ Round Off:         ₹     -0.54    │                        │
│ │ ═══════════════════════════════    │                        │
│ │ GRAND TOTAL:      ₹ 68,266.00     │                        │
│ │ (Sixty-Eight Thousand...)         │                        │
│ └────────────────────────────────────┘                        │
│ Payment Terms: [Within 30 days________]                      │
│ Bank: [auto from settings]                                   │
├──────────────────────────────────────────────────────────────┤
│ [Cancel]  [Save Draft]  [Finalize]  [Generate PDF & Finalize]│
└──────────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- "Auto-Fill" button calls `/api/invoices/auto-fill/{woId}` to pre-fill from WO + DC data
- SFT auto-calculated live as perimeter/length/qty change
- Line amounts auto-calculated: SFT × Rate
- Totals, GST, grand total all live-computed
- Amount in words auto-generated
- Inter-state toggle switches between CGST+SGST and IGST
- "Finalize" locks the invoice (no further edits)
- "Generate PDF" creates the invoice + annexure PDF
- Bank details auto-filled from app settings/config

---

## Business Rules
1. Area formula: (PerimeterMM × LengthMM × Qty) ÷ 92903.04
2. GST: Intra-state = CGST + SGST; Inter-state = IGST
3. Standard GST rate: 18% (9% + 9% CGST/SGST) — configurable
4. RoundOff applied to nearest rupee
5. AmountInWords auto-generated
6. Invoice can only be created for WOs with a DC
7. Edits only in Draft status; Finalized is locked
8. PDF includes both invoice page and annexure (area breakdown)
9. Email sends all dispatch documents as bundle
10. Invoice number auto-generated: INV-YYYY-NNN
11. Customer data snapshotted at invoice creation
12. Bank details configurable via app settings

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/InvoicesController.cs` | CRUD + PDF + email + auto-fill |
| `Services/Dispatch/IInvoiceService.cs` + impl | Logic, area calc, GST |
| `Services/Dispatch/InvoicePdfService.cs` | QuestPDF (invoice + annexure) |
| `Services/Dispatch/DocumentBundleEmailService.cs` | Bundle & email all docs |
| `Services/Shared/NumberToWordsConverter.cs` | Rupee amount to words |
| `DTOs/Dispatch/CreateInvoiceDto.cs` | |
| `DTOs/Dispatch/InvoiceDto.cs` | |
| `DTOs/Dispatch/InvoiceDetailDto.cs` | |
| `DTOs/Dispatch/InvoiceAutoFillDto.cs` | |
| `DTOs/Dispatch/SendEmailDto.cs` | |
| `Validators/Dispatch/CreateInvoiceValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/dispatch/InvoicesPage.jsx` | List |
| `src/pages/dispatch/InvoiceFormPage.jsx` | Form with live calc |
| `src/hooks/useInvoices.js` | React Query hooks |
| `src/services/invoiceService.js` | API calls |
| `src/utils/areaCalculator.js` | Client-side SFT calculation |
| `src/utils/numberToWords.js` | Client-side amount in words |

## Acceptance Criteria
1. Area auto-calculated per line: (Perimeter × Length × Qty) ÷ 92903.04
2. Line amount auto-calculated: SFT × Rate
3. GST correctly calculated (intra/inter-state toggle)
4. Grand total with round-off to nearest rupee
5. Amount in words auto-generated
6. Auto-fill from WO + DC data (one click)
7. PDF with professional invoice layout + annexure breakdown
8. Email sends document bundle (invoice + DC + TC + packing list)
9. Invoice number auto-generated: INV-YYYY-NNN
10. Edits locked after Finalize
11. Customer snapshot at creation
12. Live totals update as user types

## Reference
- **WORKFLOWS.md:** Workflow 8 — Dispatch (Invoice section)
- **01-database-schema.md:** Invoice, InvoiceLineItem entities
