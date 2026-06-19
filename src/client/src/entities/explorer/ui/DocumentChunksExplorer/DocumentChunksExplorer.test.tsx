import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DocumentChunksExplorer } from "./DocumentChunksExplorer";

const documents = [
  {
    documentId: "doc-1",
    documentName: "HR Policy",
    chunks: [
      {
        id: "chunk-1",
        documentId: "doc-1",
        documentName: "HR Policy",
        chunkIndex: 0,
        content: "Vacation days policy text.",
        indexedAtUtc: "2026-06-18T10:00:00Z",
      },
    ],
  },
];

describe("DocumentChunksExplorer", () => {
  it("shows empty state when there are no documents", () => {
    render(<DocumentChunksExplorer documents={[]} totalChunks={0} />);

    expect(screen.getByTestId("document-chunks-empty")).toBeInTheDocument();
  });

  it("expands and collapses document chunks", async () => {
    render(<DocumentChunksExplorer documents={documents} totalChunks={1} />);

    expect(screen.getByTestId("chunk-chunk-1")).toBeInTheDocument();

    await userEvent.click(screen.getByTestId("document-toggle-doc-1"));

    expect(screen.queryByTestId("chunk-chunk-1")).not.toBeInTheDocument();
  });
});
