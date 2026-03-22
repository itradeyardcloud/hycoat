# 04-ppc/01-production-schedule

## Feature ID
`04-ppc/01-production-schedule`

## Feature Name
Production Schedule (API + UI — Calendar/Planner View)

## Dependencies
- `04-ppc/00-production-work-order` — PWO entity, time calculations
- `01-masters/00-master-data-api` — ProductionUnit lookup
- `00-foundation/01-database-schema` — ProductionSchedule entity

## Business Context
PPC creates a weekly production schedule allocating PWOs to specific dates, shifts (Day/Night), and production units. This is the factory planner — the production floor relies on it to know what to work on each shift. The visual planner should show a week view with drag-and-drop capability for rescheduling.

**Workflow Reference:** WORKFLOWS.md → Workflow 3 — Weekly Production Schedule.

---

## Entities (from 01-database-schema)
- **ProductionSchedule** — Date, Shift ("Day"/"Night"), ProductionWorkOrderId, ProductionUnitId, SortOrder, Status, Notes

**Status Flow:** `Planned` → `InProgress` → `Completed` | `Cancelled`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/production-schedule` | PPC, Production, Admin, Leader | Get schedule for date range |
| POST | `/api/production-schedule` | PPC, Admin, Leader | Create schedule entry |
| PUT | `/api/production-schedule/{id}` | PPC, Admin, Leader | Update (reschedule) |
| PATCH | `/api/production-schedule/{id}/status` | PPC, Production, Admin, Leader | Status update |
| DELETE | `/api/production-schedule/{id}` | PPC, Admin | Remove from schedule |
| POST | `/api/production-schedule/reorder` | PPC, Admin, Leader | Reorder entries within a day/shift |

**Query Parameters (GET):**
- `startDate`, `endDate` — required, defines the week/range
- `productionUnitId` — optional filter
- `shift` — optional: "Day", "Night"

**GET Response:**
```json
{
  "scheduleEntries": [
    {
      "id": 1,
      "date": "2025-01-20",
      "shift": "Day",
      "productionUnitId": 1,
      "productionUnitName": "Unit-1",
      "pwoNumber": "PWO-2025-003",
      "customerName": "Ajit Coatings",
      "powderColor": "RAL 9007",
      "totalTimeHrs": 11.0,
      "sortOrder": 1,
      "status": "Planned"
    }
  ]
}
```

### Reorder Endpoint
```csharp
// POST /api/production-schedule/reorder
public class ReorderScheduleDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; }
    public int ProductionUnitId { get; set; }
    public List<int> ScheduleIds { get; set; }  // Ordered list of IDs
}
```

---

## UI Page

### ProductionSchedulePage (`/ppc/schedule`)

**Desktop Layout — Week Calendar View:**
```
┌──────────────────────────────────────────────────────────┐
│ PageHeader: "Production Schedule"                        │
│ Unit: [Unit-1 ▼]    Week: [< 20 Jan - 26 Jan 2025 >]   │
├──────────────────────────────────────────────────────────┤
│        │ Mon 20  │ Tue 21  │ Wed 22  │ Thu 23  │ ...    │
├────────┼─────────┼─────────┼─────────┼─────────┼────────┤
│        │ ┌─────┐ │ ┌─────┐ │         │ ┌─────┐ │        │
│  Day   │ │PWO-3│ │ │PWO-5│ │ (empty) │ │PWO-7│ │        │
│ Shift  │ │Ajit │ │ │XYZ  │ │         │ │ABC  │ │        │
│        │ │11hrs│ │ │8hrs │ │ [+ Add] │ │6hrs │ │        │
│        │ └─────┘ │ └─────┘ │         │ └─────┘ │        │
│        │ ┌─────┐ │         │         │         │        │
│        │ │PWO-4│ │         │         │         │        │
│        │ └─────┘ │         │         │         │        │
├────────┼─────────┼─────────┼─────────┼─────────┼────────┤
│        │         │ ┌─────┐ │         │         │        │
│ Night  │ (empty) │ │PWO-6│ │ (empty) │ (empty) │        │
│ Shift  │         │ │DEF  │ │         │         │        │
│        │ [+ Add] │ │9hrs │ │         │         │        │
│        │         │ └─────┘ │         │         │        │
└────────┴─────────┴─────────┴─────────┴─────────┴────────┘
```

**Each card shows:**
- PWO number
- Customer name (shortened)
- Powder color
- Estimated hours
- Status badge (Planned / InProgress / Completed)

**Interactions:**
- **Drag & drop** cards between day/shift cells to reschedule (calls `PUT /api/production-schedule/{id}`)
- **Drag to reorder** within a cell (calls `POST /api/production-schedule/reorder`)
- **Click "+" button** → dialog to assign an existing PWO to that date/shift
- **Click card** → opens PWO detail in a side panel or navigates to PWO page
- **Week navigation** ← → arrows to move between weeks

**Mobile Layout — List View:**
```
┌──────────────────────────┐
│ Monday, 20 Jan           │
│ ┌──────────────────────┐ │
│ │ Day Shift             │ │
│ │ PWO-003 — Ajit (11h) │ │
│ │ PWO-004 — GHI (6h)   │ │
│ └──────────────────────┘ │
│ ┌──────────────────────┐ │
│ │ Night Shift           │ │
│ │ (No orders scheduled) │ │
│ └──────────────────────┘ │
│                          │
│ Tuesday, 21 Jan          │
│ ...                      │
└──────────────────────────┘
```

On mobile, drag-and-drop is disabled. Instead, tap a card → options menu with "Reschedule" action.

---

## Assign PWO Dialog

When clicking "+" or "Add" in an empty slot:
```
┌───────────────────────────────────────────────┐
│ Assign PWO to: Monday 20 Jan — Day Shift     │
├───────────────────────────────────────────────┤
│ Production Work Order*: [Autocomplete_____▼]  │
│ (Shows unscheduled PWOs only)                │
├───────────────────────────────────────────────┤
│ Notes: [________________________]            │
├───────────────────────────────────────────────┤
│        [Cancel]           [Assign]            │
└───────────────────────────────────────────────┘
```

**PWO Autocomplete** filters to show only PWOs with status `Created` that don't already have a schedule entry.

---

## Validation
- `Date` — required
- `Shift` — required, must be "Day" or "Night"
- `ProductionWorkOrderId` — required, must exist
- `ProductionUnitId` — required, must exist
- A PWO can only be scheduled once per date+shift+unit combination
- Reorder: all IDs must belong to the same date/shift/unit

---

## Business Rules
1. Scheduling a PWO updates its status to `Scheduled`
2. When Production marks schedule as `InProgress`, PWO status updates to `InProgress`
3. Completed schedule entries mark associated PWO as `Complete` (if all schedules for that PWO are complete)
4. PPC creates schedule; Production can update status (InProgress/Completed)
5. Week starts on Monday
6. Color-code cards by status: gray=Planned, blue=InProgress, green=Completed, red=Cancelled

---

## Files to Create

### API
| File | Purpose |
|---|---|
| `Controllers/ProductionScheduleController.cs` | CRUD + reorder |
| `Services/Planning/IProductionScheduleService.cs` + impl | Schedule logic |
| `DTOs/Planning/CreateScheduleEntryDto.cs` | Create |
| `DTOs/Planning/ScheduleEntryDto.cs` | Response |
| `DTOs/Planning/ReorderScheduleDto.cs` | Reorder |
| `Validators/Planning/CreateScheduleEntryValidator.cs` | |

### UI
| File | Purpose |
|---|---|
| `src/pages/ppc/ProductionSchedulePage.jsx` | Calendar/planner view |
| `src/components/schedule/ScheduleCard.jsx` | Draggable PWO card |
| `src/components/schedule/AssignPWODialog.jsx` | Dialog for assigning PWO |
| `src/hooks/useProductionSchedule.js` | React Query hooks |
| `src/services/productionScheduleService.js` | API calls |

## Acceptance Criteria
1. Week calendar view with Day/Night shift rows
2. Cards display PWO info with status badge
3. Drag & drop to reschedule (desktop only)
4. Reorder within a cell via drag
5. "Add" button opens PWO assignment dialog
6. Mobile: list view with day groupings, no drag-and-drop
7. Week navigation (← →) loads correct date range
8. Production unit filter shows/hides units
9. Scheduling updates PWO status to Scheduled
10. Color-coded cards by status
11. Only unscheduled PWOs appear in assignment dialog

## Reference
- **WORKFLOWS.md:** Workflow 3 — Weekly Production Schedule
- **01-database-schema.md:** ProductionSchedule entity
