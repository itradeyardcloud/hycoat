import { Button, CircularProgress } from '@mui/material';
import FileDownloadIcon from '@mui/icons-material/FileDownload';

// eslint-disable-next-line react/prop-types
export default function ExportButton({ onClick, loading = false, label = 'Export to Excel' }) {
  return (
    <Button
      variant="outlined"
      startIcon={loading ? <CircularProgress size={16} /> : <FileDownloadIcon />}
      onClick={onClick}
      disabled={loading}
      size="small"
    >
      {label}
    </Button>
  );
}
