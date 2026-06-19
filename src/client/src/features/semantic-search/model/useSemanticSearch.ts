import { useMutation } from "@tanstack/react-query";
import { searchChunks } from "../api/searchApi";

export function useSemanticSearch() {
  return useMutation({
    mutationFn: (query: string) => searchChunks(query),
  });
}
