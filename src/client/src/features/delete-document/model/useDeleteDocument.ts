import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteDocument, documentKeys } from "../../../entities/document";

export function useDeleteDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteDocument(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}
