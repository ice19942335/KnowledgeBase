import { describe, expect, it } from "vitest";
import { hasPendingDocuments } from "./documentPolling";
import type { DocumentDto } from "./types";

const processedDocument: DocumentDto = {
  id: "1",
  name: "Policy",
  fileName: "policy.md",
  contentType: "text/markdown",
  createdAt: "2026-06-18T00:00:00.000Z",
  processedAt: "2026-06-18T00:05:00.000Z",
  error: null,
  status: "Processed",
  chunkCount: 3,
};

describe("hasPendingDocuments", () => {
  it("returns false when all documents are terminal", () => {
    expect(hasPendingDocuments([processedDocument])).toBe(false);
    expect(hasPendingDocuments(undefined)).toBe(false);
  });

  it("returns true when any document is still processing", () => {
    expect(
      hasPendingDocuments([
        processedDocument,
        { ...processedDocument, id: "2", status: "Processing", chunkCount: 0 },
      ]),
    ).toBe(true);
  });
});
