export function debounce(
  callback: (...args: any) => any,
  wait: number,
  immediate = false
): (...args: any) => any {
  let interval: number | undefined;

  return function (...args: any[]) {
    clearTimeout(interval);

    interval = setTimeout(() => {
      interval = undefined;
      if (!immediate) callback(args);
    }, wait);

    if (immediate && !interval) {
      callback(args)
    }
  };
}
