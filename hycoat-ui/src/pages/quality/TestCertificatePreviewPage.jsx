import { useParams, useNavigate } from 'react-router-dom';
import { Box, Button, CircularProgress, Typography, Stack } from '@mui/material';
import { Download, ArrowBack } from '@mui/icons-material';
import PageHeader from '@/components/common/PageHeader';
import { useTestCertificate } from '@/hooks/useTestCertificates';
import { testCertificateService } from '@/services/qualityService';

export default function TestCertificatePreviewPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data, isLoading } = useTestCertificate(id);

  const tc = data?.data;
  const hasPdf = tc?.fileUrl;

  const handleDownload = async () => {
    try {
      const response = await testCertificateService.downloadPdf(id);
      const blob = new Blob([response.data], { type: 'application/pdf' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${tc?.certificateNumber || 'TestCertificate'}.pdf`;
      link.click();
      URL.revokeObjectURL(url);
    } catch {
      // handled silently
    }
  };

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <PageHeader
        title={`Test Certificate — ${tc?.certificateNumber || ''}`}
        action={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<ArrowBack />} onClick={() => navigate('/quality/test-certificates')}>
              Back
            </Button>
            {hasPdf && (
              <Button variant="contained" startIcon={<Download />} onClick={handleDownload}>
                Download PDF
              </Button>
            )}
          </Stack>
        }
      />

      {hasPdf ? (
        <Box
          component="iframe"
          src={tc.fileUrl}
          sx={{ width: '100%', height: 'calc(100vh - 200px)', border: '1px solid', borderColor: 'divider', borderRadius: 1 }}
          title="Test Certificate PDF"
        />
      ) : (
        <Box sx={{ py: 8, textAlign: 'center' }}>
          <Typography color="text.secondary">
            No PDF has been generated yet. Go to the certificate form to generate one.
          </Typography>
          <Button
            variant="outlined"
            sx={{ mt: 2 }}
            onClick={() => navigate(`/quality/test-certificates/${id}`)}
          >
            Edit Certificate
          </Button>
        </Box>
      )}
    </>
  );
}
