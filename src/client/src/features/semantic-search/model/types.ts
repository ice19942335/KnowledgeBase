import type { TokenUsageSummary } from "../../../shared/model/tokenUsage";

export interface SearchResult {
  documentId: string;
  documentName: string;
  chunkIndex: number;
  content: string;
  score: number;
  embeddingTokenCount: number;
}

export interface SearchQueryResult {
  results: SearchResult[];
  tokenUsage: TokenUsageSummary;
}
