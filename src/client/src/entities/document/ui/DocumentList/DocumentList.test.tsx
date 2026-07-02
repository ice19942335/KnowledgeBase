import type { ComponentProps } from "react";
import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { DocumentList } from "./DocumentList";
import type { DocumentDto } from "../../model/types";

function createDocument(overrides: Partial<DocumentDto> = {}): DocumentDto {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    name: "HR Policy",
    fileName: "hr.pdf",
    contentType: "application/pdf",
    createdAt: "2026-01-01T00:00:00Z",
    processedAt: "2026-01-01T00:05:00Z",
    error: null,
    status: "Processed",
    chunkCount: 3,
    ...overrides,
  };
}

describe("DocumentList", () => {
  function renderList(props: ComponentProps<typeof DocumentList>) {
    return render(
      <MemoryRouter>
        <DocumentList {...props} />
      </MemoryRouter>,
    );
  }

  it("shows an empty state when there are no documents", () => {
    renderList({ documents: [], onDelete: vi.fn() });

    expect(screen.getByText(/no documents yet/i)).toBeInTheDocument();
  });

  it("renders a row per document with its status", () => {
    const documents = [
      createDocument(),
      createDocument({ id: "22222222-2222-2222-2222-222222222222", name: "Handbook", status: "Processing" }),
    ];

    renderList({ documents, onDelete: vi.fn() });

    expect(screen.getByText("HR Policy")).toBeInTheDocument();
    expect(screen.getByText("Handbook")).toBeInTheDocument();
    expect(screen.getByText("Processed")).toBeInTheDocument();
    expect(screen.getByText("Processing")).toBeInTheDocument();
  });

  it("links each document name to its details page", () => {
    renderList({ documents: [createDocument()], onDelete: vi.fn() });

    expect(screen.getByTestId("document-details-link-11111111-1111-1111-1111-111111111111")).toHaveAttribute(
      "href",
      "/documents/11111111-1111-1111-1111-111111111111",
    );
  });

  it("calls onDelete with the document id", async () => {
    const onDelete = vi.fn();
    renderList({ documents: [createDocument()], onDelete: onDelete });

    await userEvent.click(screen.getByRole("button", { name: /delete/i }));

    expect(onDelete).toHaveBeenCalledWith("11111111-1111-1111-1111-111111111111");
  });

  it("shows a retry button only for failed documents", async () => {
    const onRetry = vi.fn();
    const documents = [
      createDocument({ status: "Failed" }),
      createDocument({ id: "22222222-2222-2222-2222-222222222222", status: "Processed" }),
    ];

    renderList({ documents, onDelete: vi.fn(), onRetry: onRetry });

    expect(screen.getByRole("button", { name: /retry/i })).toBeInTheDocument();
    expect(screen.queryAllByRole("button", { name: /retry/i })).toHaveLength(1);

    await userEvent.click(screen.getByRole("button", { name: /retry/i }));

    expect(onRetry).toHaveBeenCalledWith("11111111-1111-1111-1111-111111111111");
  });
});
