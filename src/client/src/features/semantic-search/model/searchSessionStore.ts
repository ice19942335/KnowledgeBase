import { createSessionMutationStore } from "../../../shared/lib/createSessionMutationStore";
import type { SearchQueryResult } from "./types";

export const useSearchSessionStore = createSessionMutationStore<SearchQueryResult, string>();
