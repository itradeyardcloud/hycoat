import { Box, Chip } from '@mui/material';
import { DEPARTMENTS } from '../../utils/constants';
import { DEPARTMENT_COLORS } from '../../data/orderCycleData';

export default function DepartmentFilter({ activeDepartment, onSelect }) {
  return (
    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 3 }}>
      <Chip
        label="All Departments"
        variant={!activeDepartment ? 'filled' : 'outlined'}
        color={!activeDepartment ? 'primary' : 'default'}
        onClick={() => onSelect(null)}
        sx={{ fontWeight: 600 }}
      />
      {DEPARTMENTS.map((dept) => {
        const isActive = activeDepartment === dept;
        const color = DEPARTMENT_COLORS[dept] || '#757575';
        return (
          <Chip
            key={dept}
            label={dept}
            variant={isActive ? 'filled' : 'outlined'}
            onClick={() => onSelect(isActive ? null : dept)}
            sx={{
              fontWeight: 600,
              borderColor: color,
              color: isActive ? '#fff' : color,
              bgcolor: isActive ? color : 'transparent',
              '&:hover': {
                bgcolor: isActive ? color : `${color}15`,
              },
            }}
          />
        );
      })}
    </Box>
  );
}
