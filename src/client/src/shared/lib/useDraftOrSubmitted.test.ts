import { describe, expect, it } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useDraftOrSubmitted } from "./useDraftOrSubmitted";

describe("useDraftOrSubmitted", () => {
  it("shows last submitted value when no draft exists", () => {
    const { result, rerender } = renderHook(
      ({ lastSubmitted }) => useDraftOrSubmitted(lastSubmitted),
      { initialProps: { lastSubmitted: undefined as string | undefined } },
    );

    expect(result.current.value).toBe("");

    rerender({ lastSubmitted: "vacation policy" });
    expect(result.current.value).toBe("vacation policy");
  });

  it("prefers draft over last submitted while editing", () => {
    const { result } = renderHook(() => useDraftOrSubmitted("vacation policy"));

    act(() => {
      result.current.setValue("new question");
    });

    expect(result.current.value).toBe("new question");
  });

  it("falls back to last submitted after clearDraft", () => {
    const { result } = renderHook(() => useDraftOrSubmitted("vacation policy"));

    act(() => {
      result.current.setValue("draft text");
      result.current.clearDraft();
    });

    expect(result.current.value).toBe("vacation policy");
  });
});
