export const sleep = (durationMs: number) =>
  new Promise(resolve => setTimeout(resolve, durationMs));
