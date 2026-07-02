import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { DocumentDetailPage } from "./DocumentDetailPage";
import type { DocumentDto } from "../../../entities/document/model/types";
import type { SearchExplorerResult } from "../../../entities/explorer/model/types";

const documentId = "11111111-1111-1111-1111-111111111111";

const processedDocument: DocumentDto = {
  id: documentId,
  name: "HR Policy",
  fileName: "hr-policy.pdf",
  contentType: "application/pdf",
  status: "Processed",
  chunkCount: 2,
  error: null,
  createdAt: "2026-06-18T08:00:00Z",
  processedAt: "2026-06-18T08:05:00Z",
};

const explorerResult: SearchExplorerResult = {
  totalChunks: 2,
  documents: [
    {
      documentId,
      documentName: "HR Policy",
      chunks: [
        {
          id: "chunk-1",
          documentId,
          documentName: "HR Policy",
          chunkIndex: 0,
          content: "Vacation policy text.",
          indexedAt: "2026-06-18T08:05:00Z",
          embeddingTokenCount: 120,
        },
        {
          id: "chunk-2",
          documentId,
          documentName: "HR Policy",
          chunkIndex: 1,
          content: "Sick leave policy text.",
          indexedAt: "2026-06-18T08:05:00Z",
          embeddingTokenCount: 95,
        },
      ],
    },
  ],
};

const fetchDocumentById = vi.fn();
const fetchSearchExplorer = vi.fn();

vi.mock("../../../entities/document/model/queries", () => ({
  useDocument: (id: string) => ({
    data: fetchDocumentById(id),
    isLoading: false,
    isError: false,
  }),
}));

vi.mock("../../../entities/explorer", async () => {
  const actual = await vi.importActual<typeof import("../../../entities/explorer")>(
    "../../../entities/explorer",
  );

  return {
    ...actual,
    useSearchExplorer: (documentIds: string[] | undefined) => ({
      data: documentIds ? fetchSearchExplorer(documentIds) : undefined,
      isLoading: false,
      isError: false,
      isSuccess: !!documentIds,
    }),
  };
});

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <DocumentDetailPage documentId={documentId} />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("DocumentDetailPage", () => {
  beforeEach(() => {
    fetchDocumentById.mockReset();
    fetchSearchExplorer.mockReset();
  });

  it("shows metadata, indexing tokens, and chunks for processed documents", () => {
    fetchDocumentById.mockReturnValue(processedDocument);
    fetchSearchExplorer.mockReturnValue(explorerResult);

    renderPage();

    expect(screen.getByTestId("document-detail-page")).toBeInTheDocument();
    expect(screen.getByRole("heading", { level: 1, name: "HR Policy" })).toBeInTheDocument();
    expect(screen.getByTestId("document-detail-status")).toHaveTextContent("Processed");
    expect(screen.getByTestId("document-detail-chunk-count")).toHaveTextContent("2");
    expect(screen.getByTestId("document-indexing-tokens-total")).toHaveTextContent("215");
    expect(screen.getByTestId("chunk-chunk-1")).toBeInTheDocument();
    expect(screen.getByTestId("chunk-tokens-chunk-1")).toHaveTextContent("120 tokens");
  });

  it("shows a processing hint before chunks are available", () => {
    fetchDocumentById.mockReturnValue({
      ...processedDocument,
      status: "Processing",
      chunkCount: 0,
      processedAt: null,
    });

    renderPage();

    expect(screen.getByTestId("document-detail-processing-hint")).toBeInTheDocument();
    expect(screen.queryByTestId("document-detail-chunks")).not.toBeInTheDocument();
  });
});
