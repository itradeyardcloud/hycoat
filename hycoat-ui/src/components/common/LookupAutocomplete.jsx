import { Autocomplete, Button, Divider, Paper } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';

export default function LookupAutocomplete({
  addNewPath,
  addNewLabel = 'Add New',
  PaperComponent,
  ...props
}) {
  if (!addNewPath) {
    return <Autocomplete {...props} PaperComponent={PaperComponent} />;
  }

  const BasePaper = PaperComponent || Paper;

  const ActionPaper = (paperProps) => (
    <BasePaper {...paperProps}>
      {paperProps.children}
      <Divider />
      <Button
        fullWidth
        size="small"
        startIcon={<AddIcon fontSize="small" />}
        sx={{ justifyContent: 'flex-start', borderRadius: 0, py: 1 }}
        onMouseDown={(e) => {
          e.preventDefault();
          e.stopPropagation();
        }}
        onClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
          window.open(addNewPath, '_blank', 'noopener,noreferrer');
        }}
      >
        {addNewLabel}
      </Button>
    </BasePaper>
  );

  return <Autocomplete {...props} PaperComponent={ActionPaper} />;
}
