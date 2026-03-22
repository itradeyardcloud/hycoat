# 00-foundation/05-app-shell-layout

## Feature ID
`00-foundation/05-app-shell-layout`

## Feature Name
Application Shell & Layout with Route Guards

## Dependencies
- `00-foundation/03-ui-restructuring` — React Router, MUI, stores, theme
- `00-foundation/04-pwa-setup` — PWA manifest (for standalone display)
- `00-foundation/02-auth-system` — Auth endpoints, roles, JWT (for guard logic)

## Business Context
HyCoat uses 3 user tiers: Admin (full access), Leader (department overseers), and Department User (own module only). The app must be responsive — desktop users see a persistent sidebar; mobile/tablet users see a bottom navigation bar. Route protection prevents unauthorized access to modules. Breadcrumb navigation helps users orient within deep page hierarchies.

---

## Tasks

### 1. Create Layout Components

#### `src/components/layout/DashboardLayout.jsx`
Main authenticated layout wrapping all protected pages.
```
┌──────────────────────────────────────────────────┐
│  AppBar (64px)   │ Title │ Notifications │ Avatar │
├──────────────────┼───────────────────────────────┤
│  Sidebar (240px) │  Main Content Area             │
│  ─────────────── │  <Outlet />                    │
│  Dashboard       │                                │
│  Masters ▸       │                                │
│  Sales    ▸      │                                │
│  Material ▸      │                                │
│  PPC      ▸      │                                │
│  Production ▸    │                                │
│  Quality  ▸      │                                │
│  Dispatch ▸      │                                │
│  Purchase        │                                │
│  Reports         │                                │
│  Admin    ▸      │                                │
└──────────────────┴───────────────────────────────┘

Mobile (< 768px):
┌──────────────────────────────────────────────────┐
│  AppBar │ ≡ Menu │ Title │ 🔔 │ Avatar           │
├──────────────────────────────────────────────────┤
│                                                  │
│  Main Content Area (full width)                  │
│  <Outlet />                                      │
│                                                  │
├──────────────────────────────────────────────────┤
│  BottomNav: Home │ Orders │ Prod │ QA │ More     │
└──────────────────────────────────────────────────┘
```

**Properties/Behavior:**
- `useMediaQuery('(min-width:768px)')` → switch between sidebar and bottom nav
- Sidebar: Collapsible (toggle via hamburger or `uiStore.toggleSidebar()`)
- Sidebar width: 240px expanded, 64px collapsed (icon-only)
- MUI `Drawer` (permanent on desktop, temporary on mobile)
- MUI `AppBar` with:
  - Hamburger menu (mobile) / sidebar toggle (desktop)
  - Page title (from route or breadcrumb)
  - Notification bell icon with unread badge
  - User avatar + dropdown (Profile, Logout)
- MUI `BottomNavigation` (mobile only, 5 items)
- `<Outlet />` from react-router-dom renders child routes

#### `src/components/layout/AuthLayout.jsx`
Simple centered layout for login/forgot-password pages:
```
┌──────────────────────────────────────────────────┐
│                                                  │
│            ┌──────────────────┐                  │
│            │  HyCoat Logo     │                  │
│            │  ─────────────── │                  │
│            │  [Login Form]    │                  │
│            │                  │                  │
│            └──────────────────┘                  │
│                                                  │
│   Background: subtle pattern or gradient         │
└──────────────────────────────────────────────────┘
```
- Centered `Card` with max-width 420px
- HyCoat branding at top
- `<Outlet />` for auth sub-routes

#### `src/components/layout/Sidebar.jsx`
Navigation items with nested sub-items per module. Uses MUI `List`, `ListItem`, `Collapse`.

#### `src/components/layout/BottomNav.jsx`
MUI `BottomNavigation` with 5 items, varies by user role/department:
- **Admin/Leader:** Dashboard, Orders, Production, Quality, More
- **Sales:** Dashboard, Inquiries, Work Orders, Quotations, More
- **Production:** Dashboard, Schedule, Pretreatment, Coating, More
- **QA:** Dashboard, Inspections, Tests, Certificates, More

