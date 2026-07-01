import { describe, expect, it } from "vitest";
import { normalizeApiBaseUrl } from "./env";

describe("normalizeApiBaseUrl", () => {
  it("returns empty string when unset", () => {
    expect(normalizeApiBaseUrl(undefined)).toBe("");
    expect(normalizeApiBaseUrl("")).toBe("");
    expect(normalizeApiBaseUrl("   ")).toBe("");
  });

  it("strips trailing /api from the configured base URL", () => {
    expect(normalizeApiBaseUrl("https://kb.bookly.lv/api")).toBe("https://kb.bookly.lv");
    expect(normalizeApiBaseUrl("https://kb.bookly.lv/api/")).toBe("https://kb.bookly.lv");
  });

  it("keeps origin-only base URLs unchanged", () => {
    expect(normalizeApiBaseUrl("https://kb.bookly.lv")).toBe("https://kb.bookly.lv");
  });
});
