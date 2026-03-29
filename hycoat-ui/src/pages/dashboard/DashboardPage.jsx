import useAuthStore from '@/stores/authStore';
import AdminDashboardPage from './AdminDashboardPage';
import SalesDashboardPage from './SalesDashboardPage';
import PPCDashboardPage from './PPCDashboardPage';
import ProductionDashboardPage from './ProductionDashboardPage';
import QualityDashboardPage from './QualityDashboardPage';
import SCMDashboardPage from './SCMDashboardPage';
import PurchaseDashboardPage from './PurchaseDashboardPage';
import FinanceDashboardPage from './FinanceDashboardPage';
import { Alert, Box } from '@mui/material';

const DEPARTMENT_DASHBOARDS = {
  Sales: SalesDashboardPage,
  PPC: PPCDashboardPage,
  Production: ProductionDashboardPage,
  QA: QualityDashboardPage,
  SCM: SCMDashboardPage,
  Purchase: PurchaseDashboardPage,
  Finance: FinanceDashboardPage,
};

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);

  if (user?.role === 'Admin' || user?.role === 'Leader') {
    return <AdminDashboardPage />;
  }

  const DeptDashboard = DEPARTMENT_DASHBOARDS[user?.department];
  if (DeptDashboard) return <DeptDashboard />;

  return (
    <Box sx={{ p: 2 }}>
      <Alert severity="warning">Your account does not have a dashboard assigned. Contact an administrator.</Alert>
    </Box>
  );
}
