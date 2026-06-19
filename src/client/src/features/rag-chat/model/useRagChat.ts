import { useMutation } from "@tanstack/react-query";
import { askQuestion } from "../api/chatApi";

export function useRagChat() {
  return useMutation({
    mutationFn: (question: string) => askQuestion(question),
  });
}
