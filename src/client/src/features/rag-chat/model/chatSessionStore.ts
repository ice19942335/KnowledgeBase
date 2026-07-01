import { createSessionMutationStore } from "../../../shared/lib/createSessionMutationStore";
import type { ChatAnswer } from "./types";

export const useChatSessionStore = createSessionMutationStore<ChatAnswer, string>();
