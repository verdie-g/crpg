import type { Result } from '@/models/crpg-client-result';

export const response = <T>(data: T): Result<T> => ({
  data,
  errors: null,
});