#### `src/components/layout/Breadcrumbs.jsx`
Uses MUI `Breadcrumbs`. Auto-generates from route path:
- `/sales/inquiries/INQ-2024-001` → Home > Sales > Inquiries > INQ-2024-001

---

### 2. Create Route Guards

#### `src/components/common/ProtectedRoute.jsx`
```jsx
// Props: allowedRoles?: string[], allowedDepartments?: string[]
// Logic:
// 1. Check authStore.isAuthenticated → redirect to /login if false
// 2. Check allowedRoles (if specified) → show 403 if user.role not in list
// 3. Check allowedDepartments (if specified) → show 403 if user.department not in list
// 4. Admin bypasses department check (Admin sees everything)
// 5. Leader sees their department + overview of others
// 6. Render <Outlet /> if all checks pass
```

#### `src/components/common/GuestOnlyRoute.jsx`
```jsx
// Redirects to /dashboard if already authenticated
// Used for login page only
```

---

### 3. Define Complete Route Structure in `src/App.jsx`

```jsx
import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import DashboardLayout from './components/layout/DashboardLayout';
import AuthLayout from './components/layout/AuthLayout';
import ProtectedRoute from './components/common/ProtectedRoute';
import GuestOnlyRoute from './components/common/GuestOnlyRoute';
import LoadingSpinner from './components/common/LoadingSpinner';

// Lazy-loaded pages
const LoginPage = lazy(() => import('./pages/auth/LoginPage'));
const ForgotPasswordPage = lazy(() => import('./pages/auth/ForgotPasswordPage'));

const DashboardPage = lazy(() => import('./pages/dashboard/DashboardPage'));

// Masters
const CustomersPage = lazy(() => import('./pages/masters/CustomersPage'));
const CustomerFormPage = lazy(() => import('./pages/masters/CustomerFormPage'));
const SectionProfilesPage = lazy(() => import('./pages/masters/SectionProfilesPage'));
const PowderColorsPage = lazy(() => import('./pages/masters/PowderColorsPage'));
const VendorsPage = lazy(() => import('./pages/masters/VendorsPage'));
const ProcessTypesPage = lazy(() => import('./pages/masters/ProcessTypesPage'));
const UsersPage = lazy(() => import('./pages/admin/UsersPage'));

// Sales
const InquiriesPage = lazy(() => import('./pages/sales/InquiriesPage'));
const InquiryFormPage = lazy(() => import('./pages/sales/InquiryFormPage'));
const QuotationsPage = lazy(() => import('./pages/sales/QuotationsPage'));
const QuotationFormPage = lazy(() => import('./pages/sales/QuotationFormPage'));
const PIListPage = lazy(() => import('./pages/sales/PIListPage'));
const PIFormPage = lazy(() => import('./pages/sales/PIFormPage'));
const WorkOrdersPage = lazy(() => import('./pages/sales/WorkOrdersPage'));
const WorkOrderFormPage = lazy(() => import('./pages/sales/WorkOrderFormPage'));
const WorkOrderDetailPage = lazy(() => import('./pages/sales/WorkOrderDetailPage'));

// Material Inward
const MaterialInwardsPage = lazy(() => import('./pages/material-inward/MaterialInwardsPage'));
const MaterialInwardFormPage = lazy(() => import('./pages/material-inward/MaterialInwardFormPage'));
const IncomingInspectionsPage = lazy(() => import('./pages/material-inward/IncomingInspectionsPage'));
const IncomingInspectionFormPage = lazy(() => import('./pages/material-inward/IncomingInspectionFormPage'));

// PPC
const ProductionWorkOrdersPage = lazy(() => import('./pages/ppc/ProductionWorkOrdersPage'));
const PWOFormPage = lazy(() => import('./pages/ppc/PWOFormPage'));
const ProductionSchedulePage = lazy(() => import('./pages/ppc/ProductionSchedulePage'));

// Production
const PretreatmentLogsPage = lazy(() => import('./pages/production/PretreatmentLogsPage'));
const PretreatmentLogFormPage = lazy(() => import('./pages/production/PretreatmentLogFormPage'));
const CoatingLogsPage = lazy(() => import('./pages/production/CoatingLogsPage'));
const CoatingLogFormPage = lazy(() => import('./pages/production/CoatingLogFormPage'));

// Quality
const InProcessInspectionsPage = lazy(() => import('./pages/quality/InProcessInspectionsPage'));
const InProcessInspectionFormPage = lazy(() => import('./pages/quality/InProcessInspectionFormPage'));
const FinalInspectionsPage = lazy(() => import('./pages/quality/FinalInspectionsPage'));
const FinalInspectionFormPage = lazy(() => import('./pages/quality/FinalInspectionFormPage'));
const TestCertificatesPage = lazy(() => import('./pages/quality/TestCertificatesPage'));

// Dispatch
const PackingListsPage = lazy(() => import('./pages/dispatch/PackingListsPage'));
const PackingListFormPage = lazy(() => import('./pages/dispatch/PackingListFormPage'));
const DeliveryChallansPage = lazy(() => import('./pages/dispatch/DeliveryChallansPage'));
const InvoicesPage = lazy(() => import('./pages/dispatch/InvoicesPage'));
const InvoiceFormPage = lazy(() => import('./pages/dispatch/InvoiceFormPage'));

// Purchase
const PowderIndentsPage = lazy(() => import('./pages/purchase/PowderIndentsPage'));
const PurchaseOrdersPage = lazy(() => import('./pages/purchase/PurchaseOrdersPage'));
const GRNPage = lazy(() => import('./pages/purchase/GRNPage'));
const PowderStockPage = lazy(() => import('./pages/purchase/PowderStockPage'));

// Reports
const ReportsPage = lazy(() => import('./pages/reports/ReportsPage'));

function App() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <Routes>
        {/* Auth Routes */}
        <Route element={<GuestOnlyRoute />}>
          <Route element={<AuthLayout />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          </Route>
        </Route>

        {/* Protected Routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<DashboardLayout />}>
            <Route path="/dashboard" element={<DashboardPage />} />

            {/* Masters — Admin + Leader only */}
            <Route element={<ProtectedRoute allowedRoles={['Admin', 'Leader']} />}>
              <Route path="/masters/customers" element={<CustomersPage />} />
              <Route path="/masters/customers/new" element={<CustomerFormPage />} />
              <Route path="/masters/customers/:id/edit" element={<CustomerFormPage />} />
              <Route path="/masters/section-profiles" element={<SectionProfilesPage />} />
              <Route path="/masters/powder-colors" element={<PowderColorsPage />} />
              <Route path="/masters/vendors" element={<VendorsPage />} />
              <Route path="/masters/process-types" element={<ProcessTypesPage />} />
            </Route>

            {/* Sales */}
            <Route element={<ProtectedRoute allowedDepartments={['Sales']} />}>
              <Route path="/sales/inquiries" element={<InquiriesPage />} />
              <Route path="/sales/inquiries/new" element={<InquiryFormPage />} />
              <Route path="/sales/inquiries/:id" element={<InquiryFormPage />} />
              <Route path="/sales/quotations" element={<QuotationsPage />} />
              <Route path="/sales/quotations/new" element={<QuotationFormPage />} />
              <Route path="/sales/quotations/:id" element={<QuotationFormPage />} />
              <Route path="/sales/proforma-invoices" element={<PIListPage />} />
              <Route path="/sales/proforma-invoices/new" element={<PIFormPage />} />
              <Route path="/sales/proforma-invoices/:id" element={<PIFormPage />} />
              <Route path="/sales/work-orders" element={<WorkOrdersPage />} />
              <Route path="/sales/work-orders/new" element={<WorkOrderFormPage />} />
              <Route path="/sales/work-orders/:id" element={<WorkOrderDetailPage />} />
              <Route path="/sales/work-orders/:id/edit" element={<WorkOrderFormPage />} />
            </Route>

            {/* Material Inward — SCM */}
            <Route element={<ProtectedRoute allowedDepartments={['SCM']} />}>
              <Route path="/material-inward/inwards" element={<MaterialInwardsPage />} />
              <Route path="/material-inward/inwards/new" element={<MaterialInwardFormPage />} />
              <Route path="/material-inward/inwards/:id" element={<MaterialInwardFormPage />} />
              <Route path="/material-inward/inspections" element={<IncomingInspectionsPage />} />
              <Route path="/material-inward/inspections/new" element={<IncomingInspectionFormPage />} />
              <Route path="/material-inward/inspections/:id" element={<IncomingInspectionFormPage />} />
            </Route>

            {/* PPC */}
            <Route element={<ProtectedRoute allowedDepartments={['PPC']} />}>
              <Route path="/ppc/work-orders" element={<ProductionWorkOrdersPage />} />
              <Route path="/ppc/work-orders/new" element={<PWOFormPage />} />
              <Route path="/ppc/work-orders/:id" element={<PWOFormPage />} />
              <Route path="/ppc/schedule" element={<ProductionSchedulePage />} />
            </Route>

            {/* Production */}
            <Route element={<ProtectedRoute allowedDepartments={['Production']} />}>
              <Route path="/production/pretreatment" element={<PretreatmentLogsPage />} />
              <Route path="/production/pretreatment/new" element={<PretreatmentLogFormPage />} />
              <Route path="/production/pretreatment/:id" element={<PretreatmentLogFormPage />} />
              <Route path="/production/coating" element={<CoatingLogsPage />} />
              <Route path="/production/coating/new" element={<CoatingLogFormPage />} />
              <Route path="/production/coating/:id" element={<CoatingLogFormPage />} />
            </Route>

            {/* Quality */}
            <Route element={<ProtectedRoute allowedDepartments={['QA']} />}>
              <Route path="/quality/in-process" element={<InProcessInspectionsPage />} />
              <Route path="/quality/in-process/new" element={<InProcessInspectionFormPage />} />
              <Route path="/quality/in-process/:id" element={<InProcessInspectionFormPage />} />
              <Route path="/quality/final" element={<FinalInspectionsPage />} />
              <Route path="/quality/final/new" element={<FinalInspectionFormPage />} />
              <Route path="/quality/final/:id" element={<FinalInspectionFormPage />} />
              <Route path="/quality/test-certificates" element={<TestCertificatesPage />} />
            </Route>

            {/* Dispatch — SCM */}
            <Route element={<ProtectedRoute allowedDepartments={['SCM']} />}>
              <Route path="/dispatch/packing-lists" element={<PackingListsPage />} />
              <Route path="/dispatch/packing-lists/new" element={<PackingListFormPage />} />
              <Route path="/dispatch/delivery-challans" element={<DeliveryChallansPage />} />
              <Route path="/dispatch/invoices" element={<InvoicesPage />} />
              <Route path="/dispatch/invoices/new" element={<InvoiceFormPage />} />
              <Route path="/dispatch/invoices/:id" element={<InvoiceFormPage />} />
            </Route>

            {/* Purchase */}
            <Route element={<ProtectedRoute allowedDepartments={['Purchase']} />}>
              <Route path="/purchase/indents" element={<PowderIndentsPage />} />
              <Route path="/purchase/orders" element={<PurchaseOrdersPage />} />
              <Route path="/purchase/grn" element={<GRNPage />} />
              <Route path="/purchase/stock" element={<PowderStockPage />} />
            </Route>

            {/* Reports — Admin + Leader */}
            <Route element={<ProtectedRoute allowedRoles={['Admin', 'Leader']} />}>
              <Route path="/reports" element={<ReportsPage />} />
            </Route>

            {/* Admin — Admin only */}
            <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
              <Route path="/admin/users" element={<UsersPage />} />
            </Route>
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </Suspense>
  );
}

export default App;
```

