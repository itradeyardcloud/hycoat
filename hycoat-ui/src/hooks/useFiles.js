import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { fileService } from '@/services/fileService';

export const useEntityFiles = (entityType, entityId) =>
  useQuery({
    queryKey: ['files', entityType, entityId],
    queryFn: () => fileService.listByEntity(entityType, entityId),
    enabled: !!entityType && !!entityId,
  });

export const useUploadFile = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload) => fileService.upload(payload),
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ['files', vars.entityType, vars.entityId] });
    },
  });
};

export const useDeleteFile = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id }) => fileService.remove(id),
    onSuccess: (_, vars) => {
      if (vars.entityType && vars.entityId) {
        qc.invalidateQueries({ queryKey: ['files', vars.entityType, vars.entityId] });
      }
    },
  });
};
