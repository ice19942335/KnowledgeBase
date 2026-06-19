import { useQuery } from "@tanstack/react-query";
import { fetchAiStatus } from "../api/aiStatusApi";

export const aiStatusQueryKey = ["ai", "status"] as const;

export function useAiStatus() {
  return useQuery({
    queryKey: aiStatusQueryKey,
    queryFn: fetchAiStatus,
    staleTime: 60_000,
  });
}
