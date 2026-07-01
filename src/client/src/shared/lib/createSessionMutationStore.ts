import { create } from "zustand";

export interface SessionMutationState<TData, TVariables> {
  variables: TVariables | undefined;
  data: TData | undefined;
  hasError: boolean;
  setSuccess: (variables: TVariables, data: TData) => void;
  setError: () => void;
  clearError: () => void;
}

export function createSessionMutationStore<TData, TVariables>() {
  return create<SessionMutationState<TData, TVariables>>((set) => ({
    variables: undefined,
    data: undefined,
    hasError: false,
    setSuccess: (variables, data) => set({ variables, data, hasError: false }),
    setError: () => set({ hasError: true }),
    clearError: () => set({ hasError: false }),
  }));
}
