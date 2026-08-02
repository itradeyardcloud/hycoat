import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import profileDiagramService from '@/services/profileDiagramService';

const QUERY_KEY = 'profileDiagrams';

// ── Management list (paginated) ───────────────────────────────────────────────

export const useProfileDiagrams = (params) =>
  useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => profileDiagramService.getAll(params),
  });

export const useProfileDiagram = (id) =>
  useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => profileDiagramService.getById(id),
    enabled: !!id,
  });

// ── Gallery lookup by exact codes ─────────────────────────────────────────────

/**
 * @param {string[]} codes - non-empty array of profile codes
 * @param {boolean} enabled - set false to skip the request
 */
export const useProfileDiagramsByCodes = (codes, enabled = true) =>
  useQuery({
    queryKey: [QUERY_KEY, 'byCodes', codes],
    queryFn: () => profileDiagramService.getByCodes(codes),
    enabled: enabled && Array.isArray(codes) && codes.length > 0,
  });

// ── Autocomplete suggestions (partial search) ─────────────────────────────────

/**
 * @param {string} term - partial search term typed by the user
 */
export const useProfileDiagramSuggest = (term) =>
  useQuery({
    queryKey: [QUERY_KEY, 'suggest', term],
    queryFn: () => profileDiagramService.suggest(term),
    enabled: typeof term === 'string' && term.trim().length >= 1,
    staleTime: 30_000,
  });

// ── Mutations ─────────────────────────────────────────────────────────────────

export const useCreateProfileDiagram = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data) => profileDiagramService.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
};

export const useUpdateProfileDiagram = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }) => profileDiagramService.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
};

export const useDeleteProfileDiagram = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id) => profileDiagramService.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
};

export const useUploadProfileImage = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, file }) => profileDiagramService.uploadImage(id, file),
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
};