### 4. Create Shared UI Components

#### `src/components/common/LoadingSpinner.jsx`
Full-page centered MUI `CircularProgress` — used for Suspense fallback.

#### `src/components/common/PageHeader.jsx`
```jsx
// Props: title, subtitle?, action? (ReactNode for button)
// Renders: <Typography variant="h5">, optional subtitle, right-aligned action button
// Used at top of every list/detail page
```

#### `src/components/common/ConfirmDialog.jsx`
```jsx
// Props: open, title, message, onConfirm, onCancel
// MUI Dialog for delete/cancel confirmations
```

#### `src/components/common/StatusChip.jsx`
```jsx
// Props: status (string), size? ('small'|'medium')
// Maps status strings to MUI Chip colors:
//   New/Created/Draft → default (gray)
//   InProgress/Scheduled → info (blue)
//   Complete/Approved/Pass → success (green)
//   Rejected/Fail/Cancelled → error (red)
//   Pending/Awaited → warning (orange)
```

#### `src/components/common/EmptyState.jsx`
```jsx
// Props: icon?, title, description, action? (ReactNode)
// Centered message when a list is empty
```

#### `src/components/common/DataTable.jsx`
```jsx
// Props: columns, rows, loading, onRowClick?, pagination props
// Wrapper around MUI Table with:
//   - Sortable column headers
//   - Loading skeleton
//   - Empty state
//   - Pagination (MUI TablePagination)
//   - On mobile (< 768px): render as card list instead of table
```

