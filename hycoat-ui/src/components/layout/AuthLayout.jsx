import { Outlet } from 'react-router-dom';
import { Box, Card, Typography } from '@mui/material';

export default function AuthLayout() {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #e3f2fd 0%, #f5f5f5 50%, #fff3e0 100%)',
        p: 2,
      }}
    >
      <Card
        sx={{
          width: '100%',
          maxWidth: 420,
          p: { xs: 3, sm: 4 },
        }}
      >
        <Box sx={{ textAlign: 'center', mb: 3 }}>
          <Typography variant="h4" fontWeight={700} color="primary">
            HyCoat
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Internal Operations Management
          </Typography>
        </Box>
        <Outlet />
      </Card>
    </Box>
  );
}
