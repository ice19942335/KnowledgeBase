import axios from "axios";
import { env } from "../config/env";

// Until Google OAuth is wired in the client, mirror the backend dev tenant default.
const defaultTenantId = "11111111-1111-1111-1111-111111111111";

export const httpClient = axios.create({
  baseURL: import.meta.env.DEV ? "" : env.apiBaseUrl,
});

httpClient.interceptors.request.use((config) => {
  if (!config.headers.has("X-Tenant-Id")) {
    config.headers.set("X-Tenant-Id", defaultTenantId);
  }

  return config;
});
