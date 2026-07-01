import { describe, expect, it } from "vitest";
import {
  buildHighlightedChunkMap,
  extractSourceDocuments,
  getOrderedUsedChunks,
} from "./sourceDocuments";
import type { SourceReference } from "../model/types";

const sources: SourceReference[] = [
  { documentId: "doc-1", documentName: "HR Policy", chunkIndex: 2 },
  { documentId: "doc-2", documentName: "Safety Manual", chunkIndex: 0 },
  { documentId: "doc-1", documentName: "HR Policy", chunkIndex: 5 },
];

describe("extractSourceDocuments", () => {
  it("returns unique documents in source order", () => {
    expect(extractSourceDocuments(sources)).toEqual([
      { documentId: "doc-1", documentName: "HR Policy" },
      { documentId: "doc-2", documentName: "Safety Manual" },
    ]);
  });

  it("returns empty list when there are no sources", () => {
    expect(extractSourceDocuments([])).toEqual([]);
  });
});

describe("buildHighlightedChunkMap", () => {
  it("groups chunk indices by document in source priority order", () => {
    expect(buildHighlightedChunkMap(sources)).toEqual({
      "doc-1": [2, 5],
      "doc-2": [0],
    });
  });
});

describe("getOrderedUsedChunks", () => {
  it("returns used chunks in priority order with ranks", () => {
    const chunks = [
      {
        id: "chunk-2",
        documentId: "doc-1",
        documentName: "HR Policy",
        chunkIndex: 2,
        content: "Second priority chunk.",
        indexedAtUtc: "2026-06-18T10:00:00Z",
      },
      {
        id: "chunk-5",
        documentId: "doc-1",
        documentName: "HR Policy",
        chunkIndex: 5,
        content: "First priority chunk.",
        indexedAtUtc: "2026-06-18T10:00:00Z",
      },
    ];

    expect(getOrderedUsedChunks(chunks, [5, 2])).toEqual([
      { chunk: chunks[1], priorityRank: 1 },
      { chunk: chunks[0], priorityRank: 2 },
    ]);
  });
});
