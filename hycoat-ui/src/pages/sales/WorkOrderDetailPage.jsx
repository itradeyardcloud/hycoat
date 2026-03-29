import { useNavigate, useParams } from 'react-router-dom';
import {
  Box,
  Button,
  CircularProgress,
  Chip,
  Grid,
  Paper,
  Stack,
  Typography,
  Divider,
} from '@mui/material';
import { CheckCircle, RadioButtonUnchecked } from '@mui/icons-material';
import PageHeader from '@/components/common/PageHeader';
import { useWorkOrder, useWorkOrderTimeline } from '@/hooks/useWorkOrders';
import { formatDate } from '@/utils/formatters';

const STATUS_COLORS = {
  Created: 'default',
  MaterialAwaited: 'warning',
  MaterialReceived: 'info',
  InProduction: 'primary',
  QAComplete: 'secondary',
  Dispatched: 'success',
  Invoiced: 'success',
  Closed: 'default',
};

export default function WorkOrderDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const { data: woData, isLoading: loadingWO } = useWorkOrder(id);
  const { data: timelineData, isLoading: loadingTimeline } = useWorkOrderTimeline(id);

  const wo = woData?.data;
  const timeline = timelineData?.data?.events ?? [];

  if (loadingWO || loadingTimeline) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!wo) {
    return (
      <>
        <PageHeader title="Work Order" />
        <Typography color="text.secondary">Work order not found.</Typography>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={wo.woNumber || wo.wONumber || 'Work Order'}
        subtitle={wo.projectName || undefined}
        action={<Chip label={wo.status} color={STATUS_COLORS[wo.status] || 'default'} />}
      />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} sx={{ mb: 2 }}>
        <Button variant="outlined" onClick={() => navigate('/sales/work-orders')}>
          Back
        </Button>
        <Button variant="contained" onClick={() => navigate(`/sales/work-orders/${id}/edit`)}>
          Edit Work Order
        </Button>
      </Stack>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1.5 }}>
              Details
            </Typography>

            <DetailRow label="Date" value={formatDate(wo.date)} />
            <DetailRow label="Customer" value={wo.customerName} />
            <DetailRow label="Project" value={wo.projectName || '-'} />
            <DetailRow label="Process Type" value={wo.processTypeName} />
            <DetailRow label="Powder Color" value={wo.powderColorName || '-'} />
            <DetailRow label="Powder Code" value={wo.powderCode || '-'} />
            <DetailRow label="PI Number" value={wo.piNumber || wo.pINumber || '-'} />
            <DetailRow label="Dispatch Date" value={wo.dispatchDate ? formatDate(wo.dispatchDate) : '-'} />
            <DetailRow label="Notes" value={wo.notes || '-'} />
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Paper variant="outlined" sx={{ p: 2, mb: 2 }}>
            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1.5 }}>
              Linked Documents
            </Typography>
            <DetailRow label="Material Inwards" value={String(wo.materialInwardCount || 0)} />
            <DetailRow label="Production Work Orders" value={String(wo.productionWorkOrderCount || 0)} />
            <DetailRow label="Delivery Challans" value={String(wo.deliveryChallanCount || 0)} />
            <DetailRow label="Invoices" value={String(wo.invoiceCount || 0)} />
          </Paper>

          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1.5 }}>
              Timeline
            </Typography>

            {timeline.length === 0 ? (
              <Typography color="text.secondary" variant="body2">
                No timeline events.
              </Typography>
            ) : (
              <Stack spacing={1.2}>
                {timeline.map((event, idx) => (
                  <Box key={`${event.stage}-${idx}`}>
                    <Stack direction="row" spacing={1} alignItems="center">
                      {event.isComplete ? (
                        <CheckCircle fontSize="small" color="success" />
                      ) : (
                        <RadioButtonUnchecked fontSize="small" color="disabled" />
                      )}
                      <Typography variant="body2" fontWeight={600}>
                        {event.stage}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {event.documentNumber || '-'}
                      </Typography>
                    </Stack>
                    <Typography variant="caption" color="text.secondary" sx={{ ml: 3.5 }}>
                      {event.date ? formatDate(event.date) : 'Pending'}
                    </Typography>
                    {idx < timeline.length - 1 && <Divider sx={{ mt: 1 }} />}
                  </Box>
                ))}
              </Stack>
            )}
          </Paper>
        </Grid>
      </Grid>
    </>
  );
}

function DetailRow({ label, value }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, py: 0.5 }}>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ textAlign: 'right' }}>
        {value}
      </Typography>
    </Box>
  );
}
