# 09-dashboards-reports/00-dashboards

## Feature ID
`09-dashboards-reports/00-dashboards`

## Feature Name
Dashboards — Admin, Leader & Department (API + UI)

## Dependencies
- All prior features (dashboards aggregate data from all modules)
- `00-foundation/02-auth-system` — Role-based access
- `00-foundation/05-app-shell-layout` — DashboardLayout, route structure

## Business Context
Dashboards provide at-a-glance visibility into operations. Three dashboard tiers:
1. **Admin Dashboard** — Full business overview: total orders, revenue, production throughput, quality metrics, pending actions
2. **Leader Dashboard** — Same as Admin but scoped to their department's data plus cross-department KPIs
3. **Department Dashboards** — Each department sees metrics relevant to their function

Dashboards load KPI cards at the top and charts below, with links to drill down into specific lists.

**Workflow Reference:** WORKFLOWS.md → Module 9 — Reports & Dashboard.

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/dashboard/admin` | Admin | Admin KPIs + charts |
| GET | `/api/dashboard/leader` | Leader | Leader KPIs (dept scoped) |
| GET | `/api/dashboard/sales` | Sales, Admin, Leader | Sales KPIs |
| GET | `/api/dashboard/ppc` | PPC, Admin, Leader | PPC KPIs |
| GET | `/api/dashboard/production` | Production, Admin, Leader | Production KPIs |
| GET | `/api/dashboard/quality` | QA, Admin, Leader | Quality KPIs |
| GET | `/api/dashboard/scm` | SCM, Admin, Leader | SCM/Dispatch KPIs |
| GET | `/api/dashboard/purchase` | Purchase, Admin, Leader | Purchase KPIs |
| GET | `/api/dashboard/finance` | Finance, Admin, Leader | Finance KPIs |

**Query Parameters (common):**
- `dateFrom`, `dateTo` — default: current month
- `period` — "today", "week", "month", "quarter", "year"

---

## Dashboard Data Structures

### AdminDashboardDto
```csharp
public class AdminDashboardDto
{
    // KPI Cards
    public int ActiveWorkOrders { get; set; }
    public int PendingInquiries { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyProductionSFT { get; set; }
    public int PendingDispatches { get; set; }
    public decimal QualityPassRate { get; set; }
    public int LowStockAlerts { get; set; }
    public int OverdueWorkOrders { get; set; }

    // Chart Data
    public List<ChartPointDto> RevenueTrend { get; set; }        // Monthly revenue
    public List<ChartPointDto> ProductionThroughput { get; set; } // Daily SFT output
    public List<StatusCountDto> WorkOrdersByStatus { get; set; }  // Pie chart
    public List<CustomerRevenueDto> TopCustomers { get; set; }    // Bar chart
}

public class ChartPointDto
{
    public string Label { get; set; }
    public decimal Value { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; }
    public int Count { get; set; }
}

public class CustomerRevenueDto
{
    public string CustomerName { get; set; }
    public decimal Revenue { get; set; }
    public decimal AreaSFT { get; set; }
}
```

### SalesDashboardDto
```csharp
public class SalesDashboardDto
{
    public int OpenInquiries { get; set; }
    public int QuotationsSentThisMonth { get; set; }
    public int PIsAwaitingApproval { get; set; }
    public int ActiveWorkOrders { get; set; }
    public decimal QuotationToWOConversionRate { get; set; }
    public List<InquiryAgingDto> InquiryAging { get; set; }
    public List<ChartPointDto> MonthlyQuotations { get; set; }
}
```

### PPCDashboardDto
```csharp
public class PPCDashboardDto
{
    public int ActivePWOs { get; set; }
    public int UnscheduledPWOs { get; set; }
    public decimal WeekUtilizationPercent { get; set; }
    public decimal TotalScheduledSFT { get; set; }
    public List<ChartPointDto> WeeklyScheduleLoad { get; set; }
    public List<StatusCountDto> PWOsByStatus { get; set; }
}
```

### ProductionDashboardDto
```csharp
public class ProductionDashboardDto
{
    public int BasketsProcessedToday { get; set; }
    public decimal SFTCoatedToday { get; set; }
    public int ActiveProductionLogs { get; set; }
    public decimal AvgConveyorSpeed { get; set; }
    public List<ChartPointDto> DailyOutput { get; set; }
}
```

### QualityDashboardDto
```csharp
public class QualityDashboardDto
{
    public int InspectionsToday { get; set; }
    public decimal DFTPassRate { get; set; }
    public int PendingFinalInspections { get; set; }
    public int TestCertificatesIssued { get; set; }
    public decimal OverallPassRate { get; set; }
    public List<ChartPointDto> DFTTrend { get; set; }
    public List<StatusCountDto> InspectionResults { get; set; }
}
```

### SCMDashboardDto
```csharp
public class SCMDashboardDto
{
    public int MaterialsReceivedToday { get; set; }
    public int PendingDispatches { get; set; }
    public int ChallansDraftedToday { get; set; }
    public int DispatchedThisWeek { get; set; }
    public List<StatusCountDto> DCsByStatus { get; set; }
}
```

### PurchaseDashboardDto
```csharp
public class PurchaseDashboardDto
{
    public int OpenIndents { get; set; }
    public int PendingPOs { get; set; }
    public int LowStockItems { get; set; }
    public decimal TotalPowderStockKg { get; set; }
    public List<PowderStockDto> LowStockAlerts { get; set; }
    public List<ChartPointDto> MonthlyPurchaseSpend { get; set; }
}
```

### FinanceDashboardDto
```csharp
public class FinanceDashboardDto
{
    public decimal MonthlyInvoicedAmount { get; set; }
    public int InvoicesSentThisMonth { get; set; }
    public int UnpaidInvoices { get; set; }
    public decimal OutstandingAmount { get; set; }
    public List<ChartPointDto> MonthlyRevenue { get; set; }
    public List<StatusCountDto> InvoicesByStatus { get; set; }
}
```

---

## UI Pages

### AdminDashboardPage (`/dashboard` — default landing for Admin)

```
┌──────────────────────────────────────────────────────────────┐
│ HYCOAT ERP — Dashboard               Period: [This Month ▼] │
├──────────────────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│ │ Active   │ │ Monthly  │ │ Monthly  │ │ Quality  │        │
│ │ WOs: 24  │ │ ₹8.5L    │ │ 12,450   │ │ 97.2%    │        │
│ │          │ │ Revenue  │ │ SFT      │ │ Pass Rate│        │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘        │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│ │ Pending  │ │ Pending  │ │ Low Stock│ │ Overdue  │        │
│ │ Inquiry:5│ │ Dispatch:3│ │ Alerts:2 │ │ WOs: 1   │        │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘        │
├──────────────────────────────────────────────────────────────┤
│ Revenue Trend (line chart)    │ WO Status (pie chart)       │
│ ┌─────────────────────────┐   │ ┌─────────────────────────┐ │
│ │      ╱──╲               │   │ │      ████               │ │
│ │   ╱╱    ╲╲──╱╱          │   │ │   ░░░    ██             │ │
│ │ ╱╱         ╲╲           │   │ │   ░░░░░░░ ██            │ │
│ │ Jan Feb Mar Apr May Jun │   │ │ Active Completed WIP    │ │
│ └─────────────────────────┘   │ └─────────────────────────┘ │
├──────────────────────────────────────────────────────────────┤
│ Production Throughput (bar)   │ Top 5 Customers (bar)       │
│ ┌─────────────────────────┐   │ ┌─────────────────────────┐ │
│ │ ███                      │   │ │ ABC Ext:  ██████████    │ │
│ │ ████                     │   │ │ XYZ Fab:  ████████      │ │
│ │ ██████                   │   │ │ PQR Alu:  ██████        │ │
│ └─────────────────────────┘   │ └─────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

### Department Dashboards
Each department gets a route: `/dashboard/sales`, `/dashboard/ppc`, etc.
Same layout pattern: KPI cards on top, charts below.
Users see their department dashboard as default landing page.

**Key behaviors:**
- Period selector: Today, This Week, This Month, This Quarter, Custom Range
- KPI cards are clickable — link to relevant list page (e.g., click "Active WOs" → WO list filtered by Active)
- Charts use recharts: LineChart, BarChart, PieChart, AreaChart
- Dashboard auto-refreshes every 60 seconds (React Query refetchInterval)
- Low stock and overdue alerts highlighted in red/orange
- Mobile: KPI cards stack vertically, charts full-width

---

## Business Rules
1. Dashboard data is read-only (no mutations)
2. Admin sees all data; Leader sees department-scoped + cross-dept KPIs
3. Department users see only their department dashboard
4. Default period: current month
5. Revenue = sum of Finalized/Sent/Paid invoices
6. Quality pass rate = (Approved final inspections) / (Total final inspections) × 100
7. Production throughput = sum of AreaSFT from completed PWOs
8. Low stock = PowderStock where CurrentStockKg < ReorderLevelKg
9. Overdue WOs = WOs past expected completion date that aren't Completed/Dispatched
10. KPI cards link to filtered list pages for drill-down

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/DashboardController.cs` | All dashboard endpoints |
| `Services/Dashboard/IDashboardService.cs` + impl | Aggregation queries |
| `DTOs/Dashboard/AdminDashboardDto.cs` | |
| `DTOs/Dashboard/SalesDashboardDto.cs` | |
| `DTOs/Dashboard/PPCDashboardDto.cs` | |
| `DTOs/Dashboard/ProductionDashboardDto.cs` | |
| `DTOs/Dashboard/QualityDashboardDto.cs` | |
| `DTOs/Dashboard/SCMDashboardDto.cs` | |
| `DTOs/Dashboard/PurchaseDashboardDto.cs` | |
| `DTOs/Dashboard/FinanceDashboardDto.cs` | |
| `DTOs/Dashboard/ChartPointDto.cs` | Shared |
| `DTOs/Dashboard/StatusCountDto.cs` | Shared |

### UI
| File | Purpose |
|---|---|
| `src/pages/dashboard/AdminDashboardPage.jsx` | Admin |
| `src/pages/dashboard/SalesDashboardPage.jsx` | Sales |
| `src/pages/dashboard/PPCDashboardPage.jsx` | PPC |
| `src/pages/dashboard/ProductionDashboardPage.jsx` | Production |
| `src/pages/dashboard/QualityDashboardPage.jsx` | QA |
| `src/pages/dashboard/SCMDashboardPage.jsx` | SCM |
| `src/pages/dashboard/PurchaseDashboardPage.jsx` | Purchase |
| `src/pages/dashboard/FinanceDashboardPage.jsx` | Finance |
| `src/components/dashboard/KPICard.jsx` | Reusable KPI card |
| `src/components/dashboard/DashboardChart.jsx` | Chart wrapper |
| `src/components/dashboard/PeriodSelector.jsx` | Date range picker |
| `src/hooks/useDashboard.js` | React Query hooks |
| `src/services/dashboardService.js` | API calls |

## Acceptance Criteria
1. Admin dashboard shows 8 KPI cards with business-wide metrics
2. Each department has its own dashboard with relevant KPIs
3. Period selector (Today/Week/Month/Quarter/Custom)
4. Charts render correctly: line, bar, pie using recharts
5. KPI cards clickable — navigate to filtered list pages
6. Auto-refresh every 60 seconds
7. Low stock and overdue alerts highlighted
8. Mobile-responsive: stacked cards, full-width charts
9. Role-based access: users see only their department dashboard
10. Dashboard loads within reasonable time (optimize queries)

## Reference
- **WORKFLOWS.md:** Module 9 — Reports & Dashboard
- All entity definitions from `01-database-schema.md`
