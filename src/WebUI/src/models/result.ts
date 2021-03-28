export class Result<TData> {
  public errors: Error[] | null;
  public data: TData | null;
}

export class Error {
  public traceId: string | null;
  public type: ErrorType;
  public code: string;
  public title: string | null;
  public detail: string | null;
  // TODO: errorSource
  public stackTrace: string | null;
}

export enum ErrorType {
  InternalError = 'InternalError',
  Forbidden = 'Forbidden',
  Conflict = 'Conflict',
  NotFound = 'NotFound',
  Validation = 'Validation',
}
