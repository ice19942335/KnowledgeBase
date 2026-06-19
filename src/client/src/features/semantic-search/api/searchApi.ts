import { httpClient } from "../../../shared/api/httpClient";
import type { SearchResult } from "../model/types";

export async function searchChunks(query: string, topK?: number): Promise<SearchResult[]> {
  const response = await httpClient.post<SearchResult[]>("/api/search", { query, topK });
  return response.data;
}
