import { useState } from "react";

export function useDraftOrSubmitted(lastSubmitted: string | undefined) {
  const [draft, setDraft] = useState<string | null>(null);
  const value = draft ?? lastSubmitted ?? "";

  return {
    value,
    setValue: setDraft,
    clearDraft: () => setDraft(null),
  };
}