---

### 5. Sidebar Navigation Items

Navigation structure (defined as config array):

```javascript
export const navigationItems = [
  { label: 'Dashboard', icon: DashboardIcon, path: '/dashboard', departments: '*' },
  {
    label: 'Masters', icon: SettingsIcon, departments: ['Admin', 'Leader'],
    children: [
      { label: 'Customers', path: '/masters/customers' },
      { label: 'Section Profiles', path: '/masters/section-profiles' },
      { label: 'Powder Colors', path: '/masters/powder-colors' },
      { label: 'Vendors', path: '/masters/vendors' },
      { label: 'Process Types', path: '/masters/process-types' },
    ],
  },
  {
    label: 'Sales', icon: ShoppingCartIcon, departments: ['Sales'],
    children: [
      { label: 'Inquiries', path: '/sales/inquiries' },
      { label: 'Quotations', path: '/sales/quotations' },
      { label: 'Proforma Invoices', path: '/sales/proforma-invoices' },
      { label: 'Work Orders', path: '/sales/work-orders' },
    ],
  },
  {
    label: 'Material Inward', icon: InventoryIcon, departments: ['SCM'],
    children: [
      { label: 'Inward Register', path: '/material-inward/inwards' },
      { label: 'Incoming Inspection', path: '/material-inward/inspections' },
    ],
  },
  {
    label: 'PPC', icon: EventNoteIcon, departments: ['PPC'],
    children: [
      { label: 'Production Work Orders', path: '/ppc/work-orders' },
      { label: 'Schedule', path: '/ppc/schedule' },
    ],
  },
  {
    label: 'Production', icon: PrecisionManufacturingIcon, departments: ['Production'],
    children: [
      { label: 'Pretreatment Logs', path: '/production/pretreatment' },
      { label: 'Coating Logs', path: '/production/coating' },
    ],
  },
  {
    label: 'Quality', icon: VerifiedIcon, departments: ['QA'],
    children: [
      { label: 'In-Process Inspection', path: '/quality/in-process' },
      { label: 'Final Inspection', path: '/quality/final' },
      { label: 'Test Certificates', path: '/quality/test-certificates' },
    ],
  },
  {
    label: 'Dispatch', icon: LocalShippingIcon, departments: ['SCM'],
    children: [
      { label: 'Packing Lists', path: '/dispatch/packing-lists' },
      { label: 'Delivery Challans', path: '/dispatch/delivery-challans' },
      { label: 'Invoices', path: '/dispatch/invoices' },
    ],
  },
  {
    label: 'Purchase', icon: StoreIcon, departments: ['Purchase'],
    children: [
      { label: 'Powder Indents', path: '/purchase/indents' },
      { label: 'Purchase Orders', path: '/purchase/orders' },
      { label: 'GRN', path: '/purchase/grn' },
      { label: 'Powder Stock', path: '/purchase/stock' },
    ],
  },
  { label: 'Reports', icon: BarChartIcon, path: '/reports', departments: ['Admin', 'Leader'] },
  {
    label: 'Admin', icon: AdminPanelSettingsIcon, departments: ['Admin'],
    children: [
      { label: 'User Management', path: '/admin/users' },
    ],
  },
];
```

