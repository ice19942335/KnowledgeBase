import { useMutation, useQueryClient } from "@tanstack/react-query";
import { documentKeys, retryDocument } from "../../../entities/document";

export function useRetryDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => retryDocument(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}
