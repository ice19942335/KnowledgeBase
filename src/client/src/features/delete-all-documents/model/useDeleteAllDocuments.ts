import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteAllDocuments, documentKeys } from "../../../entities/document";

export function useDeleteAllDocuments() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deleteAllDocuments(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}
