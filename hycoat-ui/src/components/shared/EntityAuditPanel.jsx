import { Box, Chip, CircularProgress, Paper, Stack, Typography } from '@mui/material';
import { useEntityAuditLogs } from '@/hooks/useAuditLogs';

export default function EntityAuditPanel({ entityName, entityId }) {
  const { data, isLoading } = useEntityAuditLogs(entityName, entityId, {
    page: 1,
    pageSize: 10,
  });

  const listData = data?.data ?? data;
  const items = listData?.items ?? [];

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1.5 }}>
        Change History
      </Typography>

      {isLoading ? (
        <Box sx={{ py: 2, display: 'flex', justifyContent: 'center' }}>
          <CircularProgress size={24} />
        </Box>
      ) : items.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No audit events found for this record.
        </Typography>
      ) : (
        <Stack spacing={1}>
          {items.map((item) => (
            <Paper key={item.id} variant="outlined" sx={{ p: 1.25 }}>
              <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={1}>
                <Typography variant="body2" fontWeight={600}>
                  {item.userName || 'System'} - {item.action}
                </Typography>
                <Chip size="small" label={new Date(item.timestamp).toLocaleString()} />
              </Stack>
              {item.changedColumns ? (
                <Typography variant="caption" color="text.secondary">
                  Changed: {item.changedColumns}
                </Typography>
              ) : null}
            </Paper>
          ))}
        </Stack>
      )}
    </Paper>
  );
}
