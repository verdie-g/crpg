export interface Result<TData> {
  errors: Error[] | null;
  data: TData | null;
}

export interface Error {
  traceId: string | null;
  type: ErrorType;
  code: string;
  title: string | null;
  detail: string | null;
  // TODO: errorSource
  stackTrace: string | null;
}

export enum ErrorType {
  InternalError = 'InternalError',
  Forbidden = 'Forbidden',
  Conflict = 'Conflict',
  NotFound = 'NotFound',
  Validation = 'Validation',
}
