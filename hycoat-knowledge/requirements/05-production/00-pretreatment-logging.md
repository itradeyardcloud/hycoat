# 05-production/00-pretreatment-logging

## Feature ID
`05-production/00-pretreatment-logging`

## Feature Name
Pretreatment Production Logging (API + UI)

## Dependencies
- `04-ppc/00-production-work-order` — PWO entity, PWO must be scheduled/in-progress
- `00-foundation/01-database-schema` — PretreatmentLog, PretreatmentTankReading entities

## Business Context
Pretreatment is the first production process. Aluminum profiles go through a 10-tank chemical sequence (degreasing → rinsing → etching → chromating → drying). QA Lab performs morning titration to determine chemical concentrations and calculates etch times per basket. The operator logs each basket's start/end times and etch duration. This log sheet is critical for quality traceability. Operators use tablets/phones on the production floor.

**Workflow Reference:** WORKFLOWS.md → Workflow 5 — Pretreatment Production.

---

## Entities (from 01-database-schema)
- **PretreatmentLog** — Date, Shift, ProductionWorkOrderId, BasketNumber, StartTime, EndTime, EtchTimeMins, OperatorUserId, QASignOffUserId, Remarks
- **PretreatmentTankReading** — PretreatmentLogId, TankName, Concentration, Temperature, ChemicalAdded, ChemicalQty

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/pretreatment-logs` | Production, QA, PPC, Admin, Leader | List |
| GET | `/api/pretreatment-logs/{id}` | Production, QA, PPC, Admin, Leader | Detail with tank readings |
| POST | `/api/pretreatment-logs` | Production, QA, Admin, Leader | Create log entry for a basket |
| PUT | `/api/pretreatment-logs/{id}` | Production, QA, Admin, Leader | Update |
| DELETE | `/api/pretreatment-logs/{id}` | Admin | Soft delete |
| GET | `/api/pretreatment-logs/by-pwo/{pwoId}` | All auth'd | All logs for a specific PWO |
| POST | `/api/pretreatment-logs/{id}/tank-readings` | QA, Admin, Leader | Add/update tank readings |

**Query Parameters:**
- `search` — PWO number, Customer name
- `date`, `shift`
- `productionWorkOrderId`
- `page`, `pageSize`

---

## DTOs

### CreatePretreatmentLogDto
```csharp
public class CreatePretreatmentLogDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; }               // "Day", "Night"
    public int ProductionWorkOrderId { get; set; }
    public int BasketNumber { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? EtchTimeMins { get; set; }
    public string? Remarks { get; set; }
    public List<TankReadingDto> TankReadings { get; set; }
}

