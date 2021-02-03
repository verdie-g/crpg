const ellipsis = '...';

export function stringTruncate(str: string, maxLength: number) {
  if (str.length < ellipsis.length) {
    return ellipsis;
  }

  if (str.length < maxLength) {
    return str;
  }

  return str.substr(0, maxLength - ellipsis.length) + ellipsis;
}
