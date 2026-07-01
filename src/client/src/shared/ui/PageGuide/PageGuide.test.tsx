import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { PageGuide } from "./PageGuide";

describe("PageGuide", () => {
  it("renders summary and expandable pipeline steps", () => {
    render(
      <PageGuide
        testId="documents-guide"
        summary="Upload files to your knowledge base."
        steps={[
          { title: "Upload", description: "Document service stores the file." },
          { title: "Ingest", description: "Worker chunks and embeds text." },
        ]}
      />,
    );

    expect(screen.getByTestId("documents-guide")).toBeInTheDocument();
    expect(screen.getByText("Upload files to your knowledge base.")).toBeInTheDocument();
    expect(screen.getByText("How it works under the hood")).toBeInTheDocument();
    expect(screen.getByText("Upload")).toBeInTheDocument();
    expect(screen.getByText("Document service stores the file.")).toBeInTheDocument();
  });
});
