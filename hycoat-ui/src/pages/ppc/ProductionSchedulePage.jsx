import { useState, useMemo } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Tooltip,
  Typography,
  Autocomplete,
  CircularProgress,
} from '@mui/material';
import {
  ChevronLeft,
  ChevronRight,
  Add,
  Delete,
  ArrowUpward,
  ArrowDownward,
  Today,
} from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import StatusChip from '@/components/common/StatusChip';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import {
  useProductionSchedule,
  useCreateScheduleEntry,
  useUpdateScheduleStatus,
  useDeleteScheduleEntry,
  useReorderSchedule,
} from '@/hooks/useProductionSchedule';
import { useProductionWorkOrderLookup } from '@/hooks/useProductionWorkOrders';
import { useProductionUnits } from '@/hooks/useProductionUnits';

const SHIFTS = ['Day', 'Night'];

function getWeekRange(date) {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1);
  const monday = new Date(d.setDate(diff));
  monday.setHours(0, 0, 0, 0);
  const sunday = new Date(monday);
  sunday.setDate(monday.getDate() + 6);
  return { start: monday, end: sunday };
}

function formatDate(d) {
  return d.toISOString().split('T')[0];
}

function formatShortDate(d) {
  return d.toLocaleDateString('en-IN', { weekday: 'short', day: '2-digit', month: 'short' });
}

const STATUS_NEXT = {
  Planned: ['InProgress', 'Cancelled'],
  InProgress: ['Completed', 'Cancelled'],
};

