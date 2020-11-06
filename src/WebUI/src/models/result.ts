import Error from '@/models/error';

export default class Result<TData> {
  public errors: Error[] | null;
  public data: TData | null;
}
