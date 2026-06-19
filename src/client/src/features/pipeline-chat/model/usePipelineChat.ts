import { useMutation } from "@tanstack/react-query";
import { askQuestionWithTrace } from "../../../entities/explorer/api/explorerApi";

export function usePipelineChat() {
  return useMutation({
    mutationFn: (question: string) => askQuestionWithTrace(question),
  });
}
