import { useMemo, useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  FormControl,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import EmptyState from '@/components/common/EmptyState';
import useDebounce from '@/hooks/useDebounce';
import { useAuditLogs } from '@/hooks/useAuditLogs';

export default function AuditLogPage() {
  const [entityName, setEntityName] = useState('');
  const [action, setAction] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [selectedLog, setSelectedLog] = useState(null);

  const debouncedSearch = useDebounce(search, 300);

  const { data, isLoading } = useAuditLogs({
    entityName: entityName || undefined,
    action: action || undefined,
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    search: debouncedSearch || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  });

  const listData = data?.data ?? data;
  const rows = listData?.items ?? [];
  const totalCount = listData?.totalCount ?? 0;

  const columns = useMemo(
    () => [
      {
        field: 'timestamp',
        headerName: 'Timestamp',
        renderCell: (row) => formatDateTime(row.timestamp),
      },
      { field: 'userName', headerName: 'User' },
      { field: 'action', headerName: 'Action' },
      { field: 'entityName', headerName: 'Entity' },
      { field: 'entityId', headerName: 'Entity ID' },
      {
        field: 'details',
        headerName: 'Details',
        sortable: false,
        renderCell: (row) => (
          <Button
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              setSelectedLog(row);
            }}
          >
            View Changes
          </Button>
        ),
      },
    ],
    [],
  );

  return (
    <>
      <PageHeader
        title="Audit Trail"
        subtitle="Who changed what, and when"
      />

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mb: 2 }}>
        <TextField
          size="small"
          placeholder="Search entity, user, ID..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ minWidth: 280 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" />
                </InputAdornment>
              ),
            },
          }}
        />

        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel id="entity-label">Entity</InputLabel>
          <Select
            labelId="entity-label"
            value={entityName}
            label="Entity"
            onChange={(e) => {
              setEntityName(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="WorkOrder">WorkOrder</MenuItem>
            <MenuItem value="MaterialInward">MaterialInward</MenuItem>
            <MenuItem value="IncomingInspection">IncomingInspection</MenuItem>
            <MenuItem value="ProductionWorkOrder">ProductionWorkOrder</MenuItem>
            <MenuItem value="FinalInspection">FinalInspection</MenuItem>
            <MenuItem value="DeliveryChallan">DeliveryChallan</MenuItem>
            <MenuItem value="Invoice">Invoice</MenuItem>
            <MenuItem value="Notification">Notification</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel id="action-label">Action</InputLabel>
          <Select
            labelId="action-label"
            value={action}
            label="Action"
            onChange={(e) => {
              setAction(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Create">Create</MenuItem>
            <MenuItem value="Update">Update</MenuItem>
            <MenuItem value="Delete">Delete</MenuItem>
          </Select>
        </FormControl>

        <TextField
          size="small"
          type="date"
          label="From"
          value={dateFrom}
          onChange={(e) => {
            setDateFrom(e.target.value);
            setPage(0);
          }}
          InputLabelProps={{ shrink: true }}
        />

        <TextField
          size="small"
          type="date"
          label="To"
          value={dateTo}
          onChange={(e) => {
            setDateTo(e.target.value);
            setPage(0);
          }}
          InputLabelProps={{ shrink: true }}
        />
      </Stack>

      {!isLoading && rows.length === 0 ? (
        <EmptyState
          title="No audit logs found"
          description="Try changing filters to broaden your search."
        />
      ) : (
        <DataTable
          columns={columns}
          rows={rows}
          loading={isLoading}
          page={page}
          rowsPerPage={rowsPerPage}
          totalCount={totalCount}
          onPageChange={setPage}
          onRowsPerPageChange={(size) => {
            setRowsPerPage(size);
            setPage(0);
          }}
          onRowClick={(row) => setSelectedLog(row)}
        />
      )}

      <Dialog
        open={!!selectedLog}
        onClose={() => setSelectedLog(null)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Change Details</DialogTitle>
        <DialogContent>
          {selectedLog ? (
            <>
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  {selectedLog.entityName} #{selectedLog.entityId} - {selectedLog.action}
                </Typography>
              </Box>

              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Field</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Old Value</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>New Value</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {buildDiffRows(selectedLog).map((item) => (
                    <TableRow key={item.field}>
                      <TableCell>{item.field}</TableCell>
                      <TableCell sx={{ color: 'text.secondary' }}>{item.oldValue}</TableCell>
                      <TableCell>{item.newValue}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </>
          ) : null}
        </DialogContent>
      </Dialog>
    </>
  );
}

function buildDiffRows(log) {
  const oldValues = log.oldValues || {};
  const newValues = log.newValues || {};
  const keys = Array.from(new Set([...Object.keys(oldValues), ...Object.keys(newValues)]));

  if (keys.length === 0) {
    return [{ field: 'No field-level data', oldValue: '-', newValue: '-' }];
  }

  return keys.map((field) => ({
    field,
    oldValue: stringifyValue(oldValues[field]),
    newValue: stringifyValue(newValues[field]),
  }));
}

function stringifyValue(value) {
  if (value === null || value === undefined || value === '') return '-';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

function formatDateTime(value) {
  if (!value) return '-';
  const dt = new Date(value);
  if (Number.isNaN(dt.getTime())) return '-';
  return dt.toLocaleString();
}
