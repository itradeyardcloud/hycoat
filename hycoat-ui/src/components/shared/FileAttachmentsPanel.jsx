import { useRef } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  IconButton,
  Paper,
  Stack,
  Typography,
} from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import DownloadIcon from '@mui/icons-material/Download';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import toast from 'react-hot-toast';
import { useDeleteFile, useEntityFiles, useUploadFile } from '@/hooks/useFiles';
import { fileService } from '@/services/fileService';
import { downloadBlob } from '@/utils/downloadBlob';

export default function FileAttachmentsPanel({ entityType, entityId, category }) {
  const fileInputRef = useRef(null);

  const { data, isLoading } = useEntityFiles(entityType, entityId);
  const uploadMutation = useUploadFile();
  const deleteMutation = useDeleteFile();

  const listData = data?.data ?? data;
  const files = listData ?? [];

  const handleUpload = (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    uploadMutation.mutate(
      { file, entityType, entityId, category },
      {
        onSuccess: () => toast.success('File uploaded'),
        onError: () => toast.error('Upload failed'),
      },
    );
  };

  const handleDownload = async (file) => {
    try {
      const response = await fileService.download(file.id);
      downloadBlob(response.data, file.fileName);
    } catch {
      toast.error('Failed to download file');
    }
  };

  const handleDelete = (file) => {
    deleteMutation.mutate(
      { id: file.id, entityType, entityId },
      {
        onSuccess: () => toast.success('File deleted'),
        onError: () => toast.error('Delete failed'),
      },
    );
  };

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1.5 }}>
        <Typography variant="subtitle1" fontWeight={600}>Attachments</Typography>
        <Button
          startIcon={<UploadFileIcon />}
          size="small"
          onClick={() => fileInputRef.current?.click()}
          disabled={!entityType || !entityId || uploadMutation.isPending}
        >
          Upload File
        </Button>
      </Stack>

      <input
        ref={fileInputRef}
        type="file"
        hidden
        onChange={handleUpload}
      />

      {isLoading ? (
        <Box sx={{ py: 2, display: 'flex', justifyContent: 'center' }}>
          <CircularProgress size={24} />
        </Box>
      ) : files.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No files uploaded yet.
        </Typography>
      ) : (
        <Stack spacing={1}>
          {files.map((file) => (
            <Paper key={file.id} variant="outlined" sx={{ px: 1.5, py: 1 }}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Box>
                  <Typography variant="body2" fontWeight={600}>{file.fileName}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {file.category || 'General'} - {Math.round((file.fileSizeBytes || 0) / 1024)} KB
                  </Typography>
                </Box>
                <Stack direction="row" spacing={0.5}>
                  <IconButton size="small" onClick={() => handleDownload(file)}>
                    <DownloadIcon fontSize="small" />
                  </IconButton>
                  <IconButton size="small" color="error" onClick={() => handleDelete(file)}>
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </Stack>
              </Stack>
            </Paper>
          ))}
        </Stack>
      )}
    </Paper>
  );
}
