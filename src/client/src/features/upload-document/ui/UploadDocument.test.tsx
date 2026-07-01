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
  it("resets the form after a successful upload without throwing", async () => {
    mutateMock.mockImplementation((_input, options) => {
      options?.onSuccess?.();
    });

    renderUploadDocument();

    const fileInput = screen.getByLabelText("Document file") as HTMLInputElement;
    const nameInput = screen.getByLabelText("Document name") as HTMLInputElement;
    const file = new File(["content"], "policy.md", { type: "text/markdown" });

    fireEvent.change(fileInput, { target: { files: [file] } });
    fireEvent.change(nameInput, { target: { value: "HR Policy" } });
    fireEvent.submit(screen.getByRole("button", { name: "Upload" }).closest("form")!);

    await waitFor(() => {
      expect(mutateMock).toHaveBeenCalled();
    });

    expect(nameInput.value).toBe("");
    expect(fileInput.value).toBe("");
  });
});
