import DashboardIcon from '@mui/icons-material/Dashboard';
import SettingsIcon from '@mui/icons-material/Settings';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import InventoryIcon from '@mui/icons-material/Inventory';
import EventNoteIcon from '@mui/icons-material/EventNote';
import PrecisionManufacturingIcon from '@mui/icons-material/PrecisionManufacturing';
import VerifiedIcon from '@mui/icons-material/Verified';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';
import StoreIcon from '@mui/icons-material/Store';
import BarChartIcon from '@mui/icons-material/BarChart';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import NotificationsIcon from '@mui/icons-material/Notifications';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import { ACCESS_GROUPS } from '@/constants/accessGroups';

export const navigationItems = [
  { label: 'Dashboard', icon: DashboardIcon, path: '/dashboard', departments: '*' },
  { label: 'Notifications', icon: NotificationsIcon, path: '/notifications', departments: '*' },
  { label: 'Order Cycle', icon: AccountTreeIcon, path: '/order-cycle', departments: '*' },
  { label: 'How to Use App', icon: MenuBookIcon, path: '/guide', departments: '*' },
  {
    label: 'Masters',
    icon: SettingsIcon,
    departments: ['Admin', 'Leader'],
    groups: ACCESS_GROUPS.MASTERS,
    children: [
      { label: 'Customers', path: '/masters/customers' },
      { label: 'Section Profiles', path: '/masters/section-profiles' },
      { label: 'Powder Colors', path: '/masters/powder-colors' },
      { label: 'Vendors', path: '/masters/vendors' },
      { label: 'Process Types', path: '/masters/process-types' },
    ],
  },
  {
    label: 'Sales',
    icon: ShoppingCartIcon,
    departments: ['Sales'],
    groups: ACCESS_GROUPS.SALES,
    children: [
      { label: 'Inquiries', path: '/sales/inquiries' },
      { label: 'Quotations', path: '/sales/quotations' },
      { label: 'Proforma Invoices', path: '/sales/proforma-invoices' },
      { label: 'Work Orders', path: '/sales/work-orders' },
    ],
  },
  {
    label: 'Material Inward',
    icon: InventoryIcon,
    departments: ['SCM'],
    groups: ACCESS_GROUPS.SCM,
    children: [
      { label: 'Inward Register', path: '/material-inward/inwards' },
      { label: 'Incoming Inspection', path: '/material-inward/inspections' },
    ],
  },
  {
    label: 'PPC',
    icon: EventNoteIcon,
    departments: ['PPC'],
    groups: ACCESS_GROUPS.PPC,
    children: [
      { label: 'Production Work Orders', path: '/ppc/work-orders' },
      { label: 'Schedule', path: '/ppc/schedule' },
    ],
  },
  {
    label: 'Production',
    icon: PrecisionManufacturingIcon,
    departments: ['Production'],
    groups: ACCESS_GROUPS.PRODUCTION,
    children: [
      { label: 'Pretreatment Logs', path: '/production/pretreatment' },
      { label: 'Coating Logs', path: '/production/coating' },
      { label: 'Yield ROI', path: '/production/yield' },
    ],
  },
  {
    label: 'Quality',
    icon: VerifiedIcon,
    departments: ['QA'],
    groups: ACCESS_GROUPS.QUALITY,
    children: [
      { label: 'In-Process Inspection', path: '/quality/in-process' },
      { label: 'Final Inspection', path: '/quality/final' },
      { label: 'Test Certificates', path: '/quality/test-certificates' },
    ],
  },
  {
    label: 'Dispatch',
    icon: LocalShippingIcon,
    departments: ['SCM'],
    groups: ACCESS_GROUPS.SCM,
    children: [
      { label: 'Packing Lists', path: '/dispatch/packing-lists' },
      { label: 'Delivery Challans', path: '/dispatch/delivery-challans' },
      { label: 'Invoices', path: '/dispatch/invoices' },
    ],
  },
  {
    label: 'Purchase',
    icon: StoreIcon,
    departments: ['Purchase'],
    groups: ACCESS_GROUPS.PURCHASE,
    children: [
      { label: 'Powder Indents', path: '/purchase/indents' },
      { label: 'Purchase Orders', path: '/purchase/orders' },
      { label: 'GRN', path: '/purchase/grn' },
      { label: 'Powder Stock', path: '/purchase/stock' },
    ],
  },
  {
    label: 'Reports',
    icon: BarChartIcon,
    path: '/reports',
    departments: ['Admin', 'Leader'],
    groups: ACCESS_GROUPS.REPORTS,
  },
  {
    label: 'Admin',
    icon: AdminPanelSettingsIcon,
    departments: ['Admin'],
    groups: ACCESS_GROUPS.ADMIN,
    children: [
      { label: 'Audit Logs', path: '/admin/audit-logs' },
    ],
  },
];

/**
 * Filter navigation items based on user role and department.
 * - Admin sees all items.
 * - Group-based access can grant visibility even outside department.
 * - departments: '*' is visible to everyone.
 * - Otherwise, user's role or department must be in the item's departments array.
 */
export function getFilteredNavItems(user) {
  if (!user) return [];
  const { role, department, groups = [] } = user;

  if (role === 'Admin') {
    return navigationItems;
  }

  return navigationItems.filter((item) => {
    if (item.departments === '*') return true;

    const departmentMatch = Array.isArray(item.departments) && item.departments.includes(department);
    const roleMatch = Array.isArray(item.departments) && item.departments.includes(role);
    const groupMatch = Array.isArray(item.groups) && item.groups.some((g) => groups.includes(g));

    return departmentMatch || roleMatch || groupMatch;
  });
}
