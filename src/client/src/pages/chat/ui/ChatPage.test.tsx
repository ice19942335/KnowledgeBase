import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { ChatPage } from "./ChatPage";

vi.mock("../../../features/rag-chat/model/useRagChat", () => ({
  useRagChat: () => ({
    mutate: vi.fn(),
    data: {
      conversationId: "22222222-2222-2222-2222-222222222222",
      answer: "The probation period is 1 month.",
      sources: [
        {
          documentId: "33333333-3333-3333-3333-333333333333",
          documentName: "HR Policy",
          chunkIndex: 2,
        },
        {
          documentId: "33333333-3333-3333-3333-333333333333",
          documentName: "HR Policy",
          chunkIndex: 5,
        },
      ],
    },
    isPending: false,
    isError: false,
  }),
}));

vi.mock("../../../entities/document/api/documentApi", () => ({
  downloadDocument: vi.fn(),
}));

describe("ChatPage", () => {
  it("renders unique document source links", () => {
    render(
      <MemoryRouter>
        <ChatPage />
      </MemoryRouter>,
    );

    expect(screen.getByTestId("chat-sources")).toBeInTheDocument();
    expect(screen.getAllByTestId("chat-source-33333333-3333-3333-3333-333333333333")).toHaveLength(1);
    expect(screen.getByText("HR Policy")).toBeInTheDocument();
    expect(screen.getByTestId("chat-source-open-33333333-3333-3333-3333-333333333333")).toHaveAttribute(
      "href",
      "/documents/33333333-3333-3333-3333-333333333333/view",
    );
    expect(screen.getByTestId("chat-source-download-33333333-3333-3333-3333-333333333333")).toBeInTheDocument();
  });
});
