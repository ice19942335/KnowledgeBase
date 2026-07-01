import { createSessionMutationStore } from "../../../shared/lib/createSessionMutationStore";
import type { ChatTraceAnswer } from "../../../entities/explorer/model/types";

export const usePipelineChatSessionStore =
  createSessionMutationStore<ChatTraceAnswer, string>();
