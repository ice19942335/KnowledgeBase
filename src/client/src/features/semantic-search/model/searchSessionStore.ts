import { createSessionMutationStore } from "../../../shared/lib/createSessionMutationStore";
import type { SearchResult } from "./types";

export const useSearchSessionStore = createSessionMutationStore<SearchResult[], string>();
