export interface SourceReference {
  documentId: string;
  documentName: string;
  chunkIndex: number;
}

export interface ChatAnswer {
  conversationId: string;
  answer: string;
  sources: SourceReference[];
}
