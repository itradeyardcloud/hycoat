import { useMemo, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Grid,
  Paper,
  Stack,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import HelpCenterIcon from '@mui/icons-material/HelpCenter';
import CalculateIcon from '@mui/icons-material/Calculate';
import PeopleIcon from '@mui/icons-material/People';
import BoltIcon from '@mui/icons-material/Bolt';
import PageHeader from '../components/common/PageHeader';
import { ORDER_CYCLE_STAGES, PURCHASE_BRANCH, DEPARTMENT_COLORS } from '../data/orderCycleData';

const GUIDE_SECTIONS = [
  { id: 'quick-start', label: 'Quick Start' },
  { id: 'workflow', label: 'Order Workflow' },
  { id: 'roles', label: 'Role View' },
  { id: 'calculator', label: 'SFT Calculator' },
  { id: 'faq', label: 'FAQ' },
];

const QUICK_START_ITEMS = [
  {
    title: 'Step 1: Start from Dashboard',
    body: 'Check pending actions, new notifications, and today\'s priorities.',
    path: '/dashboard',
    output: 'Clear view of what needs attention now.',
  },
  {
    title: 'Step 2: Follow the Order Cycle',
    body: 'Use the guided process from Inquiry to Invoice so teams stay synchronized.',
    path: '/order-cycle',
    output: 'No missed handoffs across departments.',
  },
  {
    title: 'Step 3: Execute Your Department Work',
    body: 'Open your department module and complete records in sequence.',
    path: '/notifications',
    output: 'Live status updates and traceable progress.',
  },
];

const ROLE_GUIDE = {
  All: [
    { label: 'Dashboard', path: '/dashboard', when: 'Start of day', output: 'Pending actions and status overview' },
    { label: 'Order Cycle', path: '/order-cycle', when: 'Need process clarity', output: 'Visual stage-by-stage flow' },
    { label: 'Notifications', path: '/notifications', when: 'Need instant updates', output: 'Real-time alerts and reminders' },
  ],
  Admin: [
    { label: 'Masters', path: '/masters/customers', when: 'Setup or correction', output: 'Reliable master data for all teams' },
    { label: 'Reports', path: '/reports', when: 'Review business performance', output: 'Operational and quality analytics' },
    { label: 'Audit Logs', path: '/admin/audit-logs', when: 'Track user activity', output: 'Compliance and traceability records' },
  ],
  Leader: [
    { label: 'Reports', path: '/reports', when: 'Daily review', output: 'Bottlenecks and trend visibility' },
    { label: 'Masters', path: '/masters/customers', when: 'Master governance', output: 'Standardized process inputs' },
    { label: 'Order Cycle', path: '/order-cycle', when: 'Cross-team alignment', output: 'Shared operating sequence' },
  ],
  User: [
    { label: 'Notifications', path: '/notifications', when: 'During execution', output: 'Know what is pending or approved' },
    { label: 'Order Cycle', path: '/order-cycle', when: 'Need next step', output: 'Clear handoff and dependency guidance' },
    { label: 'Dashboard', path: '/dashboard', when: 'Task prioritization', output: 'Fast daily planning' },
  ],
};

const FAQ_ITEMS = [
  {
    q: 'How do I know what to do next after creating a record?',
    a: 'Use Order Cycle and Notifications together. Order Cycle shows sequence; Notifications tells you when your next action is ready.',
  },
  {
    q: 'What is the central document in Hycoat?',
    a: 'Work Order (WO) is the central reference linking sales, planning, production, quality, dispatch, and invoicing.',
  },
  {
    q: 'Where do I find quality proof for dispatch?',
    a: 'Use Test Certificates from Quality module. It contains DFT, adhesion, MEK and other checks needed before dispatch.',
  },
  {
    q: 'Who handles powder availability?',
    a: 'PPC raises powder indent and Purchase completes PO, GRN, and stock update in parallel before production starts.',
  },
];

function SectionContainer({ id, title, subtitle, icon: Icon, children }) {
  return (
    <Paper
      id={id}
      variant="outlined"
      sx={{
        mb: 3,
        borderRadius: 3,
        overflow: 'hidden',
        borderColor: 'divider',
      }}
    >
      <Box
        sx={{
          p: { xs: 2, md: 2.5 },
          background: 'linear-gradient(120deg, rgba(2,136,209,0.08), rgba(0,150,136,0.08))',
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Stack direction="row" spacing={1.25} alignItems="center">
          <Icon color="primary" fontSize="small" />
          <Box>
            <Typography variant="h6" fontWeight={700}>
              {title}
            </Typography>
            {subtitle && (
              <Typography variant="body2" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Box>
        </Stack>
      </Box>
      <Box sx={{ p: { xs: 2, md: 2.5 } }}>{children}</Box>
    </Paper>
  );
}

export default function GuidePage() {
  const [currentRole, setCurrentRole] = useState('All');
  const [selectedStageId, setSelectedStageId] = useState(ORDER_CYCLE_STAGES[0]?.id ?? null);
  const [perimeter, setPerimeter] = useState('120');
  const [length, setLength] = useState('10');
  const [quantity, setQuantity] = useState('50');
  const [activeFaq, setActiveFaq] = useState(0);

  const orderedStages = useMemo(
    () => [...ORDER_CYCLE_STAGES].sort((a, b) => a.order - b.order),
    []
  );

  const selectedStage = useMemo(() => {
    if (selectedStageId === PURCHASE_BRANCH.id) {
      return PURCHASE_BRANCH;
    }
    return orderedStages.find((stage) => stage.id === selectedStageId) ?? orderedStages[0] ?? null;
  }, [orderedStages, selectedStageId]);

  const sftValue = useMemo(() => {
    const p = Number(perimeter);
    const l = Number(length);
    const q = Number(quantity);

    if (![p, l, q].every((n) => Number.isFinite(n) && n >= 0)) {
      return null;
    }

    return p * l * q;
  }, [perimeter, length, quantity]);

  const scrollToSection = (sectionId) => {
    const section = document.getElementById(sectionId);
    if (section) {
      section.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  };

  return (
    <Box>
      <PageHeader
        title="How to Use Hycoat System"
        subtitle="Interactive, beginner-friendly guide for Admin, Leader, and User roles"
      />

      <Alert severity="info" sx={{ mb: 3, borderRadius: 2 }}>
        Simple rule: follow the order sequence, complete required records in your module, and use notifications for next actions.
      </Alert>

      <Paper
        variant="outlined"
        sx={{
          position: 'sticky',
          top: 8,
          zIndex: 5,
          mb: 3,
          borderRadius: 2,
          backdropFilter: 'blur(4px)',
          backgroundColor: 'rgba(255,255,255,0.88)',
        }}
      >
        <Tabs
          value={false}
          variant="scrollable"
          scrollButtons="auto"
          sx={{ minHeight: 44 }}
        >
          {GUIDE_SECTIONS.map((section) => (
            <Tab
              key={section.id}
              label={section.label}
              onClick={() => scrollToSection(section.id)}
              sx={{ minHeight: 44 }}
            />
          ))}
        </Tabs>
      </Paper>

      <SectionContainer
        id="quick-start"
        icon={BoltIcon}
        title="Quick Start"
        subtitle="Use these three steps to begin work in the app every day"
      >
        <Grid container spacing={2}>
          {QUICK_START_ITEMS.map((item) => (
            <Grid item xs={12} md={4} key={item.title}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 1 }}>
                    {item.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                    {item.body}
                  </Typography>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
                    Output: {item.output}
                  </Typography>
                  <Button
                    component={RouterLink}
                    to={item.path}
                    endIcon={<OpenInNewIcon fontSize="small" />}
                    size="small"
                  >
                    Open
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </SectionContainer>

      <SectionContainer
        id="workflow"
        icon={MenuBookIcon}
        title="Order Workflow"
        subtitle="Click any stage to see what it does, expected output, and where to act in the app"
      >
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
          {orderedStages.map((stage) => (
            <Chip
              key={stage.id}
              label={`${stage.order}. ${stage.shortName}`}
              onClick={() => setSelectedStageId(stage.id)}
              color={selectedStageId === stage.id ? 'primary' : 'default'}
              variant={selectedStageId === stage.id ? 'filled' : 'outlined'}
              sx={{ fontWeight: 600 }}
            />
          ))}
          <Chip
            label="Parallel: Purchase"
            onClick={() => setSelectedStageId(PURCHASE_BRANCH.id)}
            color={selectedStageId === PURCHASE_BRANCH.id ? 'primary' : 'default'}
            variant={selectedStageId === PURCHASE_BRANCH.id ? 'filled' : 'outlined'}
            sx={{ fontWeight: 600 }}
          />
        </Box>

        {selectedStage && (
          <Card variant="outlined" sx={{ borderRadius: 2 }}>
            <CardContent>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} justifyContent="space-between" sx={{ mb: 1.5 }}>
                <Box>
                  <Typography variant="h6" fontWeight={700}>
                    {selectedStage.name}
                  </Typography>
                  <Chip
                    size="small"
                    label={selectedStage.department}
                    sx={{
                      mt: 0.75,
                      bgcolor: `${DEPARTMENT_COLORS[selectedStage.department] ?? '#546e7a'}18`,
                      color: DEPARTMENT_COLORS[selectedStage.department] ?? 'text.primary',
                      fontWeight: 600,
                    }}
                  />
                </Box>
                <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 560 }}>
                  {selectedStage.description}
                </Typography>
              </Stack>

              <Divider sx={{ my: 1.5 }} />

              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" fontWeight={700} sx={{ mb: 1 }}>
                    What happens in this stage
                  </Typography>
                  <Stack spacing={0.75}>
                    {selectedStage.whatHappens?.map((line) => (
                      <Typography key={line} variant="body2" color="text.secondary">
                        • {line}
                      </Typography>
                    ))}
                  </Stack>
                </Grid>

                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" fontWeight={700} sx={{ mb: 1 }}>
                    Expected outputs
                  </Typography>
                  <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ mb: 1.5 }}>
                    {selectedStage.outputs?.map((out) => (
                      <Chip key={out} label={out} size="small" variant="outlined" />
                    ))}
                  </Stack>

                  <Typography variant="subtitle2" fontWeight={700} sx={{ mb: 1 }}>
                    Where to do this in app
                  </Typography>
                  <Stack spacing={1}>
                    {selectedStage.features?.map((feature) => (
                      <Button
                        key={feature.name}
                        component={RouterLink}
                        to={feature.path}
                        variant="outlined"
                        color="inherit"
                        sx={{ justifyContent: 'space-between' }}
                        endIcon={<OpenInNewIcon fontSize="small" />}
                      >
                        <Box sx={{ textAlign: 'left' }}>
                          <Typography variant="body2" fontWeight={700} color="text.primary">
                            {feature.name}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {feature.desc}
                          </Typography>
                        </Box>
                      </Button>
                    ))}
                  </Stack>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        )}
      </SectionContainer>

      <SectionContainer
        id="roles"
        icon={PeopleIcon}
        title="Role View"
        subtitle="Switch role to see the most useful modules and when to use them"
      >
        <Stack direction="row" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
          {Object.keys(ROLE_GUIDE).map((role) => (
            <Chip
              key={role}
              label={role}
              clickable
              onClick={() => setCurrentRole(role)}
              color={currentRole === role ? 'primary' : 'default'}
              variant={currentRole === role ? 'filled' : 'outlined'}
              sx={{ fontWeight: 600 }}
            />
          ))}
        </Stack>

        <Grid container spacing={2}>
          {ROLE_GUIDE[currentRole].map((item) => (
            <Grid item xs={12} md={4} key={`${currentRole}-${item.label}`}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 0.5 }}>
                    {item.label}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    When: {item.when}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                    Output: {item.output}
                  </Typography>
                  <Button
                    component={RouterLink}
                    to={item.path}
                    size="small"
                    endIcon={<OpenInNewIcon fontSize="small" />}
                  >
                    Go to module
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </SectionContainer>

      <SectionContainer
        id="calculator"
        icon={CalculateIcon}
        title="SFT Calculator"
        subtitle="Use this formula to understand area-based pricing and production estimates"
      >
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={3}>
            <TextField
              label="Perimeter"
              value={perimeter}
              onChange={(e) => setPerimeter(e.target.value)}
              fullWidth
              size="small"
              inputProps={{ inputMode: 'decimal' }}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <TextField
              label="Length"
              value={length}
              onChange={(e) => setLength(e.target.value)}
              fullWidth
              size="small"
              inputProps={{ inputMode: 'decimal' }}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <TextField
              label="Quantity"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              fullWidth
              size="small"
              inputProps={{ inputMode: 'numeric' }}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <Paper
              variant="outlined"
              sx={{
                p: 1.5,
                textAlign: 'center',
                borderRadius: 2,
                background: 'linear-gradient(120deg, rgba(0,150,136,0.10), rgba(2,136,209,0.10))',
              }}
            >
              <Typography variant="caption" color="text.secondary">
                Formula
              </Typography>
              <Typography variant="body2" fontWeight={700}>
                SFT = Perimeter × Length × Quantity
              </Typography>
              <Typography variant="h6" fontWeight={800} color="primary.main" sx={{ mt: 0.5 }}>
                {sftValue === null ? 'Invalid input' : sftValue.toLocaleString('en-IN')}
              </Typography>
            </Paper>
          </Grid>
        </Grid>
      </SectionContainer>

      <SectionContainer
        id="faq"
        icon={HelpCenterIcon}
        title="FAQ"
        subtitle="Common questions from new users"
      >
        <Stack spacing={1}>
          {FAQ_ITEMS.map((item, index) => (
            <Card
              key={item.q}
              variant="outlined"
              sx={{ borderRadius: 2, borderColor: activeFaq === index ? 'primary.main' : 'divider' }}
            >
              <CardContent sx={{ p: 1.5 }}>
                <Button
                  onClick={() => setActiveFaq((prev) => (prev === index ? -1 : index))}
                  color="inherit"
                  sx={{ justifyContent: 'space-between', width: '100%' }}
                >
                  <Typography variant="subtitle2" fontWeight={700} textAlign="left">
                    {item.q}
                  </Typography>
                  <Typography variant="subtitle2" fontWeight={700}>
                    {activeFaq === index ? '-' : '+'}
                  </Typography>
                </Button>
                {activeFaq === index && (
                  <Typography variant="body2" color="text.secondary" sx={{ px: 1, pt: 0.5 }}>
                    {item.a}
                  </Typography>
                )}
              </CardContent>
            </Card>
          ))}
        </Stack>
      </SectionContainer>
    </Box>
  );
}
