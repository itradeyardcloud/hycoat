import { useMemo, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Typography,
} from '@mui/material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import EmptyState from '@/components/common/EmptyState';
import {
  useDeleteNotification,
  useMarkAllNotificationsRead,
  useMarkNotificationRead,
  useNotifications,
} from '@/hooks/useNotifications';
import useNotificationStore from '@/stores/notificationStore';
import { getNotificationRoute } from '@/utils/notificationNavigator';
import { useNavigate } from 'react-router-dom';

const CATEGORY_OPTIONS = [
  'Inquiry',
  'Sales',
  'WorkOrder',
  'MaterialInward',
  'QAIncoming',
  'PWO',
  'Production',
  'QAFailure',
  'QAApproval',
  'QARejection',
  'Dispatch',
  'LowStock',
  'Overdue',
  'Invoice',
];

export default function NotificationsPage() {
  const navigate = useNavigate();
  const [isReadFilter, setIsReadFilter] = useState('all');
  const [category, setCategory] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);

  const markReadMutation = useMarkNotificationRead();
  const markAllMutation = useMarkAllNotificationsRead();
  const deleteMutation = useDeleteNotification();

  const markReadInStore = useNotificationStore((s) => s.markRead);
  const markAllInStore = useNotificationStore((s) => s.markAllRead);
  const removeInStore = useNotificationStore((s) => s.removeNotification);

  const queryParams = {
    page: page + 1,
    pageSize: rowsPerPage,
    isRead: isReadFilter === 'all' ? undefined : isReadFilter === 'read',
    category: category || undefined,
  };

  const { data, isLoading } = useNotifications(queryParams);

  const listData = data?.data ?? data;
  const rows = listData?.items ?? [];
  const totalCount = listData?.totalCount ?? 0;

  const columns = useMemo(
    () => [
      {
        field: 'type',
        headerName: 'Type',
        renderCell: (row) => (
          <Chip
            size="small"
            label={row.type}
            color={getTypeColor(row.type)}
            variant="outlined"
          />
        ),
      },
      { field: 'title', headerName: 'Title' },
      { field: 'message', headerName: 'Message' },
      { field: 'category', headerName: 'Category' },
      {
        field: 'isRead',
        headerName: 'Status',
        renderCell: (row) => (
          <Chip size="small" label={row.isRead ? 'Read' : 'Unread'} color={row.isRead ? 'default' : 'primary'} />
        ),
      },
      {
        field: 'createdAt',
        headerName: 'Received',
        renderCell: (row) => formatDateTime(row.createdAt),
      },
      {
        field: 'actions',
        headerName: 'Actions',
        sortable: false,
        renderCell: (row) => (
          <Stack direction="row" spacing={1}>
            {!row.isRead && (
              <Button
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  markReadMutation.mutate(row.id, {
                    onSuccess: () => {
                      markReadInStore(row.id);
                    },
                  });
                }}
              >
                Mark Read
              </Button>
            )}
            <Button
              color="error"
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                deleteMutation.mutate(row.id, {
                  onSuccess: () => {
                    removeInStore(row.id);
                    toast.success('Notification deleted');
                  },
                  onError: () => toast.error('Failed to delete notification'),
                });
              }}
            >
              Delete
            </Button>
          </Stack>
        ),
      },
    ],
    [deleteMutation, markReadInStore, markReadMutation, removeInStore],
  );

  const isEmpty = !isLoading && rows.length === 0;

  return (
    <>
      <PageHeader
        title="Notifications"
        subtitle="Track operational alerts and reminders"
        action={
          <Button
            variant="outlined"
            onClick={() => {
              markAllMutation.mutate(undefined, {
                onSuccess: () => {
                  markAllInStore();
                  toast.success('All notifications marked as read');
                },
                onError: () => toast.error('Failed to mark all as read'),
              });
            }}
            disabled={markAllMutation.isPending}
          >
            Mark All Read
          </Button>
        }
      />

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mb: 2 }}>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel id="read-filter-label">Read Status</InputLabel>
          <Select
            labelId="read-filter-label"
            value={isReadFilter}
            label="Read Status"
            onChange={(e) => {
              setIsReadFilter(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="all">All</MenuItem>
            <MenuItem value="unread">Unread</MenuItem>
            <MenuItem value="read">Read</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 220 }}>
          <InputLabel id="category-filter-label">Category</InputLabel>
          <Select
            labelId="category-filter-label"
            value={category}
            label="Category"
            onChange={(e) => {
              setCategory(e.target.value);
              setPage(0);
            }}
          >
            <MenuItem value="">All Categories</MenuItem>
            {CATEGORY_OPTIONS.map((option) => (
              <MenuItem key={option} value={option}>{option}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Stack>

      {isEmpty ? (
        <EmptyState
          title="No notifications found"
          description="You're all caught up for the current filter set."
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
          onRowClick={(row) => {
            if (!row.isRead) {
              markReadMutation.mutate(row.id, {
                onSuccess: () => markReadInStore(row.id),
              });
            }
            navigate(getNotificationRoute(row.referenceType, row.referenceId));
          }}
        />
      )}
    </>
  );
}

function getTypeColor(type) {
  const normalized = (type || '').toLowerCase();
  if (normalized === 'warning') return 'warning';
  if (normalized === 'alert') return 'error';
  if (normalized === 'success') return 'success';
  return 'info';
}

function formatDateTime(value) {
  if (!value) return '-';
  const dt = new Date(value);
  if (Number.isNaN(dt.getTime())) return '-';
  return dt.toLocaleString();
}
