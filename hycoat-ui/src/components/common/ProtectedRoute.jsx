import { Navigate, Outlet } from 'react-router-dom';
import { Box, Typography, Button } from '@mui/material';
import LockIcon from '@mui/icons-material/Lock';
import PropTypes from 'prop-types';
import useAuthStore from '../../stores/authStore';

export default function ProtectedRoute({ allowedRoles, allowedDepartments }) {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Role check
  if (allowedRoles && !allowedRoles.includes(user?.role)) {
    return <AccessDenied />;
  }

  // Department check — Admin and Leader bypass
  if (allowedDepartments) {
    const isPrivileged = user?.role === 'Admin' || user?.role === 'Leader';
    if (!isPrivileged && !allowedDepartments.includes(user?.department)) {
      return <AccessDenied />;
    }
  }

  return <Outlet />;
}

ProtectedRoute.propTypes = {
  allowedRoles: PropTypes.arrayOf(PropTypes.string),
  allowedDepartments: PropTypes.arrayOf(PropTypes.string),
};

function AccessDenied() {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '60vh',
        gap: 2,
        p: 3,
        textAlign: 'center',
      }}
    >
      <LockIcon sx={{ fontSize: 64, color: 'text.secondary' }} />
      <Typography variant="h5" fontWeight={600}>
        Access Denied
      </Typography>
      <Typography color="text.secondary">
        You do not have permission to view this page.
      </Typography>
      <Button variant="contained" href="/dashboard">
        Go to Dashboard
      </Button>
    </Box>
  );
}
