# Plan: Multi-Color per DC + Buffing Notification

## TL;DR
Two independent features:
1. Replace single `PowderColorId` on `MaterialInward` with a child collection `MaterialInwardPowderColor` so each DC can carry 2-3 colors. Requires schema migration, full backend stack update, and frontend form refactor.
2. After an `IncomingInspection` is created with any `BuffingRequired=true` line, fire notifications to the Production and Sales departments via the existing `INotificationService`.

---

## Feature 1: Multi-Color Support per DC

### Phase A — Backend (depends on nothing, can start immediately)

**Step 1 — New entity `MaterialInwardPowderColor`**
- Create `Models/MaterialInward/MaterialInwardPowderColor.cs`
- Fields: `Id`, `MaterialInwardId` (FK), `PowderColorId` (FK)
- Nav props: `MaterialInward MaterialInward`, `PowderColor PowderColor`
- No BaseEntity (lightweight child, no audit needed)

**Step 2 — Update `MaterialInward` model** (`Models/MaterialInward/MaterialInward.cs`)
- Remove `public int? PowderColorId` and `public PowderColor? PowderColor`
- Add `public ICollection<MaterialInwardPowderColor> PowderColors { get; set; } = new List<>()`

**Step 3 — AppDbContext** (`Data/AppDbContext.cs`)
- Add `DbSet<MaterialInwardPowderColor> MaterialInwardPowderColors`

**Step 4 — Update DTOs**
- `DTOs/MaterialInward/CreateMaterialInwardDto.cs`: Replace `int? PowderColorId` with `List<int> PowderColorIds = new()`
- `DTOs/MaterialInward/UpdateMaterialInwardDto.cs`: Same change
- `DTOs/MaterialInward/MaterialInwardDetailDto.cs`:
  - Remove `PowderColorId`, `PowderColorName`
  - Add `List<MaterialInwardPowderColorDto> PowderColors = new()`
- New nested DTO (same file or separate): `MaterialInwardPowderColorDto { int Id, int PowderColorId, string ColorName, string? PowderCode }`
- `MaterialInwardDto` (list view): No change needed (colors not shown in list)

**Step 5 — Update `MaterialInwardService`** (`Services/MaterialInward/MaterialInwardService.cs`)
- `CreateAsync`:
  - Remove `dto.PowderColorId.HasValue` validation block
  - After mapping entity, add loop: foreach `powderColorId` in `dto.PowderColorIds`, validate it exists in `_db.PowderColors`, then `entity.PowderColors.Add(new MaterialInwardPowderColor { PowderColorId = id })`
- `UpdateAsync`:
  - Remove `entity.PowderColorId = dto.PowderColorId`
  - Clear and re-add: `entity.PowderColors.Clear()` then add new ones from `dto.PowderColorIds`
  - Need to Include PowderColors in the tracking query
- `GetByIdAsync` (and any reload queries): Add `.Include(m => m.PowderColors).ThenInclude(c => c.PowderColor)` where detail is loaded

**Step 6 — Update `MaterialInwardMappingProfile`** (`Mappings/MaterialInwardMappingProfile.cs`)
- Add: `CreateMap<MaterialInwardPowderColor, MaterialInwardPowderColorDto>().ForMember(d => d.ColorName, opt => opt.MapFrom(s => s.PowderColor.ColorName)).ForMember(d => d.PowderCode, opt => opt.MapFrom(s => s.PowderColor.PowderCode))`
- Update `MaterialInward → MaterialInwardDetailDto` mapping: remove `.ForMember(d => d.PowderColorName, ...)` and `.ForMember(d => d.PowderColorId, ...)`, add `.ForMember(d => d.PowderColors, opt => opt.MapFrom(s => s.PowderColors))`

**Step 7 — EF Migration**
- Run: `dotnet ef migrations add AddMultiColorPerInward`
- Edit generated migration `Up()` to add custom SQL data migration BEFORE dropping the column:
  ```csharp
  migrationBuilder.Sql(@"
    INSERT INTO MaterialInwardPowderColors (MaterialInwardId, PowderColorId)
    SELECT Id, PowderColorId FROM MaterialInwards
    WHERE PowderColorId IS NOT NULL AND IsDeleted = 0
  ");
  ```
- Then scaffold will drop `PowderColorId` from `MaterialInwards`
- Verify migration has no unintended drops (`dotnet ef migrations script`)

### Phase B — Frontend (depends on Phase A backend being deployed, or can mock)

