import { describe, expect, it, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { UploadDocument } from "./UploadDocument";

const mutateMock = vi.fn();

vi.mock("../model/useUploadDocument", () => ({
  useUploadDocument: () => ({
    mutate: mutateMock,
    isPending: false,
    isError: false,
  }),
}));

function renderUploadDocument() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <UploadDocument />
    </QueryClientProvider>,
  );
}

describe("UploadDocument", () => {
  it("resets the form after a successful single upload without throwing", async () => {
    mutateMock.mockImplementation((_input, options) => {
      options?.onSuccess?.({ mode: "single", document: { id: "1" } });
    });

    renderUploadDocument();

    const fileInput = screen.getByTestId("document-files-input") as HTMLInputElement;
    const file = new File(["content"], "policy.md", { type: "text/markdown" });

    fireEvent.change(fileInput, { target: { files: [file] } });

    const nameInput = screen.getByLabelText("Document name") as HTMLInputElement;
    fireEvent.change(nameInput, { target: { value: "HR Policy" } });
    fireEvent.click(screen.getByTestId("upload-documents-button"));

    await waitFor(() => {
      expect(mutateMock).toHaveBeenCalledWith(
        { files: [file], name: "HR Policy" },
        expect.any(Object),
      );
      expect(screen.queryByLabelText("Document name")).not.toBeInTheDocument();
      expect(fileInput.value).toBe("");
    });
  });

  it("shows selected file count for batch uploads and hides the display name field", () => {
    renderUploadDocument();

    const fileInput = screen.getByTestId("document-files-input") as HTMLInputElement;
    const files = [
      new File(["a"], "policy-a.md", { type: "text/markdown" }),
      new File(["b"], "policy-b.md", { type: "text/markdown" }),
    ];

    fireEvent.change(fileInput, { target: { files } });

    expect(screen.getByTestId("selected-files-summary")).toHaveTextContent("2 files selected");
    expect(screen.queryByLabelText("Document name")).not.toBeInTheDocument();
    expect(screen.getByTestId("upload-documents-button")).toHaveTextContent("Upload 2 files");
  });

  it("shows a partial batch failure summary", async () => {
    mutateMock.mockImplementation((_input, options) => {
      options?.onSuccess?.({
        mode: "batch",
        result: {
          succeededCount: 1,
          failedCount: 1,
          results: [
            { fileName: "broken.md", document: null, error: "Storage unavailable" },
            { fileName: "valid.md", document: { id: "2" }, error: null },
          ],
        },
      });
    });

    renderUploadDocument();

    const fileInput = screen.getByTestId("document-files-input") as HTMLInputElement;
    fireEvent.change(fileInput, {
      target: {
        files: [
          new File(["a"], "broken.md", { type: "text/markdown" }),
          new File(["b"], "valid.md", { type: "text/markdown" }),
        ],
      },
    });
    fireEvent.click(screen.getByTestId("upload-documents-button"));

    await waitFor(() => {
      expect(screen.getByTestId("batch-upload-summary")).toHaveTextContent(
        "Uploaded 1 of 2 files. Failed: broken.md.",
      );
    });
  });
});
