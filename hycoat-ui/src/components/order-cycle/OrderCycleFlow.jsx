import { Box, Paper, Typography, useMediaQuery, useTheme } from '@mui/material';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import CallSplitIcon from '@mui/icons-material/CallSplit';
import { DEPARTMENT_COLORS, PURCHASE_BRANCH } from '../../data/orderCycleData';

export default function OrderCycleFlow({ stages, selectedStage, onSelectStage, activeDepartment }) {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const isStageActive = (stage) => {
    if (!activeDepartment) return true;
    return stage.department === activeDepartment;
  };

  const isPurchaseActive = !activeDepartment || activeDepartment === 'Purchase';

  return (
    <Box>
      {/* Main flow */}
      <Box
        sx={{
          display: 'flex',
          flexDirection: isMobile ? 'column' : 'row',
          alignItems: 'center',
          gap: isMobile ? 1 : 0,
          flexWrap: isMobile ? 'nowrap' : 'wrap',
          justifyContent: 'center',
        }}
      >
        {stages.map((stage, index) => (
          <Box
            key={stage.id}
            sx={{
              display: 'flex',
              flexDirection: isMobile ? 'column' : 'row',
              alignItems: 'center',
            }}
          >
            <StageNode
              stage={stage}
              isSelected={selectedStage === stage.id}
              isDimmed={!isStageActive(stage)}
              onClick={() => onSelectStage(stage.id === selectedStage ? null : stage.id)}
            />
            {index < stages.length - 1 && (
              <Box sx={{ display: 'flex', alignItems: 'center', color: 'text.disabled', mx: isMobile ? 0 : 0.5, my: isMobile ? 0.5 : 0 }}>
                {isMobile ? <ArrowDownwardIcon fontSize="small" /> : <ArrowForwardIcon fontSize="small" />}
              </Box>
            )}

            {/* Purchase branch indicator — shown after Planning stage */}
            {stage.id === 'production-planning' && !isMobile && (
              <Box sx={{ position: 'relative' }}>
                <Box
                  sx={{
                    position: 'absolute',
                    top: '100%',
                    left: '50%',
                    transform: 'translateX(-100%)',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    mt: 1,
                    opacity: isPurchaseActive ? 1 : 0.3,
                    transition: 'opacity 0.3s',
                  }}
                >
                  <CallSplitIcon sx={{ fontSize: 18, color: 'text.secondary', transform: 'rotate(180deg)' }} />
                  <Paper
                    elevation={selectedStage === 'purchase' ? 6 : 1}
                    onClick={() => onSelectStage(selectedStage === 'purchase' ? null : 'purchase')}
                    sx={{
                      px: 1.5,
                      py: 1,
                      cursor: 'pointer',
                      textAlign: 'center',
                      borderRadius: 2,
                      border: 2,
                      borderColor: selectedStage === 'purchase' ? DEPARTMENT_COLORS.Purchase : 'transparent',
                      bgcolor: selectedStage === 'purchase' ? `${DEPARTMENT_COLORS.Purchase}10` : 'background.paper',
                      transition: 'all 0.2s',
                      '&:hover': { borderColor: DEPARTMENT_COLORS.Purchase, transform: 'scale(1.05)' },
                      minWidth: 110,
                    }}
                  >
                    <PURCHASE_BRANCH.icon sx={{ fontSize: 22, color: DEPARTMENT_COLORS.Purchase }} />
                    <Typography variant="caption" display="block" fontWeight={600} sx={{ lineHeight: 1.2, mt: 0.5 }}>
                      {PURCHASE_BRANCH.shortName}
                    </Typography>
                    <Typography
                      variant="caption"
                      sx={{
                        fontSize: '0.6rem',
                        color: DEPARTMENT_COLORS.Purchase,
                        fontWeight: 500,
                      }}
                    >
                      (parallel)
                    </Typography>
                  </Paper>
                </Box>
              </Box>
            )}
          </Box>
        ))}
      </Box>

      {/* Purchase branch for mobile — shown after planning */}
      {isMobile && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, ml: 4, my: 1, opacity: isPurchaseActive ? 1 : 0.3, transition: 'opacity 0.3s' }}>
          <CallSplitIcon sx={{ fontSize: 18, color: 'text.secondary', transform: 'rotate(90deg)' }} />
          <Paper
            elevation={selectedStage === 'purchase' ? 6 : 1}
            onClick={() => onSelectStage(selectedStage === 'purchase' ? null : 'purchase')}
            sx={{
              px: 1.5,
              py: 1,
              cursor: 'pointer',
              textAlign: 'center',
              borderRadius: 2,
              border: 2,
              borderColor: selectedStage === 'purchase' ? DEPARTMENT_COLORS.Purchase : 'transparent',
              bgcolor: selectedStage === 'purchase' ? `${DEPARTMENT_COLORS.Purchase}10` : 'background.paper',
              transition: 'all 0.2s',
              '&:hover': { borderColor: DEPARTMENT_COLORS.Purchase },
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <PURCHASE_BRANCH.icon sx={{ fontSize: 20, color: DEPARTMENT_COLORS.Purchase }} />
              <Typography variant="caption" fontWeight={600}>
                {PURCHASE_BRANCH.name}
              </Typography>
            </Box>
            <Typography variant="caption" sx={{ fontSize: '0.6rem', color: 'text.secondary' }}>
              (parallel branch from Planning)
            </Typography>
          </Paper>
        </Box>
      )}
    </Box>
  );
}

