import { useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import DashboardLayout from './components/layout/DashboardLayout';
import AuthLayout from './components/layout/AuthLayout';
import ProtectedRoute from './components/common/ProtectedRoute';
import GuestOnlyRoute from './components/common/GuestOnlyRoute';
import LoadingSpinner from './components/common/LoadingSpinner';
import useAuthStore from './stores/authStore';

// Lazy-loaded pages
const LoginPage = lazy(() => import('./pages/auth/LoginPage'));
const ForgotPasswordPage = lazy(() => import('./pages/auth/ForgotPasswordPage'));
const DashboardPage = lazy(() => import('./pages/dashboard/DashboardPage'));

// Masters
const CustomersPage = lazy(() => import('./pages/masters/CustomersPage'));
const CustomerFormPage = lazy(() => import('./pages/masters/CustomerFormPage'));
const SectionProfilesPage = lazy(() => import('./pages/masters/SectionProfilesPage'));
const SectionProfileFormPage = lazy(() => import('./pages/masters/SectionProfileFormPage'));
const PowderColorsPage = lazy(() => import('./pages/masters/PowderColorsPage'));
const PowderColorFormPage = lazy(() => import('./pages/masters/PowderColorFormPage'));
const VendorsPage = lazy(() => import('./pages/masters/VendorsPage'));
const VendorFormPage = lazy(() => import('./pages/masters/VendorFormPage'));
const ProcessTypesPage = lazy(() => import('./pages/masters/ProcessTypesPage'));
const ProductionUnitsPage = lazy(() => import('./pages/masters/ProductionUnitsPage'));
const UsersPage = lazy(() => import('./pages/admin/UsersPage'));
const AuditLogPage = lazy(() => import('./pages/admin/AuditLogPage'));
const NotificationsPage = lazy(() => import('./pages/notifications/NotificationsPage'));

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
const PanelTestFormPage = lazy(() => import('./pages/quality/PanelTestFormPage'));
const FinalInspectionsPage = lazy(() => import('./pages/quality/FinalInspectionsPage'));
const FinalInspectionFormPage = lazy(() => import('./pages/quality/FinalInspectionFormPage'));
const TestCertificatesPage = lazy(() => import('./pages/quality/TestCertificatesPage'));
const TestCertificateFormPage = lazy(() => import('./pages/quality/TestCertificateFormPage'));
const TestCertificatePreviewPage = lazy(() => import('./pages/quality/TestCertificatePreviewPage'));

// Dispatch
const PackingListsPage = lazy(() => import('./pages/dispatch/PackingListsPage'));
const PackingListFormPage = lazy(() => import('./pages/dispatch/PackingListFormPage'));
const DeliveryChallansPage = lazy(() => import('./pages/dispatch/DeliveryChallansPage'));
const DeliveryChallanFormPage = lazy(() => import('./pages/dispatch/DeliveryChallanFormPage'));
const InvoicesPage = lazy(() => import('./pages/dispatch/InvoicesPage'));
const InvoiceFormPage = lazy(() => import('./pages/dispatch/InvoiceFormPage'));

// Purchase
const PowderIndentsPage = lazy(() => import('./pages/purchase/PowderIndentsPage'));
const PowderIndentFormPage = lazy(() => import('./pages/purchase/PowderIndentFormPage'));
const PurchaseOrdersPage = lazy(() => import('./pages/purchase/PurchaseOrdersPage'));
const PurchaseOrderFormPage = lazy(() => import('./pages/purchase/PurchaseOrderFormPage'));
const GRNPage = lazy(() => import('./pages/purchase/GRNPage'));
const GRNFormPage = lazy(() => import('./pages/purchase/GRNFormPage'));
const PowderStockPage = lazy(() => import('./pages/purchase/PowderStockPage'));

// Order Cycle
const OrderCyclePage = lazy(() => import('./pages/OrderCyclePage'));

// Reports
const ReportsPage = lazy(() => import('./pages/reports/ReportsPage'));
const OrderTrackerPage = lazy(() => import('./pages/reports/OrderTrackerPage'));
const ProductionReportPage = lazy(() => import('./pages/reports/ProductionReportPage'));
const QualityReportPage = lazy(() => import('./pages/reports/QualityReportPage'));
const CustomerHistoryPage = lazy(() => import('./pages/reports/CustomerHistoryPage'));
const DispatchRegisterPage = lazy(() => import('./pages/reports/DispatchRegisterPage'));
const YieldReportPage = lazy(() => import('./pages/reports/YieldReportPage'));

function App() {
  useEffect(() => {
    useAuthStore.getState().initialize();
  }, []);

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
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/order-cycle" element={<OrderCyclePage />} />

            {/* Masters — Admin + Leader only */}
            <Route element={<ProtectedRoute allowedRoles={['Admin', 'Leader']} />}>
              <Route path="/masters/customers" element={<CustomersPage />} />
              <Route path="/masters/customers/new" element={<CustomerFormPage />} />
              <Route path="/masters/customers/:id/edit" element={<CustomerFormPage />} />
              <Route path="/masters/section-profiles" element={<SectionProfilesPage />} />
              <Route path="/masters/section-profiles/new" element={<SectionProfileFormPage />} />
              <Route path="/masters/section-profiles/:id/edit" element={<SectionProfileFormPage />} />
              <Route path="/masters/powder-colors" element={<PowderColorsPage />} />
              <Route path="/masters/powder-colors/new" element={<PowderColorFormPage />} />
              <Route path="/masters/powder-colors/:id/edit" element={<PowderColorFormPage />} />
              <Route path="/masters/vendors" element={<VendorsPage />} />
              <Route path="/masters/vendors/new" element={<VendorFormPage />} />
              <Route path="/masters/vendors/:id/edit" element={<VendorFormPage />} />
              <Route path="/masters/process-types" element={<ProcessTypesPage />} />
              <Route path="/masters/production-units" element={<ProductionUnitsPage />} />
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
              <Route path="/production/yield" element={<YieldReportPage />} />
            </Route>

            {/* Quality */}
            <Route element={<ProtectedRoute allowedDepartments={['QA']} />}>
              <Route path="/quality/in-process" element={<InProcessInspectionsPage />} />
              <Route path="/quality/in-process/new" element={<InProcessInspectionFormPage />} />
              <Route path="/quality/in-process/:id" element={<InProcessInspectionFormPage />} />
              <Route path="/quality/panel-tests/new" element={<PanelTestFormPage />} />
              <Route path="/quality/panel-tests/:id" element={<PanelTestFormPage />} />
              <Route path="/quality/final" element={<FinalInspectionsPage />} />
              <Route path="/quality/final/new" element={<FinalInspectionFormPage />} />
              <Route path="/quality/final/:id" element={<FinalInspectionFormPage />} />
              <Route path="/quality/test-certificates" element={<TestCertificatesPage />} />
              <Route path="/quality/test-certificates/new" element={<TestCertificateFormPage />} />
              <Route path="/quality/test-certificates/:id" element={<TestCertificateFormPage />} />
              <Route path="/quality/test-certificates/:id/preview" element={<TestCertificatePreviewPage />} />
            </Route>

            {/* Dispatch — SCM */}
            <Route element={<ProtectedRoute allowedDepartments={['SCM']} />}>
              <Route path="/dispatch/packing-lists" element={<PackingListsPage />} />
              <Route path="/dispatch/packing-lists/new" element={<PackingListFormPage />} />
              <Route path="/dispatch/packing-lists/:id" element={<PackingListFormPage />} />
              <Route path="/dispatch/delivery-challans" element={<DeliveryChallansPage />} />
              <Route path="/dispatch/delivery-challans/new" element={<DeliveryChallanFormPage />} />
              <Route path="/dispatch/delivery-challans/:id" element={<DeliveryChallanFormPage />} />
              <Route path="/dispatch/invoices" element={<InvoicesPage />} />
              <Route path="/dispatch/invoices/new" element={<InvoiceFormPage />} />
              <Route path="/dispatch/invoices/:id" element={<InvoiceFormPage />} />
            </Route>

            {/* Purchase */}
            <Route element={<ProtectedRoute allowedDepartments={['Purchase']} />}>
              <Route path="/purchase/indents" element={<PowderIndentsPage />} />
              <Route path="/purchase/indents/new" element={<PowderIndentFormPage />} />
              <Route path="/purchase/indents/:id" element={<PowderIndentFormPage />} />
              <Route path="/purchase/orders" element={<PurchaseOrdersPage />} />
              <Route path="/purchase/orders/new" element={<PurchaseOrderFormPage />} />
              <Route path="/purchase/orders/:id" element={<PurchaseOrderFormPage />} />
              <Route path="/purchase/grn" element={<GRNPage />} />
              <Route path="/purchase/grn/new" element={<GRNFormPage />} />
              <Route path="/purchase/grn/:id" element={<GRNFormPage />} />
              <Route path="/purchase/stock" element={<PowderStockPage />} />
            </Route>

            {/* Reports — Admin + Leader */}
            <Route element={<ProtectedRoute allowedRoles={['Admin', 'Leader']} />}>
              <Route path="/reports" element={<ReportsPage />} />
              <Route path="/reports/order-tracker" element={<OrderTrackerPage />} />
              <Route path="/reports/production" element={<ProductionReportPage />} />
              <Route path="/reports/quality" element={<QualityReportPage />} />
              <Route path="/reports/customer" element={<CustomerHistoryPage />} />
              <Route path="/reports/dispatch" element={<DispatchRegisterPage />} />
              <Route path="/reports/yield" element={<YieldReportPage />} />
            </Route>

            {/* Admin — Admin only */}
            <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
              <Route path="/admin/users" element={<UsersPage />} />
              <Route path="/admin/audit-logs" element={<AuditLogPage />} />
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
