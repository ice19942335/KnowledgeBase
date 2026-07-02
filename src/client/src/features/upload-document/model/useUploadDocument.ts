import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  documentKeys,
  uploadDocument,
  uploadDocumentsBatch,
  type BatchUploadResultDto,
  type DocumentDto,
} from "../../../entities/document";

interface UploadDocumentsInput {
  files: File[];
  name?: string;
}

export type UploadDocumentsResult =
  | { mode: "single"; document: DocumentDto }
  | { mode: "batch"; result: BatchUploadResultDto };

export function useUploadDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ files, name }: UploadDocumentsInput): Promise<UploadDocumentsResult> => {
      if (files.length === 0) {
        throw new Error("At least one file is required.");
      }

      if (files.length === 1) {
        const document = await uploadDocument(files[0], name);
        return { mode: "single", document };
      }

      const result = await uploadDocumentsBatch(files);
      return { mode: "batch", result };
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: documentKeys.all });
    },
  });
}
