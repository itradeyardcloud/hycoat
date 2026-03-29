import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import DescriptionIcon from '@mui/icons-material/Description';
import InventoryIcon from '@mui/icons-material/Inventory';
import EventNoteIcon from '@mui/icons-material/EventNote';
import PrecisionManufacturingIcon from '@mui/icons-material/PrecisionManufacturing';
import VerifiedIcon from '@mui/icons-material/Verified';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import StoreIcon from '@mui/icons-material/Store';

// Department color mapping
export const DEPARTMENT_COLORS = {
  Sales: '#1565c0',
  SCM: '#00897b',
  PPC: '#ef6c00',
  Production: '#7b1fa2',
  QA: '#2e7d32',
  Purchase: '#6d4c41',
  Finance: '#283593',
};

export const ORDER_CYCLE_STAGES = [
  {
    id: 'inquiry',
    order: 1,
    name: 'Inquiry & Quotation',
    shortName: 'Inquiry',
    department: 'Sales',
    icon: ShoppingCartIcon,
    description:
      'Customer reaches out via email, phone, or WhatsApp. Sales team evaluates the requirement — material specs, profile drawings, process type, color — and prepares a quotation with rates per square foot.',
    features: [
      { name: 'Inquiries', path: '/sales/inquiries', desc: 'Log and track customer inquiries with status flow (New → Quoted → Won/Lost)' },
      { name: 'Quotations', path: '/sales/quotations', desc: 'Generate quotations with process rates, packing & transport charges, and PDF export' },
    ],
    outputs: ['Inquiry record', 'Quotation PDF'],
    whatHappens: [
      'Receive inquiry — log customer name, contact, project, date',
      'Evaluate material specs, profile drawings, process & color requirements',
      'Calculate coating area from perimeter × length × quantity',
      'Send quotation with generic rates per process type',
    ],
  },
  {
    id: 'order-confirmation',
    order: 2,
    name: 'Order Confirmation',
    shortName: 'Confirmation',
    department: 'Sales',
    icon: DescriptionIcon,
    description:
      'Customer confirms the order by sending BOM, drawings, and signing the Proforma Invoice. Sales creates the Work Order — the central hub document that ties the entire order lifecycle together.',
    features: [
      { name: 'Proforma Invoices', path: '/sales/proforma-invoices', desc: 'Prepare PI with itemised area calculation, GST breakup, and annexure' },
      { name: 'Work Orders', path: '/sales/work-orders', desc: 'Central hub document tracking the order from creation to dispatch/invoicing' },
    ],
    outputs: ['Proforma Invoice PDF', 'Work Order'],
    whatHappens: [
      'Receive BOM (Bill of Material) — section number, type, length, quantity',
      'Receive profile drawings with perimeter and coating/masking area markings',
      'Calculate total area (SFT) and prepare Proforma Invoice',
      'Customer confirms with signed PI, email, or formal Work Order',
      'Create internal Work Order — handed off to PPC & SCM',
    ],
  },
  {
    id: 'material-receipt',
    order: 3,
    name: 'Material Receipt',
    shortName: 'Inward',
    department: 'SCM',
    icon: InventoryIcon,
    description:
      'Customer dispatches raw aluminum sections to the factory. SCM photographs the vehicle, unloads and counts material, records discrepancies, and QA performs incoming inspection for defects.',
    features: [
      { name: 'Inward Register', path: '/material-inward/inwards', desc: 'Log material receipt with customer DC, quantities, photos, and discrepancy tracking' },
      { name: 'Incoming Inspection', path: '/material-inward/inspections', desc: 'QA checklist for watermark, scratch, dent — Pass/Fail/Conditional with buffing notes' },
    ],
    outputs: ['Inward register entry', 'Incoming inspection report', 'Identification labels'],
    whatHappens: [
      'Photograph vehicle and bundles on arrival',
      'Unload and count material against customer document',
      'Record discrepancy — excess or shortage',
      'QA opens 2-3 bundles, checks for watermarks, scratches, dents',
      'Stick identification label (customer, DC no, qty) on each bundle',
      'Store material in designated area; notify PPC',
    ],
  },
  {
    id: 'production-planning',
    order: 4,
    name: 'Production Planning',
    shortName: 'Planning',
    department: 'PPC',
    icon: EventNoteIcon,
    description:
      'PPC calculates production time based on section dimensions, basket capacity, and conveyor speed. They check powder stock, create the Production Work Order (PWO), and schedule shifts for the week.',
    features: [
      { name: 'Production Work Orders', path: '/ppc/work-orders', desc: 'Create PWOs with time calculation engine, basket plan, shift allocation, and surface area calc' },
      { name: 'Production Schedule', path: '/ppc/schedule', desc: 'Weekly calendar/planner view with day/night shift allocation per customer' },
    ],
    outputs: ['Production Work Order (PWO)', 'Weekly schedule', 'Powder indent to Purchase'],
    whatHappens: [
      'Retrieve drawings — calculate perimeter and coating area per section (SFT)',
      'Check powder stock; raise indent to Purchase if low',
      'Calculate production time — basket capacity, conveyor speed, etch time',
      'Create weekly production schedule — day/night shift allocation',
      'Issue Production Work Order (PWO) to Production, QA, and Purchase teams',
    ],
  },
  {
    id: 'production',
    order: 5,
    name: 'Production',
    shortName: 'Production',
    department: 'Production',
    icon: PrecisionManufacturingIcon,
    description:
      'Two-stage process: Pretreatment (10-tank chemical sequence — degreasing, etching, chrome coating, rinsing) followed by Powder Coating (electrostatic spray, oven curing at 200-230°C). Every step is logged.',
    features: [
      { name: 'Pretreatment Logs', path: '/production/pretreatment', desc: 'Basket-level logging across 10 tanks — etch time, temperatures, chemical concentrations' },
      { name: 'Coating Logs', path: '/production/coating', desc: 'Shift logs with conveyor speed, oven temp, powder batch, and hourly photo upload' },
    ],
    outputs: ['Pretreatment log sheets', 'Production log sheets', 'Timestamped photos'],
    whatHappens: [
      'QA Lab performs morning titration — chemical concentration and etch rate',
      'Operator runs baskets through 10-tank pretreatment sequence',
      'Air spray, hang on conveyor, set speed and oven temperature per PWO',
      'Electrostatic powder spray in booth, conveyor through curing oven',
      'Cool down, unload, and supervisor performs line inspection (DFT, gloss, shade)',
      'Record everything in log sheets with hourly photo uploads',
    ],
  },
  {
    id: 'quality',
    order: 6,
    name: 'Quality Control',
    shortName: 'Quality',
    department: 'QA',
    icon: VerifiedIcon,
    description:
      'Three-stage quality process: In-Process inspection every 15-30 min (DFT, adhesion, MEK rub), Panel Testing every 4 hours (boiling water, impact, bend, hardness), and Final Inspection with test certificate generation.',
    features: [
      { name: 'In-Process Inspection', path: '/quality/in-process', desc: 'DFT 10-point readings every 30 min, adhesion/MEK/shade tests every 2 hours' },
      { name: 'Final Inspection', path: '/quality/final', desc: 'AQL batch sampling — visual inspection, DFT re-check, shade match final confirmation' },
      { name: 'Test Certificates', path: '/quality/test-certificates', desc: 'Auto-generated PDF certificates with all test results — DFT, gloss, adhesion, MEK, impact, bend, hardness' },
    ],
    outputs: ['In-process inspection records', 'Panel test results', 'Final inspection report', 'Test Certificate PDF'],
    whatHappens: [
      'Every 15-30 min: DFT check on 5-10 points with photo upload',
      'Every 2 hours: Cross-hatch adhesion test, MEK rub test (30 rubs), shade match',
      'Every 4 hours: Panel testing — boiling water, impact, conical mandrel bend, pencil hardness',
      'After production: AQL batch sampling — visual uniformity, DFT re-check',
      'PASS → Generate Test Certificate PDF; FAIL → Rework or scrap',
    ],
  },
  {
    id: 'dispatch',
    order: 7,
    name: 'Dispatch',
    shortName: 'Dispatch',
    department: 'SCM',
    icon: LocalShippingIcon,
    description:
      'After final inspection approval, sections are taped, bundled, and labeled. SCM prepares the full dispatch document set — Packing List, Delivery Challan, and coordinates with Finance for invoicing.',
    features: [
      { name: 'Packing Lists', path: '/dispatch/packing-lists', desc: 'Bundle-wise packing details with section quantities and bundle labeling' },
      { name: 'Delivery Challans', path: '/dispatch/delivery-challans', desc: 'DC with customer info, section-wise dispatch quantities, vehicle and driver details' },
    ],
    outputs: ['Packing list', 'Delivery Challan', 'Loading photos', 'E-way bill reference'],
    whatHappens: [
      'Tape each section, bundle and tie, label each bundle',
      'Prepare Packing List with bundle-wise details',
      'Generate Delivery Challan with vehicle, driver, LR details',
      'Take loading photos and upload to app',
      'Coordinate E-way bill generation on GST portal',
    ],
  },
  {
    id: 'invoicing',
    order: 8,
    name: 'Invoicing',
    shortName: 'Invoice',
    department: 'Finance',
    icon: ReceiptLongIcon,
    description:
      'Final step — Invoice generation with detailed area calculation annexure (perimeter × length × qty = SFT per line). Three physical copies are printed, and the complete document bundle (Invoice, DC, Packing List, Test Certificate, E-way Bill) is emailed to the customer.',
    features: [
      { name: 'Invoices', path: '/dispatch/invoices', desc: 'Auto-calculated invoice with section-wise SFT, GST breakup, annexure, and PDF export' },
    ],
    outputs: ['Invoice PDF with annexure', 'Complete document bundle email to customer'],
    whatHappens: [
      'Generate Invoice with section-wise area calculation (perimeter × length × qty = SFT)',
      'Apply rates per SFT, add packing & transport charges, calculate GST',
      'Print 3 copies: driver, customer, Hycoat retention',
      'Email complete document bundle to customer — Invoice, DC, Packing List, Test Certificate, E-way Bill',
      'Update Work Order status to Invoiced/Closed',
    ],
  },
];

