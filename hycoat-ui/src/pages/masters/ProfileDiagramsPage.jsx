import { useState, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Checkbox,
  TextField,
  IconButton,
  Tooltip,
  Grid,
  Card,
  CardMedia,
  CardContent,
  Typography,
  Chip,
  Dialog,
  DialogContent,
  DialogTitle,
  CircularProgress,
  Divider,
  MenuItem,
  Paper,
  Popper,
  Stack,
  InputAdornment,
} from '@mui/material';
import {
  Add,
  Search,
  Edit,
  Delete,
  PictureAsPdf,
  Download,
  Close,
  ImageNotSupported,
  ZoomIn,
} from '@mui/icons-material';
import toast from 'react-hot-toast';
import PageHeader from '@/components/common/PageHeader';
import DataTable from '@/components/common/DataTable';
import ConfirmDialog from '@/components/common/ConfirmDialog';
import {
  useProfileDiagrams,
  useProfileDiagramSuggest,
  useDeleteProfileDiagram,
} from '@/hooks/useProfileDiagrams';
import profileDiagramService from '@/services/profileDiagramService';
import useDebounce from '@/hooks/useDebounce';


// ── sub-components ────────────────────────────────────────────────────────────

function ImageCard({ diagram, onZoom }) {
  return (
    <Card
      variant="outlined"
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        cursor: 'pointer',
        '&:hover': { boxShadow: 4 },
        transition: 'box-shadow 0.2s',
      }}
      onClick={() => onZoom(diagram)}
    >
      {diagram.imageUrl ? (
        <CardMedia
          component="img"
          image={diagram.imageUrl}
          alt={diagram.code}
          sx={{ height: 200, objectFit: 'contain', p: 1, bgcolor: 'grey.50' }}
        />
      ) : (
        <Box
          sx={{
            height: 200,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            bgcolor: 'grey.100',
          }}
        >
          <ImageNotSupported sx={{ fontSize: 48, color: 'text.disabled' }} />
        </Box>
      )}
      <CardContent sx={{ p: 1.5, flexGrow: 1 }}>
        <Typography variant="subtitle2" fontWeight="bold" noWrap>
          {diagram.code}
        </Typography>
        {diagram.system && (
          <Typography variant="caption" color="text.secondary" display="block" noWrap>
            {diagram.system}
          </Typography>
        )}
        {diagram.category && (
          <Chip label={diagram.category} size="small" sx={{ mt: 0.5, fontSize: 10 }} />
        )}
      </CardContent>
    </Card>
  );
}

function LightboxDialog({ diagram, open, onClose }) {
  if (!diagram) return null;

  const handleDownloadImage = () => {
    if (!diagram.imageUrl) return;
    const a = document.createElement('a');
    a.href = diagram.imageUrl;
    a.download = `${diagram.code}.png`;
    a.target = '_blank';
    a.rel = 'noopener noreferrer';
    a.click();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Typography variant="h6" sx={{ flexGrow: 1 }}>
          {diagram.code}
          {diagram.system && (
            <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
              {diagram.system}
            </Typography>
          )}
        </Typography>
        {diagram.imageUrl && (
          <Tooltip title="Download image">
            <IconButton onClick={handleDownloadImage} size="small">
              <Download />
            </IconButton>
          </Tooltip>
        )}
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent sx={{ p: 2, textAlign: 'center', bgcolor: 'grey.50' }}>
        {diagram.imageUrl ? (
          <img
            src={diagram.imageUrl}
            alt={diagram.code}
            style={{ maxWidth: '100%', maxHeight: '70vh', objectFit: 'contain' }}
          />
        ) : (
          <Box sx={{ py: 8 }}>
            <ImageNotSupported sx={{ fontSize: 64, color: 'text.disabled' }} />
            <Typography color="text.secondary" mt={1}>
              No image uploaded for this profile
            </Typography>
          </Box>
        )}
      </DialogContent>
    </Dialog>
  );
}

// ── main page ─────────────────────────────────────────────────────────────────

