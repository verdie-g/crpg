export const range = (start: number, end: number) =>
  Array(end - start + 1)
    .fill(null)
    .map((_, idx) => start + idx);
