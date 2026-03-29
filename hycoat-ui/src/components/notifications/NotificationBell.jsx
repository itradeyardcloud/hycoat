import { useEffect, useMemo, useState } from 'react';
import {
  Badge,
  Box,
  Button,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
  Typography,
  Tooltip,
} from '@mui/material';
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined';
import ErrorOutlineOutlinedIcon from '@mui/icons-material/ErrorOutlineOutlined';
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import {
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
  useUnreadNotificationCount,
} from '@/hooks/useNotifications';
import useNotificationStore from '@/stores/notificationStore';
import { getNotificationRoute } from '@/utils/notificationNavigator';
import { startNotificationConnection, stopNotificationConnection } from '@/services/signalr';
import useAuthStore from '@/stores/authStore';

export default function NotificationBell() {
  const navigate = useNavigate();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [anchorEl, setAnchorEl] = useState(null);
  const open = Boolean(anchorEl);

  const notifications = useNotificationStore((s) => s.notifications);
  const unreadCount = useNotificationStore((s) => s.unreadCount);
  const setLatestNotifications = useNotificationStore((s) => s.setLatestNotifications);
  const setUnreadCount = useNotificationStore((s) => s.setUnreadCount);
  const addNotification = useNotificationStore((s) => s.addNotification);
  const markReadInStore = useNotificationStore((s) => s.markRead);
  const markAllInStore = useNotificationStore((s) => s.markAllRead);

  const listQuery = useNotifications({ page: 1, pageSize: 10 });
  const unreadQuery = useUnreadNotificationCount();
  const markReadMutation = useMarkNotificationRead();
  const markAllMutation = useMarkAllNotificationsRead();

  const latestFromApi = useMemo(() => {
    const listData = listQuery.data?.data ?? listQuery.data;
    return listData?.items ?? [];
  }, [listQuery.data]);

  useEffect(() => {
    setLatestNotifications(latestFromApi);
  }, [latestFromApi, setLatestNotifications]);

  useEffect(() => {
    const countData = unreadQuery.data?.data ?? unreadQuery.data;
    if (typeof countData === 'number') {
      setUnreadCount(countData);
    }
  }, [unreadQuery.data, setUnreadCount]);

  useEffect(() => {
    if (!isAuthenticated) {
      return undefined;
    }

    let disposed = false;

    startNotificationConnection((notification) => {
      if (disposed) return;
      addNotification(notification);
      toast(notification.title || 'New notification');
    }).catch(() => {
      // Keep UI functional even if realtime connection fails.
    });

    return () => {
      disposed = true;
      stopNotificationConnection().catch(() => {});
    };
  }, [addNotification, isAuthenticated]);

  const handleOpenMenu = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleCloseMenu = () => {
    setAnchorEl(null);
  };

  const handleMarkAllRead = () => {
    markAllMutation.mutate(undefined, {
      onSuccess: () => {
        markAllInStore();
      },
    });
  };

  const handleNotificationClick = (notification) => {
    if (!notification.isRead) {
      markReadMutation.mutate(notification.id);
      markReadInStore(notification.id);
    }

    handleCloseMenu();
    navigate(getNotificationRoute(notification.referenceType, notification.referenceId));
  };

  return (
    <>
      <Tooltip title="Notifications">
        <IconButton color="inherit" onClick={handleOpenMenu}>
          <Badge color="error" badgeContent={unreadCount} max={99}>
            <NotificationsNoneIcon />
          </Badge>
        </IconButton>
      </Tooltip>

      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleCloseMenu}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        slotProps={{ paper: { sx: { width: 380, maxWidth: '95vw' } } }}
      >
        <Box sx={{ px: 2, py: 1.5, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="subtitle1" fontWeight={600}>
            Notifications
          </Typography>
          <Button
            size="small"
            onClick={handleMarkAllRead}
            disabled={markAllMutation.isPending || unreadCount === 0}
          >
            Mark All Read
          </Button>
        </Box>
        <Divider />

        {listQuery.isLoading ? (
          <Box sx={{ px: 2, py: 2 }}>
            <Typography variant="body2" color="text.secondary">Loading...</Typography>
          </Box>
        ) : notifications.length === 0 ? (
          <Box sx={{ px: 2, py: 2 }}>
            <Typography variant="body2" color="text.secondary">No notifications yet.</Typography>
          </Box>
        ) : (
          <List dense disablePadding sx={{ maxHeight: 420, overflowY: 'auto' }}>
            {notifications.map((notification) => (
              <ListItemButton
                key={notification.id}
                onClick={() => handleNotificationClick(notification)}
                sx={{
                  alignItems: 'flex-start',
                  bgcolor: notification.isRead ? 'transparent' : 'action.hover',
                }}
              >
                <ListItemIcon sx={{ minWidth: 34, mt: 0.4 }}>
                  <NotificationTypeIcon type={notification.type} />
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Typography variant="body2" fontWeight={notification.isRead ? 500 : 700}>
                      {notification.title}
                    </Typography>
                  }
                  secondary={
                    <>
                      <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                        {notification.message}
                      </Typography>
                      <Typography variant="caption" color="text.disabled">
                        {formatRelativeTime(notification.createdAt)}
                      </Typography>
                    </>
                  }
                />
              </ListItemButton>
            ))}
          </List>
        )}

        <Divider />
        <Button
          fullWidth
          onClick={() => {
            handleCloseMenu();
            navigate('/notifications');
          }}
          sx={{ py: 1.2 }}
        >
          View All Notifications
        </Button>
      </Menu>
    </>
  );
}

function NotificationTypeIcon({ type }) {
  const normalized = (type || '').toLowerCase();

  if (normalized === 'warning') {
    return <WarningAmberOutlinedIcon color="warning" fontSize="small" />;
  }

  if (normalized === 'alert') {
    return <ErrorOutlineOutlinedIcon color="error" fontSize="small" />;
  }

  if (normalized === 'success') {
    return <CheckCircleOutlineOutlinedIcon color="success" fontSize="small" />;
  }

  return <InfoOutlinedIcon color="info" fontSize="small" />;
}

function formatRelativeTime(value) {
  if (!value) return '';

  const timestamp = new Date(value).getTime();
  if (Number.isNaN(timestamp)) return '';

  const diffMs = Date.now() - timestamp;
  const diffMins = Math.floor(diffMs / 60000);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins} min ago`;

  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours} hr ago`;

  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
}
