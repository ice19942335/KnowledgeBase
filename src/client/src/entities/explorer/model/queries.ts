import { useQuery } from "@tanstack/react-query";
import { fetchSearchExplorer } from "../api/explorerApi";

export const explorerKeys = {
  chunks: ["explorer", "chunks"] as const,
};

export function useSearchExplorer() {
  return useQuery({
    queryKey: explorerKeys.chunks,
    queryFn: fetchSearchExplorer,
  });
}
