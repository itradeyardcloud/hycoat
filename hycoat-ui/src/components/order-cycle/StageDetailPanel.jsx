import { Box, Chip, Collapse, Divider, IconButton, Link as MuiLink, List, ListItem, ListItemIcon, ListItemText, Paper, Typography } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import CloseIcon from '@mui/icons-material/Close';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import ArrowRightIcon from '@mui/icons-material/ArrowRight';
import OutputIcon from '@mui/icons-material/Inventory2';
import { DEPARTMENT_COLORS } from '../../data/orderCycleData';

export default function StageDetailPanel({ stage, onClose }) {
  if (!stage) return null;

  const deptColor = DEPARTMENT_COLORS[stage.department] || '#757575';

  return (
    <Collapse in={!!stage} timeout={300}>
      <Paper
        elevation={2}
        sx={{
          mt: 3,
          p: { xs: 2, md: 3 },
          borderRadius: 2,
          borderTop: 4,
          borderColor: deptColor,
        }}
      >
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, flexWrap: 'wrap' }}>
            <stage.icon sx={{ fontSize: 32, color: deptColor }} />
            <Box>
              <Typography variant="h6" fontWeight={600}>
                {stage.name}
              </Typography>
              <Chip
                label={stage.department}
                size="small"
                sx={{
                  bgcolor: `${deptColor}18`,
                  color: deptColor,
                  fontWeight: 600,
                  fontSize: '0.7rem',
                  height: 22,
                }}
              />
            </Box>
          </Box>
          <IconButton size="small" onClick={onClose} sx={{ mt: -0.5 }}>
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>

        <Typography variant="body2" color="text.secondary" sx={{ mb: 2.5 }}>
          {stage.description}
        </Typography>

        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' },
            gap: 3,
          }}
        >
          {/* What Happens */}
          <Box>
            <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1, display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <ArrowRightIcon sx={{ color: deptColor }} />
              What Happens
            </Typography>
            <List dense disablePadding>
              {stage.whatHappens.map((step, i) => (
                <ListItem key={i} disableGutters sx={{ py: 0.25, alignItems: 'flex-start' }}>
                  <ListItemIcon sx={{ minWidth: 28, mt: 0.5 }}>
                    <Box
                      sx={{
                        width: 18,
                        height: 18,
                        borderRadius: '50%',
                        bgcolor: `${deptColor}20`,
                        color: deptColor,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '0.6rem',
                        fontWeight: 700,
                      }}
                    >
                      {i + 1}
                    </Box>
                  </ListItemIcon>
                  <ListItemText primary={step} primaryTypographyProps={{ variant: 'body2', fontSize: '0.8rem' }} />
                </ListItem>
              ))}
            </List>
          </Box>

          {/* App Features */}
          <Box>
            <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1, display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <OpenInNewIcon sx={{ color: deptColor, fontSize: 20 }} />
              App Features
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
              {stage.features.map((feature) => (
                <Paper key={feature.path} variant="outlined" sx={{ p: 1.5, borderRadius: 1.5, '&:hover': { bgcolor: 'action.hover' } }}>
                  <MuiLink
                    component={RouterLink}
                    to={feature.path}
                    underline="hover"
                    sx={{ fontWeight: 600, fontSize: '0.85rem', color: deptColor, display: 'flex', alignItems: 'center', gap: 0.5 }}
                  >
                    {feature.name}
                    <OpenInNewIcon sx={{ fontSize: 14 }} />
                  </MuiLink>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, fontSize: '0.75rem', lineHeight: 1.4 }}>
                    {feature.desc}
                  </Typography>
                </Paper>
              ))}
            </Box>
          </Box>

          {/* Key Outputs */}
          <Box>
            <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1, display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <OutputIcon sx={{ color: deptColor, fontSize: 20 }} />
              Key Outputs
            </Typography>
            <List dense disablePadding>
              {stage.outputs.map((output, i) => (
                <ListItem key={i} disableGutters sx={{ py: 0.25 }}>
                  <ListItemIcon sx={{ minWidth: 28 }}>
                    <CheckCircleOutlineIcon sx={{ fontSize: 18, color: deptColor }} />
                  </ListItemIcon>
                  <ListItemText primary={output} primaryTypographyProps={{ variant: 'body2', fontSize: '0.8rem' }} />
                </ListItem>
              ))}
            </List>
          </Box>
        </Box>
      </Paper>
    </Collapse>
  );
}
