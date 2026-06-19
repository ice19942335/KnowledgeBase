import { useQuery } from "@tanstack/react-query";
import { fetchDocuments } from "../api/documentApi";
import { documentPollingIntervalMs, hasPendingDocuments } from "./documentPolling";

export const documentKeys = {
  all: ["documents"] as const,
};

export function useDocuments() {
  return useQuery({
    queryKey: documentKeys.all,
    queryFn: fetchDocuments,
    refetchInterval: (query) =>
      hasPendingDocuments(query.state.data) ? documentPollingIntervalMs : false,
  });
}
