import { httpClient } from "../../../shared/api/httpClient";

export type AiStatus = {
  isConfigured: boolean;
  message: string;
};

export async function fetchAiStatus(): Promise<AiStatus> {
  const response = await httpClient.get<AiStatus>("/api/chat/ai/status");
  return response.data;
}
