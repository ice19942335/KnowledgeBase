import { describe, expect, it, vi, beforeEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { useSearchSessionStore } from "./searchSessionStore";
import { useSemanticSearch } from "./useSemanticSearch";

vi.mock("../api/searchApi", () => ({
  searchChunks: vi.fn(async (query: string) => [
    {
      documentId: "11111111-1111-1111-1111-111111111111",
      documentName: "HR Policy",
      chunkIndex: 0,
      content: `Result for ${query}`,
      score: 0.91,
    },
  ]),
}));

function createQueryWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}

describe("useSemanticSearch", () => {
  beforeEach(() => {
    useSearchSessionStore.setState({
      variables: undefined,
      data: undefined,
      hasError: false,
    });
  });

  it("keeps mutation results after the hook unmounts", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { mutations: { retry: false } },
    });
    const wrapper = createQueryWrapper(queryClient);

    const first = renderHook(() => useSemanticSearch(), { wrapper });

    first.result.current.mutate("probation period");
    await waitFor(() => expect(first.result.current.isSuccess).toBe(true));
    expect(first.result.current.data?.[0]?.content).toContain("probation period");

    first.unmount();

    const second = renderHook(() => useSemanticSearch(), { wrapper });
    expect(second.result.current.data?.[0]?.content).toContain("probation period");
    expect(second.result.current.variables).toBe("probation period");
  });
});
