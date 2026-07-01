import { useSessionMutation } from "../../../shared/lib/useSessionMutation";
import { askQuestionWithTrace } from "../../../entities/explorer/api/explorerApi";
import { usePipelineChatSessionStore } from "./pipelineChatSessionStore";

export function usePipelineChat() {
  return useSessionMutation(usePipelineChatSessionStore, {
    mutationFn: (question: string) => askQuestionWithTrace(question),
  });
}
