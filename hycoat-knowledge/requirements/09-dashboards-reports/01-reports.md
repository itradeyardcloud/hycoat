# 09-dashboards-reports/01-reports

## Feature ID
`09-dashboards-reports/01-reports`

## Feature Name
Reports — Order Tracker, Throughput, Exports (API + UI)

## Dependencies
- `09-dashboards-reports/00-dashboards` — Shares chart components and service patterns
- All prior features (reports aggregate data from all modules)

## Business Context
Beyond real-time dashboards, users need structured reports for analysis, review meetings, and record-keeping. Key reports:
1. **Order Tracker** — End-to-end status of each order from Inquiry → Dispatch, showing current stage and completion %
2. **Production Throughput** — Daily/weekly SFT and kg output by customer, shift, line
3. **Powder Consumption** — Powder used vs. ordered vs. stock, per order
4. **Quality Summary** — Pass/fail rates, DFT trends, rework statistics
5. **Customer History** — All orders, invoices, amounts for a customer
6. **Dispatch Register** — All DCs with dates, quantities, vehicle details

Reports can be filtered by date range, customer, etc. and exported to Excel (XLSX).

**Workflow Reference:** WORKFLOWS.md → Module 9 — Reports & Dashboard.

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/reports/order-tracker` | Admin, Leader, Sales, PPC | Order status tracker |
| GET | `/api/reports/production-throughput` | Admin, Leader, Production, PPC | Production output |
| GET | `/api/reports/powder-consumption` | Admin, Leader, Purchase, Production | Powder usage |
| GET | `/api/reports/quality-summary` | Admin, Leader, QA | Quality metrics |
| GET | `/api/reports/customer-history/{customerId}` | Admin, Leader, Sales, Finance | Customer orders/invoices |
| GET | `/api/reports/dispatch-register` | Admin, Leader, SCM | Dispatch records |
| GET | `/api/reports/{reportType}/export` | Same as GET | Export to XLSX |

**Common Query Parameters:**
- `dateFrom`, `dateTo`
- `customerId`
- `page`, `pageSize` (for paginated view)
- `format` — "json" (default) or "xlsx" (export)

---

## Report DTOs

### OrderTrackerDto
```csharp
public class OrderTrackerDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public string CurrentStage { get; set; }          // "MaterialInward", "Pretreatment", "Coating", "QA", "Dispatch"
    public int CompletionPercent { get; set; }         // 0–100
    public bool InquiryDone { get; set; }
    public bool QuotationDone { get; set; }
    public bool PIDone { get; set; }
    public bool MaterialReceived { get; set; }
    public bool IncomingInspectionDone { get; set; }
    public bool PWOCreated { get; set; }
    public bool PretreatmentDone { get; set; }
    public bool CoatingDone { get; set; }
    public bool FinalInspectionDone { get; set; }
    public bool Dispatched { get; set; }
    public bool Invoiced { get; set; }
    public int DaysInProcess { get; set; }
}
```

### ProductionThroughputDto
```csharp
public class ProductionThroughputDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; }
    public string CustomerName { get; set; }
    public string PWONumber { get; set; }
    public decimal AreaSFT { get; set; }
    public decimal PowderUsedKg { get; set; }
    public int BasketsProcessed { get; set; }
}

