import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { formatJson, PipelineTraceTimeline } from "./PipelineTraceTimeline";

describe("formatJson", () => {
  it("pretty-prints objects", () => {
    expect(formatJson({ tenantId: "abc" })).toBe('{\n  "tenantId": "abc"\n}');
  });
});

describe("PipelineTraceTimeline", () => {
  it("renders steps with collapsible input and output payloads", async () => {
    render(
      <PipelineTraceTimeline
        totalDurationMs={120}
        steps={[
          {
            order: 1,
            name: "tenant.resolve",
            description: "Resolve tenant",
            durationMs: 0,
            input: null,
            output: { tenantId: "11111111-1111-1111-1111-111111111111" },
          },
        ]}
      />,
    );

    expect(screen.getByTestId("pipeline-total-duration")).toHaveTextContent("120 ms");
    expect(screen.getByTestId("pipeline-step-1")).toBeInTheDocument();
    expect(screen.getByTestId("pipeline-step-1-output")).toHaveTextContent("tenantId");

    await userEvent.click(screen.getByTestId("pipeline-step-1-tab-input"));
    expect(screen.getByTestId("pipeline-step-1-input")).toHaveTextContent("null");
  });

  it("renders step navigation pills", async () => {
    render(
      <PipelineTraceTimeline
        steps={[
          {
            order: 1,
            name: "tenant.resolve",
            description: "Resolve tenant",
            durationMs: 0,
            input: null,
            output: {},
          },
          {
            order: 2,
            name: "conversation.load",
            description: "Load conversation",
            durationMs: 5,
            input: {},
            output: {},
          },
        ]}
      />,
    );

    expect(screen.getByTestId("pipeline-step-nav-1")).toBeInTheDocument();
    expect(screen.getByTestId("pipeline-step-nav-2")).toBeInTheDocument();

    await userEvent.click(screen.getByTestId("pipeline-step-1-toggle"));
    expect(screen.getByTestId("pipeline-step-1")).toHaveAttribute("data-expanded", "false");

    await userEvent.click(screen.getByTestId("pipeline-step-nav-2"));
    expect(screen.getByTestId("pipeline-step-2")).toHaveAttribute("data-expanded", "true");
  });

  it("renders nested search steps from output.nestedSteps", () => {
    render(
      <PipelineTraceTimeline
        steps={[
          {
            order: 3,
            name: "search.retrieve_context",
            description: "Search service",
            durationMs: 45,
            input: { question: "What is the policy?" },
            output: {
              resultCount: 2,
              nestedSteps: [
                {
                  order: 1,
                  name: "embedding.generate",
                  description: "Generate embedding",
                  durationMs: 20,
                  input: { query: "What is the policy?" },
                  output: { dimensions: 1536 },
                },
              ],
            },
          },
        ]}
      />,
    );

    expect(screen.getByText("Nested search pipeline")).toBeInTheDocument();
    expect(screen.getByTestId("pipeline-step-1-nested")).toBeInTheDocument();
  });
});
