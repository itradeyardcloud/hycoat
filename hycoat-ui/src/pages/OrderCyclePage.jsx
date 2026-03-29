import { useState } from 'react';
import { Box, Paper, Typography, Chip } from '@mui/material';
import PageHeader from '../components/common/PageHeader';
import OrderCycleFlow from '../components/order-cycle/OrderCycleFlow';
import StageDetailPanel from '../components/order-cycle/StageDetailPanel';
import DepartmentFilter from '../components/order-cycle/DepartmentFilter';
import { ORDER_CYCLE_STAGES, PURCHASE_BRANCH, DEPARTMENT_COLORS } from '../data/orderCycleData';

export default function OrderCyclePage() {
  const [selectedStage, setSelectedStage] = useState(null);
  const [activeDepartment, setActiveDepartment] = useState(null);

  const getSelectedStageData = () => {
    if (!selectedStage) return null;
    if (selectedStage === 'purchase') return PURCHASE_BRANCH;
    return ORDER_CYCLE_STAGES.find((s) => s.id === selectedStage) || null;
  };

  const stageData = getSelectedStageData();

  return (
    <Box>
      <PageHeader
        title="Order Cycle"
        subtitle="How each department and app feature fits into the order workflow"
      />

      {/* Department legend */}
      <Paper variant="outlined" sx={{ p: 2, mb: 3, borderRadius: 2 }}>
        <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5 }}>
          Filter by Department
        </Typography>
        <DepartmentFilter activeDepartment={activeDepartment} onSelect={setActiveDepartment} />
        <Typography variant="caption" color="text.secondary">
          Click a stage in the flow below to see details, app features, and direct links.
          {activeDepartment && (
            <>
              {' '}Showing stages for <strong>{activeDepartment}</strong> department.
            </>
          )}
        </Typography>
      </Paper>

      {/* Flow diagram */}
      <Box sx={{ py: { xs: 2, md: 4 }, pb: { md: 6 } }}>
        <OrderCycleFlow
          stages={ORDER_CYCLE_STAGES}
          selectedStage={selectedStage}
          onSelectStage={setSelectedStage}
          activeDepartment={activeDepartment}
        />
      </Box>

      {/* Detail panel */}
      <StageDetailPanel stage={stageData} onClose={() => setSelectedStage(null)} />

      {/* Department color legend (bottom) */}
      <Paper variant="outlined" sx={{ p: 2, mt: 3, borderRadius: 2 }}>
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
          Department Color Legend
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          {Object.entries(DEPARTMENT_COLORS).map(([dept, color]) => (
            <Chip
              key={dept}
              label={dept}
              size="small"
              sx={{
                bgcolor: `${color}18`,
                color,
                fontWeight: 600,
                fontSize: '0.7rem',
                height: 24,
              }}
            />
          ))}
        </Box>
      </Paper>
    </Box>
  );
}