export default function ProductionSchedulePage() {
  const [currentDate, setCurrentDate] = useState(() => new Date());
  const [unitFilter, setUnitFilter] = useState('');
  const [addOpen, setAddOpen] = useState(false);
  const [addSlot, setAddSlot] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const { start, end } = useMemo(() => getWeekRange(currentDate), [currentDate]);

  const { data: scheduleData, isLoading } = useProductionSchedule({
    startDate: formatDate(start),
    endDate: formatDate(end),
    productionUnitId: unitFilter || undefined,
  });
  const { data: unitsData } = useProductionUnits();
  const { data: pwoLookup } = useProductionWorkOrderLookup({ status: 'Created' });

  const createMutation = useCreateScheduleEntry();
  const statusMutation = useUpdateScheduleStatus();
  const deleteMutation = useDeleteScheduleEntry();
  const reorderMutation = useReorderSchedule();

  const units = unitsData?.data?.items ?? unitsData?.data ?? [];
  const entries = scheduleData?.data ?? [];
  const pwoOptions = pwoLookup?.data ?? [];

  const days = useMemo(() => {
    const arr = [];
    for (let i = 0; i < 7; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      arr.push(d);
    }
    return arr;
  }, [start]);

  const navigateWeek = (dir) => {
    setCurrentDate((prev) => {
      const next = new Date(prev);
      next.setDate(next.getDate() + dir * 7);
      return next;
    });
  };

  const goToday = () => setCurrentDate(new Date());

  const getSlotEntries = (date, shift) =>
    entries
      .filter(
        (e) =>
          e.date?.split('T')[0] === formatDate(date) &&
          e.shift === shift &&
          (!unitFilter || e.productionUnitId === Number(unitFilter)),
      )
      .sort((a, b) => a.sortOrder - b.sortOrder);

  const handleAddOpen = (date, shift) => {
    setAddSlot({ date: formatDate(date), shift });
    setAddOpen(true);
  };

  const handleStatusChange = (entry, newStatus) => {
    statusMutation.mutate(
      { id: entry.id, data: { status: newStatus } },
      {
        onSuccess: () => toast.success(`Status updated to ${newStatus}`),
        onError: (err) =>
          toast.error(err.response?.data?.message || 'Failed to update status'),
      },
    );
  };

  const handleDelete = () => {
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success('Schedule entry deleted');
        setDeleteTarget(null);
      },
      onError: (err) =>
        toast.error(err.response?.data?.message || 'Failed to delete'),
    });
  };

  const handleMoveInSlot = (entry, direction, slotEntries) => {
    const currentIdx = slotEntries.findIndex((e) => e.id === entry.id);
    const swapIdx = currentIdx + direction;
    if (swapIdx < 0 || swapIdx >= slotEntries.length) return;

    const reordered = [...slotEntries];
    [reordered[currentIdx], reordered[swapIdx]] = [reordered[swapIdx], reordered[currentIdx]];

    reorderMutation.mutate(
      {
        date: entry.date?.split('T')[0],
        shift: entry.shift,
        productionUnitId: entry.productionUnitId,
        scheduleIds: reordered.map((e) => e.id),
      },
      {
        onError: (err) =>
          toast.error(err.response?.data?.message || 'Failed to reorder'),
      },
    );
  };

  const weekLabel = `${formatShortDate(start)} — ${formatShortDate(end)}`;

  return (
    <>
      <PageHeader title="Production Schedule" />

      {/* Week Navigation */}
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1.5}
        sx={{ mb: 2, alignItems: 'center' }}
      >
        <Stack direction="row" spacing={0.5} alignItems="center">
          <IconButton onClick={() => navigateWeek(-1)}>
            <ChevronLeft />
          </IconButton>
          <Typography variant="subtitle1" fontWeight={600} sx={{ minWidth: 220, textAlign: 'center' }}>
            {weekLabel}
          </Typography>
          <IconButton onClick={() => navigateWeek(1)}>
            <ChevronRight />
          </IconButton>
          <Tooltip title="Go to today">
            <IconButton onClick={goToday}>
              <Today />
            </IconButton>
          </Tooltip>
        </Stack>

        <TextField
          select
          size="small"
          label="Unit"
          value={unitFilter}
          onChange={(e) => setUnitFilter(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">All Units</MenuItem>
          {units.map((u) => (
            <MenuItem key={u.id} value={u.id}>
              {u.name}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Box sx={{ overflowX: 'auto' }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: '80px repeat(7, 1fr)', gap: 0.5, minWidth: 900 }}>
            {/* Header row */}
            <Box />
            {days.map((d) => {
              const isToday = formatDate(d) === formatDate(new Date());
              return (
                <Box
                  key={formatDate(d)}
                  sx={{
                    p: 1,
                    textAlign: 'center',
                    bgcolor: isToday ? 'primary.main' : 'grey.100',
                    color: isToday ? 'primary.contrastText' : 'text.primary',
                    borderRadius: 1,
                    fontWeight: 600,
                    fontSize: '0.85rem',
                  }}
                >
                  {formatShortDate(d)}
                </Box>
              );
            })}

            {/* Shift rows */}
            {SHIFTS.map((shift) => (
              <Box key={shift} sx={{ display: 'contents' }}>
                <Box
                  sx={{
                    p: 1,
                    display: 'flex',
                    alignItems: 'flex-start',
                    justifyContent: 'center',
                    fontWeight: 600,
                    fontSize: '0.8rem',
                    bgcolor: 'grey.50',
                    borderRadius: 1,
                  }}
                >
                  {shift}
                </Box>
                {days.map((d) => {
                  const slotItems = getSlotEntries(d, shift);
                  return (
                    <Box
                      key={`${formatDate(d)}-${shift}`}
                      sx={{
                        minHeight: 100,
                        p: 0.5,
                        border: '1px solid',
                        borderColor: 'divider',
                        borderRadius: 1,
                        bgcolor: 'background.paper',
                      }}
                    >
                      {slotItems.map((entry, idx) => (
                        <ScheduleCard
                          key={entry.id}
                          entry={entry}
                          isFirst={idx === 0}
                          isLast={idx === slotItems.length - 1}
                          onStatusChange={handleStatusChange}
                          onDelete={(e) => setDeleteTarget(e)}
                          onMove={(dir) => handleMoveInSlot(entry, dir, slotItems)}
                        />
                      ))}
                      <Button
                        size="small"
                        sx={{ mt: 0.5, fontSize: '0.7rem', minWidth: 0, width: '100%' }}
                        onClick={() => handleAddOpen(d, shift)}
                      >
                        <Add fontSize="small" /> Assign
                      </Button>
                    </Box>
                  );
                })}
              </Box>
            ))}
          </Box>
        </Box>
      )}

      {/* Add Dialog */}
      <AddScheduleDialog
        open={addOpen}
        slot={addSlot}
        units={units}
        pwoOptions={pwoOptions}
        onClose={() => setAddOpen(false)}
        onSave={(data) => {
          createMutation.mutate(data, {
            onSuccess: () => {
              toast.success('Schedule entry created');
              setAddOpen(false);
            },
            onError: (err) =>
              toast.error(err.response?.data?.message || 'Failed to create entry'),
          });
        }}
        saving={createMutation.isPending}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Schedule Entry"
        message={`Remove "${deleteTarget?.pwoNumber}" from this slot?`}
        confirmText="Delete"
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}

function ScheduleCard({ entry, isFirst, isLast, onStatusChange, onDelete, onMove }) {
  const nextStatuses = STATUS_NEXT[entry.status] ?? [];

  return (
    <Card
      variant="outlined"
      sx={{
        mb: 0.5,
        '&:hover .schedule-actions': { opacity: 1 },
      }}
    >
      <CardContent sx={{ p: 1, '&:last-child': { pb: 1 } }}>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography variant="body2" fontWeight={600} noWrap>
              {entry.pwoNumber}
            </Typography>
            <Typography variant="caption" color="text.secondary" noWrap>
              {entry.customerName}
            </Typography>
            {entry.productionUnitName && (
              <Typography variant="caption" display="block" color="text.secondary">
                {entry.productionUnitName}
              </Typography>
            )}
          </Box>
          <StatusChip status={entry.status} size="small" />
        </Stack>

        <Stack
          className="schedule-actions"
          direction="row"
          spacing={0}
          sx={{ mt: 0.5, opacity: 0, transition: 'opacity 0.2s' }}
        >
          {!isFirst && (
            <Tooltip title="Move up">
              <IconButton size="small" onClick={() => onMove(-1)}>
                <ArrowUpward sx={{ fontSize: 14 }} />
              </IconButton>
            </Tooltip>
          )}
          {!isLast && (
            <Tooltip title="Move down">
              <IconButton size="small" onClick={() => onMove(1)}>
                <ArrowDownward sx={{ fontSize: 14 }} />
              </IconButton>
            </Tooltip>
          )}
          {nextStatuses.map((s) => (
            <Tooltip key={s} title={`Mark ${s}`}>
              <Chip
                label={s}
                size="small"
                variant="outlined"
                onClick={() => onStatusChange(entry, s)}
                sx={{ fontSize: '0.65rem', height: 20, cursor: 'pointer' }}
              />
            </Tooltip>
          ))}
          <Tooltip title="Delete">
            <IconButton size="small" color="error" onClick={() => onDelete(entry)}>
              <Delete sx={{ fontSize: 14 }} />
            </IconButton>
          </Tooltip>
        </Stack>
      </CardContent>
    </Card>
  );
}

function AddScheduleDialog({ open, slot, units, pwoOptions, onClose, onSave, saving }) {
  const [pwoId, setPwoId] = useState(null);
  const [unitId, setUnitId] = useState('');
  const [notes, setNotes] = useState('');

  const handleSave = () => {
    if (!pwoId || !unitId) {
      toast.error('Select a PWO and Production Unit');
      return;
    }
    onSave({
      date: slot?.date,
      shift: slot?.shift,
      productionWorkOrderId: pwoId,
      productionUnitId: unitId,
      notes: notes || null,
    });
  };

  const handleClose = () => {
    setPwoId(null);
    setUnitId('');
    setNotes('');
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        Assign PWO — {slot?.date} {slot?.shift}
      </DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          <Grid size={12}>
            <Autocomplete
              size="small"
              options={pwoOptions}
              getOptionLabel={(o) => o.name ?? `PWO #${o.id}`}
              value={pwoOptions.find((p) => p.id === pwoId) ?? null}
              onChange={(_, val) => setPwoId(val?.id ?? null)}
              renderInput={(params) => (
                <TextField {...params} label="Production Work Order *" />
              )}
              isOptionEqualToValue={(opt, val) => opt.id === val.id}
            />
          </Grid>
          <Grid size={12}>
            <TextField
              select
              size="small"
              label="Production Unit *"
              value={unitId}
              onChange={(e) => setUnitId(Number(e.target.value))}
              fullWidth
            >
              {units.map((u) => (
                <MenuItem key={u.id} value={u.id}>
                  {u.name}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={12}>
            <TextField
              size="small"
              label="Notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              fullWidth
              multiline
              rows={2}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button variant="contained" onClick={handleSave} disabled={saving}>
          {saving ? <CircularProgress size={20} /> : 'Assign'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
