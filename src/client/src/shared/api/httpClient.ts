import axios from "axios";
import { env } from "../config/env";

const devTenantId = "11111111-1111-1111-1111-111111111111";

export const httpClient = axios.create({
  baseURL: import.meta.env.DEV ? "" : env.apiBaseUrl,
});

httpClient.interceptors.request.use((config) => {
  if (import.meta.env.DEV) {
    config.headers.set("X-Tenant-Id", devTenantId);
  }

  return config;
});
