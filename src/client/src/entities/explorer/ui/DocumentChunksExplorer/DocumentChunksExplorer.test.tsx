import { describe, expect, it } from "vitest";
import { render, screen, within } from "@testing-library/react";
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
        indexedAt: "2026-06-18T10:00:00Z",
        embeddingTokenCount: 0,
      },
      {
        id: "chunk-3",
        documentId: "doc-1",
        documentName: "HR Policy",
        chunkIndex: 5,
        content: "Sick leave policy text.",
        indexedAt: "2026-06-18T10:00:00Z",
        embeddingTokenCount: 0,
      },
      {
        id: "chunk-4",
        documentId: "doc-1",
        documentName: "HR Policy",
        chunkIndex: 2,
        content: "Parental leave policy text.",
        indexedAt: "2026-06-18T10:00:00Z",
        embeddingTokenCount: 0,
      },
    ],
  },
  {
    documentId: "doc-2",
    documentName: "Safety Manual",
    chunks: [
      {
        id: "chunk-2",
        documentId: "doc-2",
        documentName: "Safety Manual",
        chunkIndex: 1,
        content: "Emergency exit locations.",
        indexedAt: "2026-06-18T10:00:00Z",
        embeddingTokenCount: 0,
      },
    ],
  },
];

describe("DocumentChunksExplorer", () => {
  it("renders indexed date from API field", () => {
    render(
      <DocumentChunksExplorer
        documents={[documents[0]]}
        totalChunks={3}
      />,
    );

    const indexedAt = screen.getByTestId("chunk-indexed-at-chunk-1");
    expect(indexedAt).not.toHaveTextContent("Invalid Date");
    expect(indexedAt.textContent?.length).toBeGreaterThan(0);
  });

  it("shows awaiting-request empty state", () => {
    render(
      <DocumentChunksExplorer
        documents={[]}
        totalChunks={0}
        emptyState="awaiting-request"
      />,
    );

    expect(screen.getByTestId("document-chunks-empty")).toHaveTextContent(
      "Run a traced chat question",
    );
  });

  it("shows chunk view tabs for a single document with used chunks", async () => {
    render(
      <DocumentChunksExplorer
        documents={[documents[0]]}
        totalChunks={3}
        highlightedChunkIndices={{ "doc-1": [5, 2] }}
      />,
    );

    expect(screen.getByTestId("chunk-view-tabs-doc-1")).toBeInTheDocument();
    expect(screen.getByTestId("document-used-chunks-doc-1")).toBeInTheDocument();

    const usedList = screen.getByTestId("document-used-chunks-doc-1");
    const usedItems = within(usedList).getAllByRole("listitem");
    expect(usedItems).toHaveLength(2);
    expect(usedItems[0]).toHaveAttribute("data-priority-rank", "1");
    expect(usedItems[0]).toHaveAttribute("data-testid", "chunk-chunk-3");
    expect(usedItems[1]).toHaveAttribute("data-priority-rank", "2");
    expect(usedItems[1]).toHaveAttribute("data-testid", "chunk-chunk-4");

    await userEvent.click(screen.getByTestId("chunk-view-all-doc-1"));

    const allList = screen.getByTestId("document-chunks-doc-1");
    expect(within(allList).getAllByRole("listitem")).toHaveLength(3);
  });

  it("switches document chunks with subtabs", async () => {
    render(
      <DocumentChunksExplorer
        documents={documents}
        totalChunks={4}
        highlightedChunkIndices={{ "doc-1": [0], "doc-2": [1] }}
      />,
    );

    expect(screen.getByTestId("document-chunks-tabs")).toBeInTheDocument();
    expect(screen.getByTestId("chunk-chunk-1")).toBeInTheDocument();
    expect(screen.queryByTestId("chunk-chunk-2")).not.toBeInTheDocument();

    await userEvent.click(screen.getByTestId("document-tab-doc-2"));

    expect(screen.getByTestId("chunk-chunk-2")).toBeInTheDocument();
    expect(screen.queryByTestId("chunk-chunk-1")).not.toBeInTheDocument();
    expect(screen.getByTestId("chunk-view-tabs-doc-2")).toBeInTheDocument();
  });
});
