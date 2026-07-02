import { httpClient } from "../../../shared/api/httpClient";
import type { SearchQueryResult } from "../model/types";

export async function searchChunks(query: string, topK?: number): Promise<SearchQueryResult> {
  const response = await httpClient.post<SearchQueryResult>("/api/search", { query, topK });
  return response.data;
}
