import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Button } from "./Button";

describe("Button", () => {
  it("renders its children", () => {
    render(<Button>Upload</Button>);

    expect(screen.getByRole("button", { name: "Upload" })).toBeInTheDocument();
  });

  it("invokes onClick when pressed", async () => {
    const handleClick = vi.fn();
    render(<Button onClick={handleClick}>Click</Button>);

    await userEvent.click(screen.getByRole("button", { name: "Click" }));

    expect(handleClick).toHaveBeenCalledOnce();
  });

  it("does not invoke onClick when disabled", async () => {
    const handleClick = vi.fn();
    render(
      <Button onClick={handleClick} disabled>
        Disabled
      </Button>,
    );

    await userEvent.click(screen.getByRole("button", { name: "Disabled" }));

    expect(handleClick).not.toHaveBeenCalled();
  });
});
