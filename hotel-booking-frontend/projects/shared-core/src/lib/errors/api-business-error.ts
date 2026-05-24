export class ApiBusinessError extends Error {
  override readonly name = 'ApiBusinessError';

  constructor(
    message?: string | null,
    public readonly errors?: Array<string> | null
  ) {
    super(message ?? 'Request failed');
  }
}
