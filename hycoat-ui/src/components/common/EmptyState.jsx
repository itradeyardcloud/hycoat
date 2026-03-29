import { Box, Typography } from '@mui/material';
import InboxIcon from '@mui/icons-material/Inbox';
import PropTypes from 'prop-types';

export default function EmptyState({ icon: Icon = InboxIcon, title, description, action }) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: 8,
        px: 3,
        textAlign: 'center',
      }}
    >
      <Icon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      {description && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2, maxWidth: 360 }}>
          {description}
        </Typography>
      )}
      {action && <Box>{action}</Box>}
    </Box>
  );
}

EmptyState.propTypes = {
  icon: PropTypes.elementType,
  title: PropTypes.node.isRequired,
  description: PropTypes.node,
  action: PropTypes.node,
};
