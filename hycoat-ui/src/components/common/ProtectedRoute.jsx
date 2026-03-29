import { Outlet } from 'react-router-dom';
import { Box, Typography, Button } from '@mui/material';
import LockIcon from '@mui/icons-material/Lock';
import PropTypes from 'prop-types';
import useAuthStore from '../../stores/authStore';
import LoadingSpinner from './LoadingSpinner';

export default function ProtectedRoute({ allowedRoles, allowedDepartments, allowedGroups }) {
  const { isAuthenticated, isInitializing, user, authError, startLogin } = useAuthStore();

  if (isInitializing) {
    return <LoadingSpinner />;
  }

  if (!isAuthenticated) {
    return <AuthRequired message={authError} onSignIn={startLogin} />;
  }

  const isAdmin = user?.role === 'Admin';
  if (!isAdmin) {
    const hasRoleRule = Array.isArray(allowedRoles) && allowedRoles.length > 0;
    const hasDepartmentRule = Array.isArray(allowedDepartments) && allowedDepartments.length > 0;
    const hasGroupRule = Array.isArray(allowedGroups) && allowedGroups.length > 0;
    const hasAnyRule = hasRoleRule || hasDepartmentRule || hasGroupRule;

    const hasRoleAccess = hasRoleRule && allowedRoles.includes(user?.role);

    const isPrivilegedDepartmentBypass = user?.role === 'Leader';
    const hasDepartmentAccess = hasDepartmentRule
      && (isPrivilegedDepartmentBypass || allowedDepartments.includes(user?.department));

    const userGroups = user?.groups || [];
    const hasGroupAccess = hasGroupRule
      && allowedGroups.some((group) => userGroups.includes(group));

    if (hasAnyRule && !hasRoleAccess && !hasDepartmentAccess && !hasGroupAccess) {
      return <AccessDenied />;
    }
  }

  return <Outlet />;
}

ProtectedRoute.propTypes = {
  allowedRoles: PropTypes.arrayOf(PropTypes.string),
  allowedDepartments: PropTypes.arrayOf(PropTypes.string),
  allowedGroups: PropTypes.arrayOf(PropTypes.string),
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

function AuthRequired({ message, onSignIn }) {
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
        Sign In Required
      </Typography>
      <Typography color="text.secondary">
        {message || 'Please sign in with your Microsoft account to continue.'}
      </Typography>
      <Button variant="contained" onClick={onSignIn}>
        Sign In with Microsoft
      </Button>
    </Box>
  );
}

AuthRequired.propTypes = {
  message: PropTypes.string,
  onSignIn: PropTypes.func.isRequired,
};
