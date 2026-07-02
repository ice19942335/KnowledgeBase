import type { ChunkDetail } from "../../explorer/model/types";

export function sumEmbeddingTokens(chunks: ChunkDetail[]): number {
  return chunks.reduce((total, chunk) => total + chunk.embeddingTokenCount, 0);
}