**Step 8 — Form schema & state** (`hycoat-ui/src/pages/material-inward/MaterialInwardFormPage.jsx`)
- Schema: Replace `powderColorId: z.number().nullable().optional()` with `powderColorIds: z.array(z.number().min(1)).optional().default([])`
- defaultValues: Replace `powderColorId: null` with `powderColorIds: []`
- Add `useFieldArray` for `powderColorIds` OR manage with plain `useState` (since it's just IDs, not complex objects)
- Recommendation: Use a simple controlled multi-select or an "Add Color" add-row pattern

**Step 9 — Form UI — replace single PowderColor dropdown**
- Remove: the existing single `<Controller name="powderColorId" ...>` Autocomplete
- Add a "Powder Colors" card/section (similar to the lines table section)
- Row per color: `Autocomplete` of `powderColorOptions` + `IconButton` Delete
- "Add Color" button (`<Button startIcon={<Add />}`)
- No min-required validation (colors are optional like before)

**Step 10 — WO auto-fill update**
- In `handleWOChange`: Replace `setValue('powderColorId', wo.powderColorId ?? null)` with `setValue('powderColorIds', wo.powderColorId ? [wo.powderColorId] : [])`

**Step 11 — Edit mode data load**
- In `useEffect` reset: Replace `powderColorId: existingData.powderColorId ?? null` with `powderColorIds: existingData.powderColors?.map(c => c.powderColorId) ?? []`

**Step 12 — Submit payload**
- In `onSubmit`: Remove `powderColorId: data.powderColorId || null`, add `powderColorIds: data.powderColorIds ?? []`

**Step 13 — Detail/display view** (MaterialInwardsPage list / detail chip area)
- Show colors as MUI `<Chip>` components from `powderColors` array in detail view
- List view: no change (colors not shown there currently)

---

## Feature 2: Buffing Notification to Production & Sales

**Step 14 — Inject INotificationService** (`Services/MaterialInward/IncomingInspectionService.cs`)
- Add `INotificationService _notificationService` field
- Update constructor to accept and assign `INotificationService notificationService`

**Step 15 — Fire notification in `CreateAsync`**
- After `await _db.SaveChangesAsync()` (after the status update)
- Check: `var buffingLines = dto.Lines.Where(l => l.BuffingRequired).ToList()`
- If `buffingLines.Any()`:
  - Build message: e.g. `"Inspection {inspectionNumber} for {customerName} — {buffingLines.Count} item(s) require buffing. Total charges: ₹{totalCharge:N2}."`
  - Call `await _notificationService.NotifyDepartmentAsync("Production", title, message, "warning", "IncomingInspection", entity.Id, "IncomingInspection")`
  - Call `await _notificationService.NotifyDepartmentAsync("Sales", title, message, "warning", "IncomingInspection", entity.Id, "IncomingInspection")`
- ⚠️ **Verify department string values**: Check `AppUser.Department` column values in DB or seeded data to confirm "Production" and "Sales" match exactly

**Step 16 — No additional frontend changes**
- The notification bell in the header auto-shows new notifications via SignalR `ReceiveNotification` event (already wired)

---

## Relevant Files

**Backend:**
- `hycoat-api/Models/MaterialInward/MaterialInward.cs` — remove PowderColorId, add PowderColors collection
- `hycoat-api/Models/MaterialInward/MaterialInwardPowderColor.cs` — NEW entity
- `hycoat-api/Data/AppDbContext.cs` — add DbSet
- `hycoat-api/DTOs/MaterialInward/CreateMaterialInwardDto.cs` — PowderColorIds list
- `hycoat-api/DTOs/MaterialInward/UpdateMaterialInwardDto.cs` — PowderColorIds list
- `hycoat-api/DTOs/MaterialInward/MaterialInwardDetailDto.cs` — PowderColors collection DTO
- `hycoat-api/Services/MaterialInward/MaterialInwardService.cs` — Create/Update/load logic
- `hycoat-api/Services/MaterialInward/IncomingInspectionService.cs` — inject INotificationService, fire on create
- `hycoat-api/Mappings/MaterialInwardMappingProfile.cs` — new color mapping
- `hycoat-api/Migrations/` — new migration file

**Frontend:**
- `hycoat-ui/src/pages/material-inward/MaterialInwardFormPage.jsx` — multi-color UI

---

## Verification

1. **Create new inward** with 0 colors → API accepts (colors optional)
2. **Create new inward** with 2 colors → detail DTO returns `powderColors` array with both entries
3. **Edit existing inward** with 1 color → form pre-populates with that color; add a second → saved correctly
4. **WO auto-fill** → populates first color from WO's powderColorId
5. **Migration** → existing inwards that had `PowderColorId` now have a row in `MaterialInwardPowderColors`; no data loss
6. **Create inspection** with no buffing → no notification sent; with buffing on any line → both Production and Sales users receive notification via SignalR + DB
7. **Notification content** includes inspection number, customer name, item count

---

## Decisions
- Colors at **header-level** (one list for the whole DC), not per line — matches client description
- Notification fires on **Create only** (not Update)
- `MaterialInwardPowderColor` is a lightweight child (no BaseEntity audit fields)
- Existing single-color data migrated via custom SQL in EF migration `Up()`
