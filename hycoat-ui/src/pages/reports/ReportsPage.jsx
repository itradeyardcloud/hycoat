import { useNavigate } from 'react-router-dom';
import { Box, Grid, Card, CardActionArea, CardContent, Typography } from '@mui/material';
import PageHeader from '@/components/common/PageHeader';
import AssignmentIcon from '@mui/icons-material/Assignment';
import FactoryIcon from '@mui/icons-material/Factory';
import VerifiedIcon from '@mui/icons-material/Verified';
import PeopleIcon from '@mui/icons-material/People';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';

const REPORTS = [
  { title: 'Order Tracker', description: 'Track work orders through all stages', path: '/reports/order-tracker', icon: <AssignmentIcon sx={{ fontSize: 40 }} /> },
  { title: 'Production Throughput', description: 'Daily production summary with SFT trends', path: '/reports/production', icon: <FactoryIcon sx={{ fontSize: 40 }} /> },
  { title: 'Quality Summary', description: 'Inspection pass rates and DFT trends', path: '/reports/quality', icon: <VerifiedIcon sx={{ fontSize: 40 }} /> },
  { title: 'Customer History', description: 'Order history and revenue by customer', path: '/reports/customer', icon: <PeopleIcon sx={{ fontSize: 40 }} /> },
  { title: 'Dispatch Register', description: 'Delivery challan register with export', path: '/reports/dispatch', icon: <LocalShippingIcon sx={{ fontSize: 40 }} /> },
];

export default function ReportsPage() {
  const navigate = useNavigate();

  return (
    <Box>
      <PageHeader title="Reports" />
      <Grid container spacing={2}>
        {REPORTS.map((r) => (
          <Grid key={r.path} size={{ xs: 12, sm: 6, md: 4 }}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardActionArea onClick={() => navigate(r.path)} sx={{ height: '100%' }}>
                <CardContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', textAlign: 'center', py: 3 }}>
                  <Box sx={{ color: 'primary.main', mb: 1 }}>{r.icon}</Box>
                  <Typography variant="h6" gutterBottom>{r.title}</Typography>
                  <Typography variant="body2" color="text.secondary">{r.description}</Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
