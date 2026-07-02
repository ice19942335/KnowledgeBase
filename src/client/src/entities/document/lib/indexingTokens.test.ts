import { describe, expect, it } from "vitest";
import { sumEmbeddingTokens } from "./indexingTokens";
import type { ChunkDetail } from "../../explorer/model/types";

function createChunk(embeddingTokenCount: number): ChunkDetail {
  return {
    id: "chunk-1",
    documentId: "doc-1",
    documentName: "Policy",
    chunkIndex: 0,
    content: "Sample content",
    indexedAt: "2026-06-18T10:00:00Z",
    embeddingTokenCount,
  };
}

describe("sumEmbeddingTokens", () => {
  it("returns zero for an empty chunk list", () => {
    expect(sumEmbeddingTokens([])).toBe(0);
  });

  it("sums embedding token counts across chunks", () => {
    const chunks = [createChunk(120), createChunk(85), createChunk(40)];

    expect(sumEmbeddingTokens(chunks)).toBe(245);
  });
});