**Filtering logic:**
- `departments: '*'` → visible to all users
- `departments: ['Sales']` → visible to Sales department users + Admin + Leader
- Admin and Leader always see all nav items

---

## Files to Create
| File | Purpose |
|---|---|
| `src/components/layout/DashboardLayout.jsx` | Main authenticated layout |
| `src/components/layout/AuthLayout.jsx` | Login/auth page layout |
| `src/components/layout/Sidebar.jsx` | Desktop sidebar navigation |
| `src/components/layout/BottomNav.jsx` | Mobile bottom navigation |
| `src/components/layout/Breadcrumbs.jsx` | Auto breadcrumb from route |
| `src/components/common/ProtectedRoute.jsx` | Auth + role/dept route guard |
| `src/components/common/GuestOnlyRoute.jsx` | Redirect auth'd users away from login |
| `src/components/common/LoadingSpinner.jsx` | Full-page spinner |
| `src/components/common/PageHeader.jsx` | Reusable page title |
| `src/components/common/ConfirmDialog.jsx` | Delete/action confirmation |
| `src/components/common/StatusChip.jsx` | Colored status badges |
| `src/components/common/EmptyState.jsx` | Empty list placeholder |
| `src/components/common/DataTable.jsx` | Responsive data table / card list |
| `src/utils/navigation.js` | Navigation config array |

