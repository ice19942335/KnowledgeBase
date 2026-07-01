import { useSessionMutation } from "../../../shared/lib/useSessionMutation";
import { searchChunks } from "../api/searchApi";
import { useSearchSessionStore } from "./searchSessionStore";

export function useSemanticSearch() {
  return useSessionMutation(useSearchSessionStore, {
    mutationFn: (query: string) => searchChunks(query),
  });
}