public class TankReadingDto
{
    public string TankName { get; set; }             // e.g., "Degreasing", "Etching", etc.
    public decimal? Concentration { get; set; }
    public decimal? Temperature { get; set; }
    public string? ChemicalAdded { get; set; }
    public decimal? ChemicalQty { get; set; }
}
```

### PretreatmentLogDetailDto
```csharp
public class PretreatmentLogDetailDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Shift { get; set; }
    public string PWONumber { get; set; }
    public string CustomerName { get; set; }
    public int BasketNumber { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? EtchTimeMins { get; set; }
    public string? OperatorName { get; set; }
    public string? QASignOffName { get; set; }
    public string? Remarks { get; set; }
    public List<TankReadingDto> TankReadings { get; set; }
}
```

---

## Tank Reading Template

Standard 10-tank sequence. When creating a log, pre-populate empty readings for:
1. Degreasing
2. Water Rinse 1
3. Etching
4. Water Rinse 2
5. Deoxidizing / De-smutting
6. Water Rinse 3
7. Chrome Conversion Coating
8. Water Rinse 4
9. DI Water Rinse
10. Oven Dry

QA fills in concentration, temperature, and chemical additions for key tanks (Degreasing, Etching, Chromating). Others may be left blank.

---

## Validation
- `Date`, `Shift` — required
- `ProductionWorkOrderId` — required, must exist and be in InProgress status
- `BasketNumber` — must be > 0, should be sequential per PWO (warn if gap)
- `EtchTimeMins` — if provided, must be > 0
- `TankName` — must be from the predefined list

---

## UI Pages

### PretreatmentLogsPage (`/production/pretreatment`)
Columns: Date, Shift, PWO Number, Customer, Basket #, Etch Time, Operator, QA.
Filters: date, shift, PWO.
Group by date and PWO for easier viewing.

### PretreatmentLogFormPage (`/production/pretreatment/new` or `:id`)

**Optimized for tablet/mobile entry by production floor operators:**

```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Pretreatment Log Entry"                     │
├──────────────────────────────────────────────────────────┤
│ PWO*: [Autocomplete — active PWOs only_______________▼]  │
│ Date*: [Today — DatePicker]  Shift*: [Day ○] [Night ○]  │
│ Basket Number*: [1___]                                   │
│ Start Time: [09:30]   End Time: [10:15]                  │
│ Etch Time (min): [25__]                                  │
├──────────────────────────────────────────────────────────┤
│ TANK READINGS                                            │
│                                                          │
│ ┌─ 1. Degreasing ──────────────────────────────────────┐│
│ │ Conc (point): [3.5__]  Temp (°C): [55___]            ││
│ │ Chemical Added: [________________]  Qty: [____]      ││
│ └──────────────────────────────────────────────────────┘│
│                                                          │
│ ┌─ 3. Etching ─────────────────────────────────────────┐│
│ │ Conc (point): [6.2__]  Temp (°C): [60___]            ││
│ │ Chemical Added: [Sodium Hydroxide__]  Qty: [2.5_]    ││
│ └──────────────────────────────────────────────────────┘│
│                                                          │
│ ┌─ 7. Chrome Conversion ───────────────────────────────┐│
│ │ Conc (point): [4.0__]  Temp (°C): [25___]            ││
│ │ Chemical Added: [________________]  Qty: [____]      ││
│ └──────────────────────────────────────────────────────┘│
│ (Other tanks: expandable/collapsible — empty by default) │
├──────────────────────────────────────────────────────────┤
│ Remarks: [________________________]                      │
├──────────────────────────────────────────────────────────┤
│     [Cancel]                 [Save Log Entry]            │
└──────────────────────────────────────────────────────────┘
```

**Key behaviors:**
- PWO autocomplete shows only PWOs with status InProgress
- Date defaults to today, shift auto-detects based on current time (before 6pm = Day, after = Night)
- Basket number suggests next sequential number for the selected PWO
- Key tanks (Degreasing, Etching, Chromating) expanded by default; others collapsed
- Large input fields and touch-friendly buttons for tablet use
- Operator auto-tagged from current logged-in user
- QA sign-off field available for QA team to endorse

---

## Business Rules
1. Logs are per-basket per PWO — multiple log entries per PWO expected
2. Basket numbers should be sequential within a PWO
3. QA provides etch time instructions at the start of each shift (entered as guidance)
4. Tank readings are optional for most tanks; Etching is most critical
5. Production department creates logs; QA can add tank readings and sign off
6. These logs are used for traceability in case of quality issues downstream
7. End Time - Start Time should roughly match EtchTimeMins + processing time

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/PretreatmentLogsController.cs` | CRUD + tank readings |
| `Services/Production/IPretreatmentLogService.cs` + impl | Logic |
| `DTOs/Production/CreatePretreatmentLogDto.cs` | Create |
| `DTOs/Production/PretreatmentLogDto.cs` | List |
| `DTOs/Production/PretreatmentLogDetailDto.cs` | Detail |
| `DTOs/Production/TankReadingDto.cs` | Tank reading |
| `Validators/Production/CreatePretreatmentLogValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/production/PretreatmentLogsPage.jsx` | List grouped by date/PWO |
| `src/pages/production/PretreatmentLogFormPage.jsx` | Form with tank readings |
| `src/hooks/usePretreatmentLogs.js` | React Query hooks |
| `src/services/pretreatmentLogService.js` | API calls |

## Acceptance Criteria
1. Log entries are per-basket per PWO
2. Tank reading sections pre-populated with 10-tank template
3. Key tanks expanded by default, others collapsible
4. Date defaults to today, shift auto-detected
5. Basket number auto-suggests next sequential for the PWO
6. PWO autocomplete shows only InProgress PWOs
7. Operator auto-tagged from logged-in user
8. Touch-friendly input fields for tablet use
9. QA sign-off field available
10. Logs grouped by date and PWO on list page

## Reference
- **WORKFLOWS.md:** Workflow 5 — Pretreatment Production
- **01-database-schema.md:** PretreatmentLog, PretreatmentTankReading entities
