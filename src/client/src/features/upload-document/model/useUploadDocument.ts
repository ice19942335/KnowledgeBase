import { useMutation, useQueryClient } from "@tanstack/react-query";
import { documentKeys, uploadDocument } from "../../../entities/document";

interface UploadDocumentInput {
  file: File;
  name?: string;
}

export function useUploadDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ file, name }: UploadDocumentInput) => uploadDocument(file, name),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}
