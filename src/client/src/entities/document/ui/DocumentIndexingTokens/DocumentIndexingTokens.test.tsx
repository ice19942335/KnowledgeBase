import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { DocumentIndexingTokens } from "./DocumentIndexingTokens";

describe("DocumentIndexingTokens", () => {
  it("renders chunk count and embedding token total", () => {
    render(<DocumentIndexingTokens chunkCount={12} tokenCount={1840} />);

    expect(screen.getByTestId("document-indexing-tokens-chunks")).toHaveTextContent("12");
    expect(screen.getByTestId("document-indexing-tokens-total")).toHaveTextContent("1840");
  });
});
