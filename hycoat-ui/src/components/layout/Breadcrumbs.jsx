import { Link as RouterLink, useLocation } from 'react-router-dom';
import { Breadcrumbs as MuiBreadcrumbs, Link, Typography } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';

const labelMap = {
  dashboard: 'Dashboard',
  masters: 'Masters',
  customers: 'Customers',
  'section-profiles': 'Section Profiles',
  'powder-colors': 'Powder Colors',
  vendors: 'Vendors',
  'process-types': 'Process Types',
  sales: 'Sales',
  inquiries: 'Inquiries',
  quotations: 'Quotations',
  'proforma-invoices': 'Proforma Invoices',
  'work-orders': 'Work Orders',
  'material-inward': 'Material Inward',
  inwards: 'Inward Register',
  inspections: 'Inspections',
  ppc: 'PPC',
  schedule: 'Schedule',
  production: 'Production',
  pretreatment: 'Pretreatment',
  coating: 'Coating',
  quality: 'Quality',
  'in-process': 'In-Process',
  final: 'Final',
  'test-certificates': 'Test Certificates',
  dispatch: 'Dispatch',
  'packing-lists': 'Packing Lists',
  'delivery-challans': 'Delivery Challans',
  invoices: 'Invoices',
  purchase: 'Purchase',
  indents: 'Powder Indents',
  orders: 'Purchase Orders',
  grn: 'GRN',
  stock: 'Powder Stock',
  reports: 'Reports',
  admin: 'Admin',
  users: 'Users',
  new: 'New',
  edit: 'Edit',
};

function formatSegment(segment) {
  return labelMap[segment] || segment.replace(/-/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase());
}

export default function Breadcrumbs() {
  const location = useLocation();
  const pathSegments = location.pathname.split('/').filter(Boolean);

  if (pathSegments.length <= 1) return null;

  return (
    <MuiBreadcrumbs sx={{ mb: 2 }} aria-label="breadcrumb">
      <Link
        component={RouterLink}
        to="/dashboard"
        color="inherit"
        underline="hover"
        sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}
      >
        <HomeIcon fontSize="small" />
        Home
      </Link>
      {pathSegments.map((segment, index) => {
        const path = '/' + pathSegments.slice(0, index + 1).join('/');
        const isLast = index === pathSegments.length - 1;
        const label = formatSegment(segment);

        if (isLast) {
          return (
            <Typography key={path} color="text.primary" variant="body2">
              {label}
            </Typography>
          );
        }

        return (
          <Link
            key={path}
            component={RouterLink}
            to={path}
            color="inherit"
            underline="hover"
            variant="body2"
          >
            {label}
          </Link>
        );
      })}
    </MuiBreadcrumbs>
  );
}