export default function ProfileDiagramsPage() {
  const navigate = useNavigate();

  // Lookup state
  const [selectedProfiles, setSelectedProfiles] = useState([]);
  const [suggestInput, setSuggestInput] = useState('');
  const [lightboxDiagram, setLightboxDiagram] = useState(null);
  const [pdfLoading, setPdfLoading] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const searchAnchorRef = useRef(null);

  // Management table state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const debouncedSearch = useDebounce(search, 300);
  const debouncedSuggestInput = useDebounce(suggestInput, 300);

  // Autocomplete suggestions for lookup
  const { data: suggestData, isFetching: suggestLoading } =
    useProfileDiagramSuggest(debouncedSuggestInput);

  const apiSuggestions = suggestData?.data?.items ?? [];

  // When input is empty show already-selected profiles so they are visible on open.
  // When typing, merge selected profiles at the top then API results (deduped).
  const suggestions = useMemo(() => {
    if (!debouncedSuggestInput) return selectedProfiles;
    const selectedIds = new Set(selectedProfiles.map((p) => p.id));
    const extra = apiSuggestions.filter((s) => !selectedIds.has(s.id));
    return [...selectedProfiles, ...extra];
  }, [debouncedSuggestInput, apiSuggestions, selectedProfiles]);

  // Management table query
  const { data: tableData, isLoading: tableLoading } = useProfileDiagrams({
    search: debouncedSearch || undefined,
    page: page + 1,
    pageSize: rowsPerPage,
  });

  const deleteMutation = useDeleteProfileDiagram();

  // ── lookup handlers ───────────────────────────────────────────────────────────────

  const handleDownloadPdf = async () => {
    if (selectedProfiles.length === 0) return;
    const codes = selectedProfiles.map((d) => d.code);
    setPdfLoading(true);
    try {
      const blob = await profileDiagramService.downloadPdf(codes);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download =
        codes.length === 1
          ? `profile-${codes[0]}.pdf`
          : `profiles-${new Date().toISOString().slice(0, 10)}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
      toast.success('PDF downloaded');
    } catch {
      toast.error('Failed to generate PDF');
    } finally {
      setPdfLoading(false);
    }
  };

  // ── management table handlers ─────────────────────────────────────────────

  const handleDelete = () => {
    deleteMutation.mutate(deleteTarget.id, {
      onSuccess: () => {
        toast.success('Profile diagram deleted');
        setDeleteTarget(null);
      },
      onError: () => toast.error('Failed to delete'),
    });
  };

  const columns = useMemo(
    () => [
      { field: 'code', headerName: 'Code', width: 140 },
      { field: 'family', headerName: 'Family', width: 80 },
      { field: 'series', headerName: 'Series', width: 80 },
      { field: 'category', headerName: 'Category' },
      { field: 'system', headerName: 'System' },
      { field: 'sortOrder', headerName: 'Order', width: 70 },
      {
        field: 'imageUrl',
        headerName: 'Image',
        sortable: false,
        width: 70,
        renderCell: (row) =>
          row.imageUrl ? (
            <Tooltip title="View image">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  window.open(row.imageUrl, '_blank', 'noopener,noreferrer');
                }}
              >
                <ZoomIn fontSize="small" color="primary" />
              </IconButton>
            </Tooltip>
          ) : (
            '—'
          ),
      },
      {
        field: 'actions',
        headerName: '',
        sortable: false,
        width: 90,
        renderCell: (row) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="Edit / Upload image">
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/masters/profile-diagrams/${row.id}/edit`);
                }}
              >
                <Edit fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton
                size="small"
                color="error"
                onClick={(e) => {
                  e.stopPropagation();
                  setDeleteTarget(row);
                }}
              >
                <Delete fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        ),
      },
    ],
    [navigate],
  );

  // ─────────────────────────────────────────────────────────────────────────

  return (
    <Box>
      <PageHeader
        title="Profile Diagrams"
        subtitle="Look up aluminium profile technical diagrams by code"
        action={
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => navigate('/masters/profile-diagrams/new')}
          >
            Add Profile
          </Button>
        }
      />

      {/* ── Gallery Lookup ─────────────────────────────────────── */}
      <Box sx={{ mb: 4, p: 3, border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.paper' }}>
        <Typography variant="h6" gutterBottom>
          Lookup Diagrams
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          Search by partial code, family, series or category — select one or more profiles to view
          their diagrams and download a combined PDF.
        </Typography>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5} mt={1.5} alignItems="flex-start">
          <Box ref={searchAnchorRef} sx={{ flex: 1, minWidth: 0 }}>
            <TextField
              fullWidth
              size="small"
              placeholder="Search by code, family, series…"
              value={suggestInput}
              onChange={(e) => setSuggestInput(e.target.value)}
              onFocus={() => setDropdownOpen(true)}
              onBlur={() => setTimeout(() => setDropdownOpen(false), 150)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <Search fontSize="small" />
                  </InputAdornment>
                ),
                endAdornment: suggestLoading ? <CircularProgress size={14} sx={{ mr: 1 }} /> : null,
              }}
            />
            <Popper
              open={dropdownOpen}
              anchorEl={searchAnchorRef.current}
              placement="bottom-start"
              sx={{ zIndex: 1300 }}
              style={{ width: searchAnchorRef.current?.offsetWidth ?? 'auto' }}
            >
              <Paper elevation={4} sx={{ maxHeight: 360, overflowY: 'auto', mt: 0.5 }}>
                {suggestions.length === 0 ? (
                  <Box sx={{ p: 2 }}>
                    <Typography variant="body2" color="text.secondary">
                      {debouncedSuggestInput.length < 1 ? 'Type to search…' : 'No profiles found'}
                    </Typography>
                  </Box>
                ) : (
                  suggestions.map((option) => (
                    <MenuItem
                      key={option.id}
                      dense
                      onMouseDown={(e) => e.preventDefault()}
                      onClick={() =>
                        setSelectedProfiles((prev) =>
                          prev.some((p) => p.id === option.id)
                            ? prev.filter((p) => p.id !== option.id)
                            : [...prev, option],
                        )
                      }
                    >
                      <Checkbox
                        checked={selectedProfiles.some((p) => p.id === option.id)}
                        size="small"
                        sx={{ p: 0.5, mr: 1 }}
                      />
                      <Box>
                        <Typography variant="body2" fontWeight="bold">
                          {option.code}
                        </Typography>
                        {(option.system || option.category) && (
                          <Typography variant="caption" color="text.secondary" display="block">
                            {[option.system, option.category].filter(Boolean).join(' · ')}
                          </Typography>
                        )}
                      </Box>
                    </MenuItem>
                  ))
                )}
              </Paper>
            </Popper>
          </Box>
          {selectedProfiles.length > 0 && (
            <Button
              variant="outlined"
              startIcon={pdfLoading ? <CircularProgress size={16} /> : <PictureAsPdf />}
              onClick={handleDownloadPdf}
              disabled={pdfLoading}
              sx={{ whiteSpace: 'nowrap', minWidth: 160 }}
            >
              Download PDF ({selectedProfiles.length})
            </Button>
          )}
        </Stack>

        {/* Selected chips strip */}
        {selectedProfiles.length > 0 && (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75, mt: 1.5 }}>
            {selectedProfiles.map((profile, index) => (
              <Chip
                key={profile.id}
                label={profile.code}
                size="small"
                onDelete={() => {
                  const next = [...selectedProfiles];
                  next.splice(index, 1);
                  setSelectedProfiles(next);
                }}
              />
            ))}
          </Box>
        )}

        {/* Gallery grid */}
        {selectedProfiles.length > 0 && (
          <Grid container spacing={2} mt={1}>
            {selectedProfiles.map((diagram) => (
              <Grid item xs={12} sm={6} md={4} lg={3} key={diagram.id}>
                <ImageCard diagram={diagram} onZoom={setLightboxDiagram} />
              </Grid>
            ))}
          </Grid>
        )}
      </Box>

      <Divider sx={{ mb: 3 }} />

      {/* ── Management Table ───────────────────────────────────── */}
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h6">All Profiles</Typography>
        <TextField
          size="small"
          placeholder="Search code, family, series…"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(0);
          }}
          sx={{ width: 280 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search fontSize="small" />
              </InputAdornment>
            ),
          }}
        />
      </Box>

      <DataTable
        columns={columns}
        rows={tableData?.data?.items ?? []}
        totalCount={tableData?.data?.totalCount ?? 0}
        page={page}
        rowsPerPage={rowsPerPage}
        onPageChange={setPage}
        onRowsPerPageChange={(rpp) => {
          setRowsPerPage(rpp);
          setPage(0);
        }}
        loading={tableLoading}
      />

      {/* Lightbox */}
      <LightboxDialog
        diagram={lightboxDiagram}
        open={!!lightboxDiagram}
        onClose={() => setLightboxDiagram(null)}
      />

      {/* Confirm delete */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Profile Diagram"
        message={`Delete profile diagram "${deleteTarget?.code}"? This cannot be undone.`}
        onConfirm={handleDelete}
        onCancel={() => setDeleteTarget(null)}
        loading={deleteMutation.isPending}
      />
    </Box>
  );
}
