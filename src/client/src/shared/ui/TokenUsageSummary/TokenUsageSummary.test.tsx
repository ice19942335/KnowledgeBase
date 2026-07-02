import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { TokenUsageSummaryPanel } from "./TokenUsageSummary";

describe("TokenUsageSummaryPanel", () => {
  it("renders request, indexed, and total token counts", () => {
    render(<TokenUsageSummaryPanel tokenUsage={{ requestTokens: 120, indexedTokens: 340 }} />);

    expect(screen.getByTestId("token-usage-summary-request")).toHaveTextContent("120");
    expect(screen.getByTestId("token-usage-summary-indexed")).toHaveTextContent("340");
    expect(screen.getByTestId("token-usage-summary-total")).toHaveTextContent("460");
  });
});
