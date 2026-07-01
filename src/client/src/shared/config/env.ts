/**
 * API paths in the client already include the `/api` prefix (e.g. `/api/documents`).
 * The base URL must be the site origin only, not `.../api`.
 */
export function normalizeApiBaseUrl(value: string | undefined): string {
  const trimmed = (value ?? "").trim();
  if (!trimmed) {
    return "";
  }

  return trimmed.replace(/\/api\/?$/i, "");
}

export const env = {
  apiBaseUrl: normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL),
} as const;
