import { describe, expect, it } from "vitest";
import { uniqueDocumentSources } from "./uniqueDocumentSources";

describe("uniqueDocumentSources", () => {
  it("deduplicates sources by document id", () => {
    const sources = uniqueDocumentSources([
      { documentId: "a", documentName: "Policy" },
      { documentId: "a", documentName: "Policy" },
      { documentId: "b", documentName: "Handbook" },
    ]);

    expect(sources).toHaveLength(2);
    expect(sources.map((source) => source.documentId)).toEqual(["a", "b"]);
  });
});
