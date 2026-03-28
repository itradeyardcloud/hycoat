import { Paper, Typography } from '@mui/material';

export default function NotificationsPage() {
  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h5" fontWeight={600} gutterBottom>
        Notifications
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Notification center is under development.
      </Typography>
    </Paper>
  );
}
