import { useMutation, type UseMutationOptions } from "@tanstack/react-query";
import type { StoreApi, UseBoundStore } from "zustand";
import type { SessionMutationState } from "./createSessionMutationStore";

type SessionStore<TData, TVariables> = UseBoundStore<
  StoreApi<SessionMutationState<TData, TVariables>>
>;

export function useSessionMutation<TData, TError, TVariables>(
  store: SessionStore<TData, TVariables>,
  options: UseMutationOptions<TData, TError, TVariables>,
) {
  const session = store();

  const mutation = useMutation({
    ...options,
    onMutate: async (variables, context) => {
      session.clearError();
      return options.onMutate?.(variables, context);
    },
    onSuccess: (data, variables, onMutateResult, context) => {
      session.setSuccess(variables, data);
      return options.onSuccess?.(data, variables, onMutateResult, context);
    },
    onError: (error, variables, onMutateResult, context) => {
      session.setError();
      return options.onError?.(error, variables, onMutateResult, context);
    },
  });

  return {
    ...mutation,
    data: mutation.data ?? session.data,
    variables: mutation.variables ?? session.variables,
    isError: mutation.isError || (session.hasError && !mutation.isPending),
  };
}
