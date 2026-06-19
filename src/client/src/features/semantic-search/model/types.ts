export interface SearchResult {
  documentId: string;
  documentName: string;
  chunkIndex: number;
  content: string;
  score: number;
}