## Files to Modify
| File | Changes |
|---|---|
| `src/App.jsx` | Full route structure with lazy imports + guards |

## Mobile Responsiveness Rules
- Breakpoint: 768px (MUI `md`)
- **≥ 768px:** Sidebar (permanent drawer) + AppBar
- **< 768px:** Temporary drawer (hamburger) + AppBar + BottomNavigation
- Data tables: Switch to card layout on mobile
- Forms: Single-column on mobile, 2-column on desktop
- Bottom nav: 5 tabs, context-aware based on department

## Acceptance Criteria
1. Authenticated users see `DashboardLayout` with sidebar (desktop) or bottom nav (mobile)
2. Unauthenticated users redirected to `/login`
3. `/login` page uses `AuthLayout` (centered card)
4. Already-authenticated users redirected away from `/login` to `/dashboard`
5. Admin sees all sidebar navigation items
6. Sales department user sees only Dashboard + Sales + shared items
7. Production user sees Dashboard + Production + shared items
8. Attempting to navigate to `/admin/users` as a non-Admin shows 403 or redirects
9. Sidebar collapses/expands on desktop; hamburger opens temporary drawer on mobile
10. Bottom navigation shows on mobile screens only
11. Breadcrumbs render correctly for nested routes
12. `DataTable` renders as table on desktop, card list on mobile
13. Lazy-loaded pages show `LoadingSpinner` while loading
14. No console errors on any route transition

## Reference
- **README.md (requirements):** "Roles & Access" section
- **02-auth-system.md:** Role and department definitions
- **03-ui-restructuring.md:** MUI theme, stores, project structure
