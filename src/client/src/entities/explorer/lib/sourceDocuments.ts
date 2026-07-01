import type { ChunkDetail, SourceReference } from "../model/types";

export interface PrioritizedChunk {
  chunk: ChunkDetail;
  priorityRank: number;
}

export interface SourceDocument {
  documentId: string;
  documentName: string;
}

export function extractSourceDocuments(sources: SourceReference[]): SourceDocument[] {
  const documents = new Map<string, string>();

  for (const source of sources) {
    if (!documents.has(source.documentId)) {
      documents.set(source.documentId, source.documentName);
    }
  }

  return Array.from(documents, ([documentId, documentName]) => ({
    documentId,
    documentName,
  }));
}

export function buildHighlightedChunkMap(
  sources: SourceReference[],
): Record<string, number[]> {
  const usedChunkIndices: Record<string, number[]> = {};

  for (const source of sources) {
    const indices = usedChunkIndices[source.documentId] ?? [];
    if (!indices.includes(source.chunkIndex)) {
      indices.push(source.chunkIndex);
    }

    usedChunkIndices[source.documentId] = indices;
  }

  return usedChunkIndices;
}

export function getOrderedUsedChunks(
  chunks: ChunkDetail[],
  orderedChunkIndices: number[],
): PrioritizedChunk[] {
  const chunkByIndex = new Map(chunks.map((chunk) => [chunk.chunkIndex, chunk]));

  return orderedChunkIndices.flatMap((chunkIndex, index) => {
    const chunk = chunkByIndex.get(chunkIndex);
    return chunk ? [{ chunk, priorityRank: index + 1 }] : [];
  });
}
