# 01-masters/01-master-data-ui

## Feature ID
`01-masters/01-master-data-ui`

## Feature Name
Master Data — UI (Data Tables, Forms, Search)

## Dependencies
- `00-foundation/03-ui-restructuring` — React project structure, MUI, TanStack Query, Zustand, axios
- `00-foundation/05-app-shell-layout` — DashboardLayout, DataTable, PageHeader, ProtectedRoute, routes
- `01-masters/00-master-data-api` — API endpoints to consume

## Business Context
Admin and Leader users manage master data that is referenced throughout the order lifecycle. Each master entity needs a list page with search/pagination and a create/edit form. Mobile users (Leaders on factory floor) need card-based layouts instead of tables.

---

## Pages & Routes

| Route | Page | Description |
|---|---|---|
| `/masters/customers` | CustomersPage | Searchable list, paginated |
| `/masters/customers/new` | CustomerFormPage | Create form |
| `/masters/customers/:id/edit` | CustomerFormPage | Edit form (loads existing data) |
| `/masters/section-profiles` | SectionProfilesPage | List with drawing preview |
| `/masters/section-profiles/new` | SectionProfileFormPage | Create + drawing upload |
| `/masters/section-profiles/:id/edit` | SectionProfileFormPage | Edit + replace drawing |
| `/masters/powder-colors` | PowderColorsPage | List with vendor filter |
| `/masters/powder-colors/new` | PowderColorFormPage | Create |
| `/masters/powder-colors/:id/edit` | PowderColorFormPage | Edit |
| `/masters/vendors` | VendorsPage | List with type filter |
| `/masters/vendors/new` | VendorFormPage | Create |
| `/masters/vendors/:id/edit` | VendorFormPage | Edit |
| `/masters/process-types` | ProcessTypesPage | Simple list |
| `/masters/production-units` | ProductionUnitsPage | List with dimensions |

---

## Customer Pages

### CustomersPage
```
┌────────────────────────────────────────────────────┐
│ PageHeader: "Customers"          [+ Add Customer]  │
├────────────────────────────────────────────────────┤
│ Search: [___________________________] 🔍           │
├────────────────────────────────────────────────────┤
│ DataTable:                                         │
│ Name       │ City    │ GSTIN       │ Contact │ ⋮   │
│ ───────────┼─────────┼─────────────┼─────────┼──── │
│ Ajit Coat  │ Pune    │ 27AABCA... │ Ravi    │ ✏🗑 │
│ XYZ Extru  │ Mumbai  │ 27BBBCB... │ Sunil   │ ✏🗑 │
│ ...        │         │             │         │     │
├────────────────────────────────────────────────────┤
│ Showing 1-20 of 45     < 1 2 3 >                  │
└────────────────────────────────────────────────────┘

Mobile (card view):
┌─────────────────────────┐
│ Ajit Coatings           │
│ 📍 Pune                 │
│ 📞 Ravi — 98205xxxxx   │
│ GST: 27AABCA...        │
│              [Edit] [⋮] │
└─────────────────────────┘
```

**Features:**
- Search field → debounced (300ms) → calls `GET /api/customers?search=...`
- Click row → navigate to edit form
- ✏ icon → edit, 🗑 icon → ConfirmDialog → soft delete
- Pagination: page size selector (10, 20, 50)
- Sort by clicking column header

### CustomerFormPage
```
┌────────────────────────────────────────────────────┐
│ PageHeader: "Add Customer" / "Edit Customer"       │
├────────────────────────────────────────────────────┤
│ Company Name*: [________________________]          │
│ Short Name:    [__________]                        │
├────────────────────────────────────────────────────┤
│ Address:       [________________________]          │
│ City:          [__________]  State: [__________]   │
│ Pincode:       [______]                            │
├────────────────────────────────────────────────────┤
│ GSTIN:         [_______________]                   │
├────────────────────────────────────────────────────┤
│ Contact Person: [_______________]                  │
│ Phone:          [_______________]                  │
│ Email:          [_______________]                  │
├────────────────────────────────────────────────────┤
│ Notes:         [________________________]          │
│                [________________________]          │
├────────────────────────────────────────────────────┤
│            [Cancel]        [Save Customer]         │
└────────────────────────────────────────────────────┘
```

**Validation (react-hook-form + zod):**
- Name: required, max 300
- GSTIN: optional, must match Indian format regex
- Email: optional, must be valid email
- Pincode: optional, must be 6 digits

**Behavior:**
- Edit mode: `useParams().id` → fetch `GET /api/customers/{id}` → populate form
- Save → `POST /api/customers` or `PUT /api/customers/{id}`
- On success: toast "Customer saved" → navigate back to list
- On error: show validation errors inline

---

## Section Profile Pages

### SectionProfilesPage
Same pattern as Customers. Columns: Section Number, Type, Perimeter (mm), Weight/m, Drawing (icon).

