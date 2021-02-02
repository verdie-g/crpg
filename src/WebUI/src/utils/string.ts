const ellipsis = '...';

export function stringTruncate(str: string, maxLength: number) {
  if (str.length < ellipsis.length) {
    return ellipsis;
  }

  const length = Math.min(str.length, maxLength) - ellipsis.length;
  return str.substr(0, length) + ellipsis;
}