function StageNode({ stage, isSelected, isDimmed, onClick }) {
  const deptColor = DEPARTMENT_COLORS[stage.department] || '#757575';
  const Icon = stage.icon;

  return (
    <Paper
      elevation={isSelected ? 6 : 1}
      onClick={onClick}
      sx={{
        px: { xs: 2, md: 1.5 },
        py: { xs: 1.5, md: 2 },
        cursor: 'pointer',
        textAlign: 'center',
        borderRadius: 2,
        border: 2,
        borderColor: isSelected ? deptColor : 'transparent',
        bgcolor: isSelected ? `${deptColor}10` : 'background.paper',
        opacity: isDimmed ? 0.3 : 1,
        transition: 'all 0.2s',
        '&:hover': {
          borderColor: deptColor,
          transform: isDimmed ? 'none' : 'scale(1.05)',
          boxShadow: isDimmed ? undefined : 4,
        },
        minWidth: { xs: 'auto', md: 100 },
        maxWidth: { xs: '100%', md: 120 },
        width: { xs: '100%', md: 'auto' },
        display: 'flex',
        flexDirection: { xs: 'row', md: 'column' },
        alignItems: 'center',
        gap: { xs: 1.5, md: 0 },
        position: 'relative',
      }}
    >
      {/* Stage number badge */}
      <Box
        sx={{
          position: 'absolute',
          top: -8,
          left: { xs: 'auto', md: -8 },
          right: { xs: -8, md: 'auto' },
          width: 22,
          height: 22,
          borderRadius: '50%',
          bgcolor: deptColor,
          color: '#fff',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          fontSize: '0.7rem',
          fontWeight: 700,
        }}
      >
        {stage.order}
      </Box>

      <Icon sx={{ fontSize: { xs: 24, md: 28 }, color: deptColor }} />
      <Box sx={{ textAlign: { xs: 'left', md: 'center' } }}>
        <Typography
          variant="caption"
          display="block"
          fontWeight={600}
          sx={{ lineHeight: 1.2, mt: { xs: 0, md: 0.5 }, fontSize: { xs: '0.8rem', md: '0.72rem' } }}
        >
          {stage.shortName}
        </Typography>
        <Typography
          variant="caption"
          sx={{
            fontSize: '0.6rem',
            color: deptColor,
            fontWeight: 500,
            display: 'block',
            lineHeight: 1.2,
          }}
        >
          {stage.department}
        </Typography>
      </Box>
    </Paper>
  );
}
