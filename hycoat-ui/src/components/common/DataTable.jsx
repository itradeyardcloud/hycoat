import { useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  TableSortLabel,
  Paper,
  Skeleton,
  Card,
  CardContent,
  CardActionArea,
  Typography,
  Box,
  Stack,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import EmptyState from './EmptyState';

export default function DataTable({
  columns,
  rows,
  loading = false,
  onRowClick,
  page = 0,
  rowsPerPage = 10,
  totalCount = 0,
  onPageChange,
  onRowsPerPageChange,
}) {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const [orderBy, setOrderBy] = useState('');
  const [order, setOrder] = useState('asc');

  const handleSort = (field) => {
    const isAsc = orderBy === field && order === 'asc';
    setOrder(isAsc ? 'desc' : 'asc');
    setOrderBy(field);
  };

  const sortedRows = orderBy
    ? [...rows].sort((a, b) => {
        const aVal = a[orderBy] ?? '';
        const bVal = b[orderBy] ?? '';
        const cmp = String(aVal).localeCompare(String(bVal), undefined, { numeric: true });
        return order === 'asc' ? cmp : -cmp;
      })
    : rows;

  if (loading) {
    return isDesktop ? <DesktopSkeleton columns={columns} /> : <MobileSkeleton />;
  }

  if (!rows || rows.length === 0) {
    return <EmptyState title="No records found" description="There are no items to display." />;
  }

  return (
    <Box>
      {isDesktop ? (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                {columns.map((col) => (
                  <TableCell key={col.field} sx={{ fontWeight: 600 }}>
                    {col.sortable !== false ? (
                      <TableSortLabel
                        active={orderBy === col.field}
                        direction={orderBy === col.field ? order : 'asc'}
                        onClick={() => handleSort(col.field)}
                      >
                        {col.headerName}
                      </TableSortLabel>
                    ) : (
                      col.headerName
                    )}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {sortedRows.map((row, idx) => (
                <TableRow
                  key={row.id ?? idx}
                  hover
                  onClick={() => onRowClick?.(row)}
                  sx={{ cursor: onRowClick ? 'pointer' : 'default' }}
                >
                  {columns.map((col) => (
                    <TableCell key={col.field}>
                      {col.renderCell ? col.renderCell(row) : row[col.field]}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Stack spacing={1.5}>
          {sortedRows.map((row, idx) => (
            <Card key={row.id ?? idx} variant="outlined">
              {onRowClick ? (
                <CardActionArea onClick={() => onRowClick(row)}>
                  <MobileCardContent columns={columns} row={row} />
                </CardActionArea>
              ) : (
                <MobileCardContent columns={columns} row={row} />
              )}
            </Card>
          ))}
        </Stack>
      )}

      {onPageChange && (
        <TablePagination
          component="div"
          count={totalCount}
          page={page}
          onPageChange={(_, newPage) => onPageChange(newPage)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(e) => onRowsPerPageChange?.(parseInt(e.target.value, 10))}
          rowsPerPageOptions={[5, 10, 25, 50]}
        />
      )}
    </Box>
  );
}

function MobileCardContent({ columns, row }) {
  return (
    <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
      {columns.map((col, idx) => (
        <Box key={col.field} sx={{ display: 'flex', justifyContent: 'space-between', py: 0.25 }}>
          <Typography variant="caption" color="text.secondary">
            {col.headerName}
          </Typography>
          <Typography variant="body2" fontWeight={idx === 0 ? 600 : 400}>
            {col.renderCell ? col.renderCell(row) : row[col.field]}
          </Typography>
        </Box>
      ))}
    </CardContent>
  );
}

function DesktopSkeleton({ columns }) {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table size="small">
        <TableHead>
          <TableRow>
            {columns.map((col) => (
              <TableCell key={col.field} sx={{ fontWeight: 600 }}>
                {col.headerName}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {Array.from({ length: 5 }).map((_, i) => (
            <TableRow key={i}>
              {columns.map((col) => (
                <TableCell key={col.field}>
                  <Skeleton variant="text" />
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

function MobileSkeleton() {
  return (
    <Stack spacing={1.5}>
      {Array.from({ length: 5 }).map((_, i) => (
        <Card key={i} variant="outlined">
          <CardContent>
            <Skeleton variant="text" width="60%" />
            <Skeleton variant="text" width="40%" />
            <Skeleton variant="text" width="50%" />
          </CardContent>
        </Card>
      ))}
    </Stack>
  );
}
