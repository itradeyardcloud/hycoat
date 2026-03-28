import { Badge, IconButton, Tooltip } from '@mui/material';
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone';
import { useNavigate } from 'react-router-dom';

export default function NotificationBell() {
  const navigate = useNavigate();

  return (
    <Tooltip title="Notifications">
      <IconButton color="inherit" onClick={() => navigate('/notifications')}>
        <Badge color="error" variant="dot">
          <NotificationsNoneIcon />
        </Badge>
      </IconButton>
    </Tooltip>
  );
}
