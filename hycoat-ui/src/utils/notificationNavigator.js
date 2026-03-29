const routeByReferenceType = {
  WorkOrder: (id) => `/sales/work-orders/${id}`,
  MaterialInward: (id) => `/material-inward/inwards/${id}`,
  IncomingInspection: (id) => `/material-inward/inspections/${id}`,
  ProductionWorkOrder: (id) => `/ppc/work-orders/${id}`,
  FinalInspection: (id) => `/quality/final/${id}`,
  DeliveryChallan: (id) => `/dispatch/delivery-challans/${id}`,
  Invoice: (id) => `/dispatch/invoices/${id}`,
};

export function getNotificationRoute(referenceType, referenceId) {
  if (!referenceType || !referenceId) {
    return '/notifications';
  }

  const factory = routeByReferenceType[referenceType];
  return factory ? factory(referenceId) : '/notifications';
}
