import { useSessionMutation } from "../../../shared/lib/useSessionMutation";
import { askQuestion } from "../api/chatApi";
import { useChatSessionStore } from "./chatSessionStore";

export function useRagChat() {
  return useSessionMutation(useChatSessionStore, {
    mutationFn: (question: string) => askQuestion(question),
  });
}
