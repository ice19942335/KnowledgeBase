import { useQuery } from "@tanstack/react-query";
import { fetchDocumentById, fetchDocuments } from "../api/documentApi";
import { documentPollingIntervalMs, hasPendingDocuments, isDocumentPending } from "./documentPolling";

export const documentKeys = {
  all: ["documents"] as const,
  detail: (documentId: string) => ["documents", documentId] as const,
};

export function useDocuments() {
  return useQuery({
    queryKey: documentKeys.all,
    queryFn: fetchDocuments,
    refetchInterval: (query) =>
      hasPendingDocuments(query.state.data) ? documentPollingIntervalMs : false,
  });
}

export function useDocument(documentId: string) {
  return useQuery({
    queryKey: documentKeys.detail(documentId),
    queryFn: () => fetchDocumentById(documentId),
    refetchInterval: (query) =>
      isDocumentPending(query.state.data) ? documentPollingIntervalMs : false,
  });
}
