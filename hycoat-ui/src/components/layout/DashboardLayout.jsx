import { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import {
  AppBar,
  Box,
  Drawer,
  IconButton,
  Toolbar,
  Typography,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Avatar,
  Divider,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import PersonIcon from '@mui/icons-material/Person';
import LogoutIcon from '@mui/icons-material/Logout';
import useAuthStore from '../../stores/authStore';
import useUiStore from '../../stores/uiStore';
import Sidebar from './Sidebar';
import BottomNav from './BottomNav';
import Breadcrumbs from './Breadcrumbs';
import NotificationBell from '../notifications/NotificationBell';

const DRAWER_WIDTH = 240;
const DRAWER_COLLAPSED = 64;

export default function DashboardLayout() {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const navigate = useNavigate();

  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);

  const sidebarOpen = useUiStore((s) => s.sidebarOpen);
  const toggleSidebar = useUiStore((s) => s.toggleSidebar);
  const mobileDrawerOpen = useUiStore((s) => s.mobileDrawerOpen);
  const toggleMobileDrawer = useUiStore((s) => s.toggleMobileDrawer);
  const closeMobileDrawer = useUiStore((s) => s.closeMobileDrawer);

  const [avatarAnchor, setAvatarAnchor] = useState(null);

  const drawerWidth = sidebarOpen ? DRAWER_WIDTH : DRAWER_COLLAPSED;

  const handleLogout = () => {
    setAvatarAnchor(null);
    logout();
    navigate('/login');
  };

  const userInitials = user?.fullName
    ? user.fullName.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2)
    : '?';

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {/* AppBar */}
      <AppBar
        position="fixed"
        sx={{
          zIndex: theme.zIndex.drawer + 1,
          ...(isDesktop && {
            width: `calc(100% - ${drawerWidth}px)`,
            ml: `${drawerWidth}px`,
            transition: theme.transitions.create(['width', 'margin'], {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
          }),
        }}
        color="inherit"
        elevation={1}
      >
        <Toolbar>
          {isDesktop ? (
            <IconButton edge="start" onClick={toggleSidebar} sx={{ mr: 1 }}>
              {sidebarOpen ? <ChevronLeftIcon /> : <MenuIcon />}
            </IconButton>
          ) : (
            <IconButton edge="start" onClick={toggleMobileDrawer} sx={{ mr: 1 }}>
              <MenuIcon />
            </IconButton>
          )}

          <Typography variant="h6" noWrap sx={{ flexGrow: 1, fontWeight: 600 }}>
            HyCoat ERP
          </Typography>

          {/* Notification bell */}
          <NotificationBell />

          {/* User avatar */}
          <IconButton onClick={(e) => setAvatarAnchor(e.currentTarget)} size="small">
            <Avatar
              sx={{
                width: 32,
                height: 32,
                bgcolor: 'primary.main',
                fontSize: 14,
              }}
            >
              {userInitials}
            </Avatar>
          </IconButton>

          <Menu
            anchorEl={avatarAnchor}
            open={Boolean(avatarAnchor)}
            onClose={() => setAvatarAnchor(null)}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            transformOrigin={{ vertical: 'top', horizontal: 'right' }}
          >
            <Box sx={{ px: 2, py: 1 }}>
              <Typography variant="subtitle2" fontWeight={600}>
                {user?.fullName}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {user?.role} — {user?.department}
              </Typography>
            </Box>
            <Divider />
            <MenuItem
              onClick={() => {
                setAvatarAnchor(null);
                // Profile page — future feature
              }}
            >
              <ListItemIcon><PersonIcon fontSize="small" /></ListItemIcon>
              <ListItemText>Profile</ListItemText>
            </MenuItem>
            <MenuItem onClick={handleLogout}>
              <ListItemIcon><LogoutIcon fontSize="small" /></ListItemIcon>
              <ListItemText>Logout</ListItemText>
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      {/* Desktop Drawer */}
      {isDesktop && (
        <Drawer
          variant="permanent"
          sx={{
            width: drawerWidth,
            flexShrink: 0,
            '& .MuiDrawer-paper': {
              width: drawerWidth,
              boxSizing: 'border-box',
              borderRight: '1px solid',
              borderColor: 'divider',
              transition: theme.transitions.create('width', {
                easing: theme.transitions.easing.sharp,
                duration: theme.transitions.duration.leavingScreen,
              }),
              overflowX: 'hidden',
            },
          }}
        >
          <Toolbar />
          <Sidebar collapsed={!sidebarOpen} />
        </Drawer>
      )}

      {/* Mobile Drawer */}
      {!isDesktop && (
        <Drawer
          variant="temporary"
          open={mobileDrawerOpen}
          onClose={closeMobileDrawer}
          ModalProps={{ keepMounted: true }}
          sx={{
            '& .MuiDrawer-paper': {
              width: DRAWER_WIDTH,
              boxSizing: 'border-box',
            },
          }}
        >
          <Toolbar />
          <Sidebar collapsed={false} onItemClick={closeMobileDrawer} />
        </Drawer>
      )}

      {/* Main Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          minHeight: '100vh',
          ...(isDesktop && {
            width: `calc(100% - ${drawerWidth}px)`,
            transition: theme.transitions.create('width', {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
          }),
        }}
      >
        <Toolbar />
        <Box
          sx={{
            flexGrow: 1,
            p: { xs: 2, sm: 3 },
            pb: !isDesktop ? '72px' : undefined,
          }}
        >
          <Breadcrumbs />
          <Outlet />
        </Box>
      </Box>

      {/* Mobile Bottom Navigation */}
      {!isDesktop && <BottomNav />}
    </Box>
  );
}
