import { useQuery } from "@tanstack/react-query";
import { fetchSearchExplorer } from "../api/explorerApi";

export const explorerKeys = {
  chunks: (documentIds: string[]) =>
    ["explorer", "chunks", ...documentIds.slice().sort()] as const,
};

export function useSearchExplorer(documentIds: string[] | undefined) {
  const enabled = !!documentIds && documentIds.length > 0;

  return useQuery({
    queryKey: enabled ? explorerKeys.chunks(documentIds) : ["explorer", "chunks", "idle"],
    queryFn: () => fetchSearchExplorer(documentIds!),
    enabled,
  });
}
