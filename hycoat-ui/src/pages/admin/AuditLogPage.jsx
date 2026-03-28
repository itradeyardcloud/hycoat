import { Paper, Typography } from '@mui/material';

export default function AuditLogPage() {
  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h5" fontWeight={600} gutterBottom>
        Audit Logs
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Audit log reporting will be available here.
      </Typography>
    </Paper>
  );
}
