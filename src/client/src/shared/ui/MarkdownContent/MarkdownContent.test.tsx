import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { MarkdownContent } from "./MarkdownContent";

describe("MarkdownContent", () => {
  it("renders bold text and lists from markdown", () => {
    render(
      <MarkdownContent
        content={"Based on the policy, the period is **1 month**.\n\n- Item one\n- Item two"}
        testId="markdown-test"
      />,
    );

    expect(screen.getByTestId("markdown-test")).toBeInTheDocument();
    expect(screen.getByText("1 month").tagName).toBe("STRONG");
    expect(screen.getByText("Item one")).toBeInTheDocument();
    expect(screen.getByText("Item two")).toBeInTheDocument();
  });
});