### SectionProfileFormPage
Same form pattern. Additional field:
- **Drawing Upload:** File input (accept image/pdf, max 10MB). Show preview thumbnail if image. Upload calls `POST /api/section-profiles/{id}/upload-drawing`.

---

## Powder Color Pages

### PowderColorsPage
Columns: Powder Code, Color Name, RAL Code, Make, Vendor Name.
Filter: Vendor dropdown (from `/api/vendors/lookup`).

### PowderColorFormPage
Fields: PowderCode*, ColorName*, RALCode, Make, VendorId (MUI Autocomplete from vendor lookup), WarrantyYears, Notes.

---

## Vendor Pages

### VendorsPage
Columns: Name, Type (chip/badge), City, Contact, Phone.
Filter: VendorType dropdown (Powder, Chemical, Consumable, Other).

### VendorFormPage
Fields: Name*, VendorType* (Select), Address, City, State, GSTIN, ContactPerson, Phone, Email.

---

## Process Type & Production Unit Pages

### ProcessTypesPage
Simple table. Columns: Name, Default Rate/SFT, Description. No separate form page — use inline dialog or small form on same page.

### ProductionUnitsPage
Table with unit name and key dimensions. Small dataset — no pagination needed.

---

## React Query Hooks

Create one hook file per entity in `src/hooks/`:

```javascript
// src/hooks/useCustomers.js
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/services/api';

export const useCustomers = (params) =>
  useQuery({
    queryKey: ['customers', params],
    queryFn: () => api.get('/customers', { params }).then(r => r.data),
  });

export const useCustomer = (id) =>
  useQuery({
    queryKey: ['customers', id],
    queryFn: () => api.get(`/customers/${id}`).then(r => r.data),
    enabled: !!id,
  });

export const useCustomerLookup = () =>
  useQuery({
    queryKey: ['customers', 'lookup'],
    queryFn: () => api.get('/customers/lookup').then(r => r.data),
    staleTime: 10 * 60 * 1000, // 10 min — lookups rarely change
  });

export const useCreateCustomer = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => api.post('/customers', data).then(r => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
};

export const useUpdateCustomer = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => api.put(`/customers/${id}`, data).then(r => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
};

export const useDeleteCustomer = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => api.delete(`/customers/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
};
```

Same pattern for `useSectionProfiles.js`, `usePowderColors.js`, `useVendors.js`, `useProcessTypes.js`, `useProductionUnits.js`.

---

## Service Functions

```javascript
// src/services/masterService.js
import api from './api';

export const customerService = {
  getAll: (params) => api.get('/customers', { params }),
  getById: (id) => api.get(`/customers/${id}`),
  create: (data) => api.post('/customers', data),
  update: (id, data) => api.put(`/customers/${id}`, data),
  delete: (id) => api.delete(`/customers/${id}`),
  lookup: () => api.get('/customers/lookup'),
};

// Same pattern for sectionProfileService, powderColorService, vendorService, processTypeService, productionUnitService
```

---

## Files to Create
| File | Purpose |
|---|---|
| `src/pages/masters/CustomersPage.jsx` | Customer list |
| `src/pages/masters/CustomerFormPage.jsx` | Create/edit customer |
| `src/pages/masters/SectionProfilesPage.jsx` | Section profile list |
| `src/pages/masters/SectionProfileFormPage.jsx` | Create/edit + drawing |
| `src/pages/masters/PowderColorsPage.jsx` | Powder color list |
| `src/pages/masters/PowderColorFormPage.jsx` | Create/edit |
| `src/pages/masters/VendorsPage.jsx` | Vendor list |
| `src/pages/masters/VendorFormPage.jsx` | Create/edit |
| `src/pages/masters/ProcessTypesPage.jsx` | Process type list (inline edit) |
| `src/pages/masters/ProductionUnitsPage.jsx` | Production unit list |
| `src/hooks/useCustomers.js` | React Query hooks |
| `src/hooks/useSectionProfiles.js` | |
| `src/hooks/usePowderColors.js` | |
| `src/hooks/useVendors.js` | |
| `src/hooks/useProcessTypes.js` | |
| `src/hooks/useProductionUnits.js` | |
| `src/services/masterService.js` | API service layer |

## Acceptance Criteria
1. Each master entity has a list page with search, pagination, and sorting
2. Each master entity has create/edit forms with client-side validation (zod)
3. Customer form validates GSTIN format (Indian 15-char)
4. Section profile form allows drawing file upload with preview
5. Powder colors page has vendor dropdown filter
6. Vendors page has type dropdown filter
7. List pages render as cards on mobile (< 768px)
8. Delete shows confirmation dialog before executing
9. Success/error toast notifications on all mutations
10. Lookup hooks use 10-minute stale time (shared across app)
11. Empty states show meaningful message + "Add" CTA button
12. Loading states show skeleton/spinner

## Reference
- **00-master-data-api.md:** Endpoints, DTOs, validation rules
- **05-app-shell-layout.md:** DataTable, PageHeader, route structure
- **WORKFLOWS.md:** Module 1 — Masters
