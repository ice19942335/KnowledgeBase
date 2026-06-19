export interface PipelineTraceStep {
  order: number;
  name: string;
  description: string;
  durationMs: number;
  input: unknown;
  output: unknown;
}

export interface ChunkDetail {
  id: string;
  documentId: string;
  documentName: string;
  chunkIndex: number;
  content: string;
  indexedAtUtc: string;
}

export interface DocumentChunksGroup {
  documentId: string;
  documentName: string;
  chunks: ChunkDetail[];
}

export interface SearchExplorerResult {
  documents: DocumentChunksGroup[];
  totalChunks: number;
}

export interface SourceReference {
  documentId: string;
  documentName: string;
  chunkIndex: number;
}

export interface ChatTraceAnswer {
  conversationId: string;
  answer: string;
  sources: SourceReference[];
  steps: PipelineTraceStep[];
  totalDurationMs: number;
}
