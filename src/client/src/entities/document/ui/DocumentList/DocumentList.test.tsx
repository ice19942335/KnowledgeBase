import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DocumentList } from "./DocumentList";
import type { DocumentDto } from "../../model/types";

function createDocument(overrides: Partial<DocumentDto> = {}): DocumentDto {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    name: "HR Policy",
    fileName: "hr.pdf",
    contentType: "application/pdf",
    uploadedAtUtc: "2026-01-01T00:00:00Z",
    status: "Processed",
    chunkCount: 3,
    ...overrides,
  };
}

describe("DocumentList", () => {
  it("shows an empty state when there are no documents", () => {
    render(<DocumentList documents={[]} onDelete={vi.fn()} />);

    expect(screen.getByText(/no documents yet/i)).toBeInTheDocument();
  });

  it("renders a row per document with its status", () => {
    const documents = [
      createDocument(),
      createDocument({ id: "22222222-2222-2222-2222-222222222222", name: "Handbook", status: "Processing" }),
    ];

    render(<DocumentList documents={documents} onDelete={vi.fn()} />);

    expect(screen.getByText("HR Policy")).toBeInTheDocument();
    expect(screen.getByText("Handbook")).toBeInTheDocument();
    expect(screen.getByText("Processed")).toBeInTheDocument();
    expect(screen.getByText("Processing")).toBeInTheDocument();
  });

  it("calls onDelete with the document id", async () => {
    const onDelete = vi.fn();
    render(<DocumentList documents={[createDocument()]} onDelete={onDelete} />);

    await userEvent.click(screen.getByRole("button", { name: /delete/i }));

    expect(onDelete).toHaveBeenCalledWith("11111111-1111-1111-1111-111111111111");
  });
});
