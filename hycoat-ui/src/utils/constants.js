export const DEPARTMENTS = [
  'Sales', 'PPC', 'SCM', 'Production', 'QA', 'Purchase', 'Finance'
];

export const ROLES = ['Admin', 'Leader', 'User'];

export const PROCESS_TYPES = [
  'Powder Coating', 'Anodizing', 'Wood Effect', 'Chromotizing', 'PVDF', 'Mill Finish'
];

export const ORDER_STATUSES = {
  WORK_ORDER: ['Created', 'MaterialAwaited', 'MaterialReceived', 'InProduction', 'QAComplete', 'Dispatched', 'Invoiced', 'Closed'],
  INQUIRY: ['New', 'QuotationSent', 'BOMReceived', 'PISent', 'Confirmed', 'Closed', 'Lost'],
  QUOTATION: ['Draft', 'Sent', 'Accepted', 'Rejected', 'Expired'],
  PROFORMA_INVOICE: ['Draft', 'Sent', 'Accepted', 'Rejected'],
};

export const GST_RATE = 0.18;
export const CGST_RATE = 0.09;
export const SGST_RATE = 0.09;

export const DFT_MIN_MICRON = 60;
export const DFT_MAX_MICRON = 80;

// Area calculation: SFT = (PerimeterMM × LengthMM × Qty) / 92903.04
export const SQ_MM_TO_SQ_FT = 92903.04;
