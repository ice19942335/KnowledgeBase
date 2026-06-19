import { httpClient } from "../../../shared/api/httpClient";
import type { ChatAnswer } from "../model/types";

export async function askQuestion(question: string): Promise<ChatAnswer> {
  const response = await httpClient.post<ChatAnswer>("/api/chat", { question });
  return response.data;
}
