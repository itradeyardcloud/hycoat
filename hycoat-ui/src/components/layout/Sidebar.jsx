import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  Tooltip,
  Box,
  Divider,
  Typography,
} from '@mui/material';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import useAuthStore from '../../stores/authStore';
import useUiStore from '../../stores/uiStore';
import { getFilteredNavItems } from '../../utils/navigation';

export default function Sidebar({ collapsed = false, onItemClick }) {
  const navigate = useNavigate();
  const location = useLocation();
  const user = useAuthStore((s) => s.user);
  const sidebarOpen = useUiStore((s) => s.sidebarOpen);
  const isCollapsed = collapsed || !sidebarOpen;
  const navItems = getFilteredNavItems(user);
  const [openMenus, setOpenMenus] = useState({});

  const toggleMenu = (label) => {
    setOpenMenus((prev) => ({ ...prev, [label]: !prev[label] }));
  };

  const handleNavigate = (path) => {
    navigate(path);
    onItemClick?.();
  };

  const isActive = (path) => location.pathname === path;
  const isParentActive = (item) => {
    if (item.path) return isActive(item.path);
    return item.children?.some((child) => location.pathname.startsWith(child.path));
  };

  return (
    <Box sx={{ py: 1, overflow: 'auto', height: '100%' }}>
      {!isCollapsed && (
        <Box sx={{ px: 2, py: 1.5, mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
            <Box
              component="img"
              src="/branding/logo.png"
              alt="HYCOAT"
              sx={{ width: 32, height: 32, display: 'block' }}
            />
            <Box sx={{ minWidth: 0 }}>
              <Typography variant="subtitle2" sx={{ lineHeight: 1.1, fontWeight: 700, letterSpacing: 0.3 }}>
                HYCOAT SYSTEMS
              </Typography>
              <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.1 }}>
                Shielding Surfaces
              </Typography>
            </Box>
          </Box>
        </Box>
      )}
      {isCollapsed && (
        <Box sx={{ textAlign: 'center', py: 1.5, mb: 1 }}>
          <Box
            component="img"
            src="/branding/logo.png"
            alt="HYCOAT"
            sx={{ width: 32, height: 32, mx: 'auto', display: 'block' }}
          />
        </Box>
      )}
      <Divider sx={{ mb: 1 }} />

      <List component="nav" disablePadding>
        {navItems.map((item) => {
          const Icon = item.icon;
          const hasChildren = item.children && item.children.length > 0;
          const parentActive = isParentActive(item);
          const menuOpen = openMenus[item.label] ?? parentActive;

          if (!hasChildren) {
            // Simple link item
            const btn = (
              <ListItemButton
                key={item.label}
                selected={isActive(item.path)}
                onClick={() => handleNavigate(item.path)}
                sx={{
                  mx: 1,
                  borderRadius: 1,
                  minHeight: 44,
                  justifyContent: isCollapsed ? 'center' : 'initial',
                  px: isCollapsed ? 1 : 2,
                }}
              >
                <ListItemIcon
                  sx={{
                    minWidth: isCollapsed ? 0 : 40,
                    justifyContent: 'center',
                    color: parentActive ? 'primary.main' : 'text.secondary',
                  }}
                >
                  <Icon fontSize="small" />
                </ListItemIcon>
                {!isCollapsed && <ListItemText primary={item.label} />}
              </ListItemButton>
            );
            return isCollapsed ? (
              <Tooltip key={item.label} title={item.label} placement="right">
                {btn}
              </Tooltip>
            ) : (
              btn
            );
          }

          // Collapsible parent
          const parentBtn = (
            <ListItemButton
              onClick={() => {
                if (isCollapsed) {
                  // Navigate to first child on collapsed click
                  handleNavigate(item.children[0].path);
                } else {
                  toggleMenu(item.label);
                }
              }}
              sx={{
                mx: 1,
                borderRadius: 1,
                minHeight: 44,
                justifyContent: isCollapsed ? 'center' : 'initial',
                px: isCollapsed ? 1 : 2,
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: isCollapsed ? 0 : 40,
                  justifyContent: 'center',
                  color: parentActive ? 'primary.main' : 'text.secondary',
                }}
              >
                <Icon fontSize="small" />
              </ListItemIcon>
              {!isCollapsed && (
                <>
                  <ListItemText
                    primary={item.label}
                    primaryTypographyProps={{
                      fontWeight: parentActive ? 600 : 400,
                    }}
                  />
                  {menuOpen ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />}
                </>
              )}
            </ListItemButton>
          );

          return (
            <Box key={item.label}>
              {isCollapsed ? (
                <Tooltip title={item.label} placement="right">
                  {parentBtn}
                </Tooltip>
              ) : (
                parentBtn
              )}
              {!isCollapsed && (
                <Collapse in={menuOpen} timeout="auto" unmountOnExit>
                  <List component="div" disablePadding>
                    {item.children.map((child) => (
                      <ListItemButton
                        key={child.path}
                        selected={isActive(child.path)}
                        onClick={() => handleNavigate(child.path)}
                        sx={{
                          pl: 6,
                          mx: 1,
                          borderRadius: 1,
                          minHeight: 36,
                        }}
                      >
                        <ListItemText
                          primary={child.label}
                          primaryTypographyProps={{ variant: 'body2' }}
                        />
                      </ListItemButton>
                    ))}
                  </List>
                </Collapse>
              )}
            </Box>
          );
        })}
      </List>
    </Box>
  );
}