public class ThroughputSummaryDto
{
    public decimal TotalSFT { get; set; }
    public decimal TotalPowderKg { get; set; }
    public int TotalBaskets { get; set; }
    public List<ProductionThroughputDto> Details { get; set; }
    public List<ChartPointDto> DailyTrend { get; set; }
}
```

### PowderConsumptionDto
```csharp
public class PowderConsumptionDto
{
    public string PowderCode { get; set; }
    public string ColorName { get; set; }
    public decimal OrderedKg { get; set; }
    public decimal ReceivedKg { get; set; }
    public decimal ConsumedKg { get; set; }
    public decimal CurrentStockKg { get; set; }
    public decimal WastagePercent { get; set; }
}
```

### QualitySummaryDto
```csharp
public class QualitySummaryDto
{
    public int TotalInspections { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public int ReworkCount { get; set; }
    public decimal PassRate { get; set; }
    public decimal AvgDFT { get; set; }
    public List<ChartPointDto> DFTTrend { get; set; }
    public List<StatusCountDto> FailureReasons { get; set; }
}
```

### CustomerHistoryDto
```csharp
public class CustomerHistoryDto
{
    public string CustomerName { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalAreaSFT { get; set; }
    public decimal TotalInvoicedAmount { get; set; }
    public List<CustomerOrderDto> Orders { get; set; }
}

public class CustomerOrderDto
{
    public string WorkOrderNumber { get; set; }
    public DateTime Date { get; set; }
    public decimal AreaSFT { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? InvoiceAmount { get; set; }
    public string Status { get; set; }
}
```

### DispatchRegisterDto
```csharp
public class DispatchRegisterDto
{
    public string DCNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public string WorkOrderNumber { get; set; }
    public int TotalQuantity { get; set; }
    public string? VehicleNumber { get; set; }
    public string Status { get; set; }
    public string? InvoiceNumber { get; set; }
}
```

---

## Excel Export

Use **ClosedXML** (NuGet) for XLSX generation:

```csharp
// Services/Reports/ExcelExportService.cs
public byte[] ExportToExcel<T>(List<T> data, string sheetName)
{
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add(sheetName);
    worksheet.Cell(1, 1).InsertTable(data);
    worksheet.Columns().AdjustToContents();

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return stream.ToArray();
}
```

Export endpoint returns `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` with filename header.

---

## UI Pages

### OrderTrackerPage (`/reports/order-tracker`)

```
┌──────────────────────────────────────────────────────────────┐
│ PageHeader: "Order Tracker"         [Export Excel]           │
│ Filters: [Customer ▼] [Date From] [Date To] [Search___]     │
├──────────────────────────────────────────────────────────────┤
│ WO#       │Customer │Stage      │Progress │Days│            │
│ WO-2025-18│ABC Ext  │QA         │████████░░ 80%│ 5  │       │
│ WO-2025-17│XYZ Fab  │Dispatch   │██████████ 95%│ 8  │       │
│ WO-2025-16│PQR Alu  │Coating    │██████░░░░ 60%│ 3  │       │
├──────────────────────────────────────────────────────────────┤
│ Expand row → Stage checklist:                                │
│ ✅ Inquiry ✅ Quotation ✅ PI ✅ Material Inward              │
│ ✅ Incoming Inspection ✅ PWO ✅ Pretreatment                 │
│ 🔄 Coating ⬜ Final Inspection ⬜ Dispatch ⬜ Invoice         │
└──────────────────────────────────────────────────────────────┘
```

### ProductionReportPage (`/reports/production`)
- Summary cards: Total SFT, Total Baskets, Total Powder (kg)
- Daily trend bar chart
- Detail table with date/shift/customer/PWO/SFT

### QualityReportPage (`/reports/quality`)
- KPI cards: Pass rate, Avg DFT, Rework count
- DFT trend line chart
- Failure reasons pie chart

### CustomerHistoryPage (`/reports/customer/:id`)
- Customer info header
- Summary: total orders, total SFT, total invoiced
- Orders table with WO#, date, SFT, invoice ref, status

### DispatchRegisterPage (`/reports/dispatch`)
- Table: DC#, Date, Customer, WO#, Qty, Vehicle, Status, Invoice
- Date range filter, export to Excel

**Key behaviors:**
- All report pages have "Export to Excel" button
- Filters applied to both table and charts
- Expandable rows for detailed breakdown
- Progress bars use color coding (green > 80%, yellow > 50%, red < 50%)
- Pagination for large datasets
- Order tracker shows pipeline-style progress visualization

---

## Business Rules
1. Order completion % calculated: 11 stages, each ~9% (rounded)
2. Stage determination: check which entities exist for the WO
3. DaysInProcess = today - WO creation date (for active WOs)
4. Reports are read-only
5. Excel export includes all filtered data (not just current page)
6. Powder consumption = ordered - current stock (approximation)
7. Quality pass rate = Approved / (Approved + Rejected + Rework)

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/ReportsController.cs` | All report endpoints |
| `Services/Reports/IReportService.cs` + impl | Report queries |
| `Services/Reports/ExcelExportService.cs` | XLSX generation |
| `DTOs/Reports/OrderTrackerDto.cs` | |
| `DTOs/Reports/ProductionThroughputDto.cs` | |
| `DTOs/Reports/PowderConsumptionDto.cs` | |
| `DTOs/Reports/QualitySummaryDto.cs` | |
| `DTOs/Reports/CustomerHistoryDto.cs` | |
| `DTOs/Reports/DispatchRegisterDto.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/reports/OrderTrackerPage.jsx` | Order pipeline |
| `src/pages/reports/ProductionReportPage.jsx` | Throughput |
| `src/pages/reports/QualityReportPage.jsx` | Quality metrics |
| `src/pages/reports/CustomerHistoryPage.jsx` | Per-customer |
| `src/pages/reports/DispatchRegisterPage.jsx` | Dispatch log |
| `src/components/reports/ProgressBar.jsx` | Stage progress |
| `src/components/reports/StageChecklist.jsx` | Expandable stages |
| `src/hooks/useReports.js` | React Query hooks |
| `src/services/reportService.js` | API calls |

### NuGet Package
```xml
<PackageReference Include="ClosedXML" Version="0.104.*" />
```

## Acceptance Criteria
1. Order tracker shows each WO with stage progress bar and checklist
2. Expandable row shows all 11 stages with check/pending icons
3. Production throughput with daily/weekly aggregation and chart
4. Quality summary with pass rate, DFT trend, failure breakdown
5. Customer history with order list and totals
6. Dispatch register with full DC details
7. All reports filterable by date range, customer
8. Export to Excel (XLSX) for all report types
9. Excel includes all filtered data, not just current page
10. Pagination and search for large datasets

## Reference
- **WORKFLOWS.md:** Module 9 — Reports & Dashboard
- All entity definitions from `01-database-schema.md`