// Purchase runs as a parallel branch from Planning stage
export const PURCHASE_BRANCH = {
  id: 'purchase',
  linkedFrom: 'production-planning',
  name: 'Powder Procurement',
  shortName: 'Purchase',
  department: 'Purchase',
  icon: StoreIcon,
  description:
    'Runs in parallel once PPC raises a powder indent. Purchase raises PO to vendor, receives goods, updates stock ledger. Powder must be available before production can begin.',
  features: [
    { name: 'Powder Indents', path: '/purchase/indents', desc: 'Raise indent from PPC specifying powder code, color, and required quantity' },
    { name: 'Purchase Orders', path: '/purchase/orders', desc: 'PO to vendor with powder details, rates, and delivery date' },
    { name: 'GRN', path: '/purchase/grn', desc: 'Goods Received Note on powder delivery — verify and update stock' },
    { name: 'Powder Stock', path: '/purchase/stock', desc: 'Live stock ledger with reorder alerts and consumption tracking' },
  ],
  outputs: ['Purchase Order', 'GRN', 'Updated stock ledger'],
  whatHappens: [
    'Receive indent from PPC with powder code, color, and quantity',
    'Check warehouse stock — issue to production if available',
    'If short, raise Purchase Order to powder vendor',
    'Vendor delivers powder — create Goods Received Note (GRN)',
    'Update stock ledger; powder ready for production',
  ],
};
