import { httpClient } from "../../../shared/api/httpClient";
import type { ChatTraceAnswer, SearchExplorerResult } from "../model/types";

export async function fetchSearchExplorer(): Promise<SearchExplorerResult> {
  const response = await httpClient.get<SearchExplorerResult>("/api/search/explorer");
  return response.data;
}

export async function askQuestionWithTrace(question: string): Promise<ChatTraceAnswer> {
  const response = await httpClient.post<ChatTraceAnswer>("/api/chat/trace", { question });
  return response.data;
}
