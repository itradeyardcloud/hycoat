import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  BottomNavigation,
  BottomNavigationAction,
  Paper,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import PrecisionManufacturingIcon from '@mui/icons-material/PrecisionManufacturing';
import VerifiedIcon from '@mui/icons-material/Verified';
import MoreHorizIcon from '@mui/icons-material/MoreHoriz';
import EventNoteIcon from '@mui/icons-material/EventNote';
import InventoryIcon from '@mui/icons-material/Inventory';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';
import StoreIcon from '@mui/icons-material/Store';
import BarChartIcon from '@mui/icons-material/BarChart';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import useAuthStore from '../../stores/authStore';

const navConfigs = {
  'Admin': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Orders', icon: <ShoppingCartIcon />, path: '/sales/work-orders' },
    { label: 'Production', icon: <PrecisionManufacturingIcon />, path: '/production/pretreatment' },
    { label: 'Quality', icon: <VerifiedIcon />, path: '/quality/in-process' },
  ],
  'Leader': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Orders', icon: <ShoppingCartIcon />, path: '/sales/work-orders' },
    { label: 'Production', icon: <PrecisionManufacturingIcon />, path: '/production/pretreatment' },
    { label: 'Quality', icon: <VerifiedIcon />, path: '/quality/in-process' },
  ],
  'Sales': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Inquiries', icon: <ShoppingCartIcon />, path: '/sales/inquiries' },
    { label: 'Work Orders', icon: <ShoppingCartIcon />, path: '/sales/work-orders' },
    { label: 'Quotations', icon: <ShoppingCartIcon />, path: '/sales/quotations' },
  ],
  'Production': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Schedule', icon: <EventNoteIcon />, path: '/ppc/schedule' },
    { label: 'Pretreat', icon: <PrecisionManufacturingIcon />, path: '/production/pretreatment' },
    { label: 'Coating', icon: <PrecisionManufacturingIcon />, path: '/production/coating' },
  ],
  'QA': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'In-Process', icon: <VerifiedIcon />, path: '/quality/in-process' },
    { label: 'Final', icon: <VerifiedIcon />, path: '/quality/final' },
    { label: 'Certs', icon: <VerifiedIcon />, path: '/quality/test-certificates' },
  ],
  'SCM': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Inward', icon: <InventoryIcon />, path: '/material-inward/inwards' },
    { label: 'Dispatch', icon: <LocalShippingIcon />, path: '/dispatch/packing-lists' },
    { label: 'Invoices', icon: <LocalShippingIcon />, path: '/dispatch/invoices' },
  ],
  'PPC': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Work Orders', icon: <EventNoteIcon />, path: '/ppc/work-orders' },
    { label: 'Schedule', icon: <EventNoteIcon />, path: '/ppc/schedule' },
    { label: 'Production', icon: <PrecisionManufacturingIcon />, path: '/production/pretreatment' },
  ],
  'Purchase': [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Indents', icon: <StoreIcon />, path: '/purchase/indents' },
    { label: 'Orders', icon: <StoreIcon />, path: '/purchase/orders' },
    { label: 'Stock', icon: <StoreIcon />, path: '/purchase/stock' },
  ],
};

const moreMenuItems = [
  { label: 'Masters', icon: <AdminPanelSettingsIcon fontSize="small" />, path: '/masters/customers', roles: ['Admin', 'Leader'] },
  { label: 'Reports', icon: <BarChartIcon fontSize="small" />, path: '/reports', roles: ['Admin', 'Leader'] },
  { label: 'Audit Logs', icon: <AdminPanelSettingsIcon fontSize="small" />, path: '/admin/audit-logs', roles: ['Admin'] },
];

export default function BottomNav() {
  const navigate = useNavigate();
  const location = useLocation();
  const user = useAuthStore((s) => s.user);
  const [moreAnchor, setMoreAnchor] = useState(null);

  const configKey = user?.role === 'Admin' || user?.role === 'Leader' ? user.role : user?.department;
  const items = navConfigs[configKey] || navConfigs['Admin'];

  const allPaths = [...items.map((i) => i.path), 'more'];
  const currentValue = items.findIndex((i) => location.pathname.startsWith(i.path));
  const value = currentValue >= 0 ? currentValue : 4;

  const filteredMore = moreMenuItems.filter((item) => {
    if (!item.roles) return true;
    return item.roles.includes(user?.role);
  });

  return (
    <>
      <Paper
        sx={{ position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: 1100 }}
        elevation={3}
      >
        <BottomNavigation
          value={value}
          onChange={(_, newValue) => {
            if (newValue === 4) return; // "More" handled by click
            navigate(items[newValue].path);
          }}
          showLabels
        >
          {items.map((item) => (
            <BottomNavigationAction
              key={item.label}
              label={item.label}
              icon={item.icon}
            />
          ))}
          <BottomNavigationAction
            label="More"
            icon={<MoreHorizIcon />}
            onClick={(e) => setMoreAnchor(e.currentTarget)}
          />
        </BottomNavigation>
      </Paper>

      <Menu
        anchorEl={moreAnchor}
        open={Boolean(moreAnchor)}
        onClose={() => setMoreAnchor(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
        transformOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        {filteredMore.map((item) => (
          <MenuItem
            key={item.label}
            onClick={() => {
              setMoreAnchor(null);
              navigate(item.path);
            }}
          >
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText>{item.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
