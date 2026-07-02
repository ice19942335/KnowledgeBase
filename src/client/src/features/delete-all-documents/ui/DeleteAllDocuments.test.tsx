import { describe, expect, it, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { DeleteAllDocuments } from "./DeleteAllDocuments";

const mutateMock = vi.fn();

vi.mock("../model/useDeleteAllDocuments", () => ({
  useDeleteAllDocuments: () => ({
    mutate: mutateMock,
    isPending: false,
    isError: false,
  }),
}));

function renderDeleteAllDocuments(documentCount: number) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <DeleteAllDocuments documentCount={documentCount} />
    </QueryClientProvider>,
  );
}

describe("DeleteAllDocuments", () => {
  it("renders nothing when there are no documents", () => {
    const { container } = renderDeleteAllDocuments(0);

    expect(container).toBeEmptyDOMElement();
  });

  it("shows confirmation before deleting all documents", async () => {
    renderDeleteAllDocuments(3);

    fireEvent.click(screen.getByTestId("delete-all-documents-button"));

    expect(screen.getByTestId("delete-all-documents-confirm")).toBeInTheDocument();
    expect(screen.getByText(/Delete all 3 documents/)).toBeInTheDocument();
  });

  it("calls delete mutation after confirmation", async () => {
    mutateMock.mockImplementation((_input, options) => {
      options?.onSuccess?.();
    });

    renderDeleteAllDocuments(2);

    fireEvent.click(screen.getByTestId("delete-all-documents-button"));
    fireEvent.click(screen.getByTestId("delete-all-documents-confirm-button"));

    await waitFor(() => {
      expect(mutateMock).toHaveBeenCalled();
    });
  });
});
