import { Card, CardContent, Typography, Box } from '@mui/material';

// eslint-disable-next-line react/prop-types
export default function KPICard({ title, value, subtitle, icon, onClick, color = 'primary.main' }) {
  return (
    <Card
      sx={{
        cursor: onClick ? 'pointer' : 'default',
        '&:hover': onClick ? { boxShadow: 4 } : {},
        height: '100%',
      }}
      onClick={onClick}
    >
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Box>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" fontWeight={700} color={color}>
              {value}
            </Typography>
            {subtitle && (
              <Typography variant="caption" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Box>
          {icon && (
            <Box sx={{ color, opacity: 0.7, fontSize: 40 }}>{icon}</Box>
          )}
        </Box>
      </CardContent>
    </Card>
  );
}
