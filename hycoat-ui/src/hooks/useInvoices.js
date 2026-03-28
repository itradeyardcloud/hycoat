import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { invoiceService } from '@/services/dispatchService';

export const useInvoices = (params) =>
  useQuery({
    queryKey: ['invoices', params],
    queryFn: () => invoiceService.getAll(params),
  });

export const useInvoice = (id) =>
  useQuery({
    queryKey: ['invoices', id],
    queryFn: () => invoiceService.getById(id),
    enabled: !!id,
  });

export const useInvoiceByWorkOrder = (woId) =>
  useQuery({
    queryKey: ['invoices', 'byWorkOrder', woId],
    queryFn: () => invoiceService.getByWorkOrder(woId),
    enabled: !!woId,
  });

export const useInvoiceAutoFill = (woId) =>
  useQuery({
    queryKey: ['invoices', 'autoFill', woId],
    queryFn: () => invoiceService.autoFill(woId),
    enabled: !!woId,
  });

export const useCreateInvoice = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => invoiceService.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
    },
  });
};

export const useUpdateInvoice = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => invoiceService.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
    },
  });
};

export const useDeleteInvoice = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => invoiceService.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
    },
  });
};

export const useUpdateInvoiceStatus = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => invoiceService.updateStatus(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
    },
  });
};

export const useGenerateInvoicePdf = () =>
  useMutation({
    mutationFn: (id) => invoiceService.generatePdf(id),
  });

export const useSendInvoiceEmail = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => invoiceService.sendEmail(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
    },
  });
};
