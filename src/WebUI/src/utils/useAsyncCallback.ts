// TODO: use https://github.com/vueuse/vueuse/issues/2890

import { makeDestructurable } from '@vueuse/shared';

export type AnyPromiseFn = (...args: any[]) => Promise<any>;

export type UseAsyncCallbackReturn<Fn extends AnyPromiseFn> = readonly [
  Fn,
  Ref<boolean>,
  Ref<any>
] & {
  execute: Fn;
  loading: Ref<boolean>;
  error: Ref<any>;
};

/**
 * Using async functions
 *
 * @see https://vueuse.org/useAsyncCallback
 * @param fn
 */
export function useAsyncCallback<T extends AnyPromiseFn>(fn: T): UseAsyncCallbackReturn<T> {
  const error = ref();
  const loading = ref(false);

  const execute = (async (...args: any[]) => {
    try {
      loading.value = true;
      const result = await fn(...args);
      loading.value = false;
      return result;
    } catch (err) {
      loading.value = false;
      error.value = err;
      throw err;
    }
  }) as T;

  return makeDestructurable(
    { execute, loading, error } as const,
    [execute, loading, error] as const
  );
}
