import { useMemo, useState } from 'react';
import {
  Box,
  Grid,
  TextField,
  MenuItem,
  Button,
  Stack,
  Paper,
  Typography,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import KPICard from '@/components/dashboard/KPICard';
import { useYieldReports, useYieldSummary, useCreateYieldReport } from '@/hooks/useYieldReports';
import { useProductionUnits } from '@/hooks/useProductionUnits';
import { formatCurrency, formatDate, formatNumber } from '@/utils/formatters';

function toInputDate(date) {
  return date.toISOString().split('T')[0];
}

const defaultForm = {
  date: toInputDate(new Date()),
  shift: 'Day',
  productionUnitId: '',
  productionSFT: '',
  rejectionSFT: '',
  electricityOpeningKwh: '',
  electricityClosingKwh: '',
  electricityRatePerKwh: '',
  ovenGasOpeningReading: '',
  ovenGasClosingReading: '',
  ovenGasRatePerUnit: '',
  powderUsedKg: '',
  powderRatePerKg: '',
  manpowerCost: '',
  otherCost: '',
  sellingPricePerSFT: '',
  remarks: '',
};

export default function YieldReportPage() {
  const [filters, setFilters] = useState({
    dateFrom: toInputDate(new Date()),
    dateTo: toInputDate(new Date()),
    productionUnitId: '',
    page: 0,
    rowsPerPage: 20,
  });

  const [form, setForm] = useState(defaultForm);

  const { data: unitsRes } = useProductionUnits();
  const unitOptions = unitsRes?.data ?? [];

  const listParams = {
    dateFrom: filters.dateFrom || undefined,
    dateTo: filters.dateTo || undefined,
    productionUnitId: filters.productionUnitId || undefined,
    page: filters.page + 1,
    pageSize: filters.rowsPerPage,
  };

  const { data: listRes, isLoading } = useYieldReports(listParams);
  const { data: summaryRes } = useYieldSummary({ date: filters.dateTo || filters.dateFrom || undefined });
  const createMutation = useCreateYieldReport();

  const rows = listRes?.data?.items ?? [];
  const totalCount = listRes?.data?.totalCount ?? 0;
  const summary = summaryRes?.data;

  const columns = useMemo(
    () => [
      { field: 'date', headerName: 'Date', renderCell: (row) => formatDate(row.date) },
      { field: 'productionUnitName', headerName: 'Unit' },
      { field: 'shift', headerName: 'Shift' },
      { field: 'productionSFT', headerName: 'Prod SFT', renderCell: (row) => formatNumber(row.productionSFT) },
      { field: 'rejectionSFT', headerName: 'Reject SFT', renderCell: (row) => formatNumber(row.rejectionSFT) },
      { field: 'netProductionSFT', headerName: 'Net SFT', renderCell: (row) => formatNumber(row.netProductionSFT) },
      {
        field: 'yieldPercent',
        headerName: 'Yield %',
        renderCell: (row) => `${Number(row.yieldPercent || 0).toFixed(2)}%`,
      },
      {
        field: 'electricityConsumedKwh',
        headerName: 'Power Used (kWh)',
        renderCell: (row) => formatNumber(row.electricityConsumedKwh),
      },
      {
        field: 'ovenGasConsumedUnits',
        headerName: 'Gas Used',
        renderCell: (row) => formatNumber(row.ovenGasConsumedUnits),
      },
      { field: 'totalCost', headerName: 'Total Cost', renderCell: (row) => formatCurrency(row.totalCost) },
      { field: 'costPerSFT', headerName: 'Cost/SFT', renderCell: (row) => formatCurrency(row.costPerSFT) },
      { field: 'revenue', headerName: 'Revenue', renderCell: (row) => formatCurrency(row.revenue) },
      { field: 'profit', headerName: 'Profit', renderCell: (row) => formatCurrency(row.profit) },
      {
        field: 'roiPercent',
        headerName: 'ROI %',
        renderCell: (row) => `${Number(row.roiPercent || 0).toFixed(2)}%`,
      },
    ],
    [],
  );

  const onFormChange = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const toNumber = (v) => (v === '' || v === null || v === undefined ? 0 : Number(v));

  const handleCreate = () => {
    if (!form.productionUnitId) {
      toast.error('Please select a production unit');
      return;
    }

    const payload = {
      date: form.date,
      shift: form.shift,
      productionUnitId: Number(form.productionUnitId),
      productionSFT: toNumber(form.productionSFT),
      rejectionSFT: toNumber(form.rejectionSFT),
      electricityOpeningKwh: toNumber(form.electricityOpeningKwh),
      electricityClosingKwh: toNumber(form.electricityClosingKwh),
      electricityRatePerKwh: toNumber(form.electricityRatePerKwh),
      ovenGasOpeningReading: toNumber(form.ovenGasOpeningReading),
      ovenGasClosingReading: toNumber(form.ovenGasClosingReading),
      ovenGasRatePerUnit: toNumber(form.ovenGasRatePerUnit),
      powderUsedKg: toNumber(form.powderUsedKg),
      powderRatePerKg: toNumber(form.powderRatePerKg),
      manpowerCost: toNumber(form.manpowerCost),
      otherCost: toNumber(form.otherCost),
      sellingPricePerSFT: toNumber(form.sellingPricePerSFT),
      remarks: form.remarks || null,
    };

    createMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('Yield report saved');
        setForm((prev) => ({
          ...defaultForm,
          date: prev.date,
          productionUnitId: prev.productionUnitId,
          shift: prev.shift,
        }));
      },
      onError: (err) => {
        const msg =
          err.response?.data?.errors?.[0] ||
          err.response?.data?.message ||
          'Failed to save yield report';
        toast.error(msg);
      },
    });
  };

  return (
    <Box>
      <PageHeader title="Daily Yield ROI Report" />

      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, sm: 6, md: 2.5 }}>
          <TextField
            fullWidth
            size="small"
            type="date"
            label="From"
            value={filters.dateFrom}
            onChange={(e) => setFilters((p) => ({ ...p, dateFrom: e.target.value, page: 0 }))}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2.5 }}>
          <TextField
            fullWidth
            size="small"
            type="date"
            label="To"
            value={filters.dateTo}
            onChange={(e) => setFilters((p) => ({ ...p, dateTo: e.target.value, page: 0 }))}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <TextField
            fullWidth
            size="small"
            select
            label="Production Unit"
            value={filters.productionUnitId}
            onChange={(e) => setFilters((p) => ({ ...p, productionUnitId: e.target.value, page: 0 }))}
          >
            <MenuItem value="">All Units</MenuItem>
            {unitOptions.map((u) => (
              <MenuItem key={u.id} value={u.id}>{u.name}</MenuItem>
            ))}
          </TextField>
        </Grid>
      </Grid>

      {summary && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid size={{ xs: 6, md: 2 }}><KPICard title="Net SFT" value={formatNumber(summary.totalNetProductionSFT)} /></Grid>
          <Grid size={{ xs: 6, md: 2 }}><KPICard title="Yield %" value={`${Number(summary.yieldPercent || 0).toFixed(2)}%`} /></Grid>
          <Grid size={{ xs: 6, md: 2 }}><KPICard title="Cost / SFT" value={formatCurrency(summary.costPerSFT)} /></Grid>
          <Grid size={{ xs: 6, md: 2 }}><KPICard title="ROI %" value={`${Number(summary.roiPercent || 0).toFixed(2)}%`} /></Grid>
          <Grid size={{ xs: 6, md: 2 }}><KPICard title="Total Cost" value={formatCurrency(summary.totalCost)} /></Grid>
          <Grid size={{ xs: 6, md: 2 }}><KPICard title="Profit" value={formatCurrency(summary.profit)} /></Grid>
        </Grid>
      )}

      <Paper variant="outlined" sx={{ p: 2, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 1.5 }}>Add Daily Yield Entry</Typography>

        <Grid container spacing={1.5}>
          <Grid size={{ xs: 12, sm: 4, md: 2 }}>
            <TextField
              fullWidth
              size="small"
              type="date"
              label="Date"
              value={form.date}
              onChange={(e) => onFormChange('date', e.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4, md: 2 }}>
            <TextField
              fullWidth
              size="small"
              select
              label="Shift"
              value={form.shift}
              onChange={(e) => onFormChange('shift', e.target.value)}
            >
              <MenuItem value="Day">Day</MenuItem>
              <MenuItem value="Night">Night</MenuItem>
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 4, md: 3 }}>
            <TextField
              fullWidth
              size="small"
              select
              label="Production Unit"
              value={form.productionUnitId}
              onChange={(e) => onFormChange('productionUnitId', e.target.value)}
            >
              {unitOptions.map((u) => (
                <MenuItem key={u.id} value={u.id}>{u.name}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 6, md: 2.5 }}>
            <TextField fullWidth size="small" type="number" label="Production SFT" value={form.productionSFT} onChange={(e) => onFormChange('productionSFT', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.5 }}>
            <TextField fullWidth size="small" type="number" label="Rejection SFT" value={form.rejectionSFT} onChange={(e) => onFormChange('rejectionSFT', e.target.value)} />
          </Grid>

          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Power Open (kWh)" value={form.electricityOpeningKwh} onChange={(e) => onFormChange('electricityOpeningKwh', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Power Close (kWh)" value={form.electricityClosingKwh} onChange={(e) => onFormChange('electricityClosingKwh', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Power Rate" value={form.electricityRatePerKwh} onChange={(e) => onFormChange('electricityRatePerKwh', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Gas Open" value={form.ovenGasOpeningReading} onChange={(e) => onFormChange('ovenGasOpeningReading', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Gas Close" value={form.ovenGasClosingReading} onChange={(e) => onFormChange('ovenGasClosingReading', e.target.value)} />
          </Grid>

          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Gas Rate" value={form.ovenGasRatePerUnit} onChange={(e) => onFormChange('ovenGasRatePerUnit', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Powder Used (kg)" value={form.powderUsedKg} onChange={(e) => onFormChange('powderUsedKg', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Powder Rate" value={form.powderRatePerKg} onChange={(e) => onFormChange('powderRatePerKg', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Manpower Cost" value={form.manpowerCost} onChange={(e) => onFormChange('manpowerCost', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 6, md: 2.4 }}>
            <TextField fullWidth size="small" type="number" label="Other Cost" value={form.otherCost} onChange={(e) => onFormChange('otherCost', e.target.value)} />
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <TextField fullWidth size="small" type="number" label="Selling Price / SFT" value={form.sellingPricePerSFT} onChange={(e) => onFormChange('sellingPricePerSFT', e.target.value)} />
          </Grid>
          <Grid size={{ xs: 12, md: 9 }}>
            <TextField fullWidth size="small" label="Remarks" value={form.remarks} onChange={(e) => onFormChange('remarks', e.target.value)} />
          </Grid>
        </Grid>

        <Stack direction="row" justifyContent="flex-end" sx={{ mt: 2 }}>
          <Button variant="contained" onClick={handleCreate} disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Saving...' : 'Save Yield Entry'}
          </Button>
        </Stack>
      </Paper>

      <DataTable
        columns={columns}
        rows={rows}
        loading={isLoading}
        page={filters.page}
        rowsPerPage={filters.rowsPerPage}
        totalCount={totalCount}
        onPageChange={(p) => setFilters((prev) => ({ ...prev, page: p }))}
        onRowsPerPageChange={(size) => setFilters((prev) => ({ ...prev, rowsPerPage: size, page: 0 }))}
      />
    </Box>
  );
}