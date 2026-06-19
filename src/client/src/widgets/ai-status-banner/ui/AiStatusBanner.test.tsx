import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { AiStatusBanner } from "./AiStatusBanner";

describe("AiStatusBanner", () => {
  it("renders the missing API key message", () => {
    render(<AiStatusBanner message="Gemini API key is not configured." />);

    expect(screen.getByTestId("ai-status-banner")).toBeInTheDocument();
    expect(screen.getByText("Gemini API key is not configured.")).toBeInTheDocument();
  });
});
