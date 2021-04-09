export function arrayMergeBy<TValue, TKey>(a: TValue[], b: TValue[], getKey: (v: TValue) => TKey) {
  return a.map(el => {
    const elKey = getKey(el);
    const otherEl = b.find(e => getKey(e) === elKey);
    return otherEl !== undefined ? otherEl : el;
  });
}

export function arrayRemove<T>(arr: T[], predicate: (value: T) => boolean) {
  const idx = arr.findIndex(predicate);
  if (idx !== -1) {
    arr.splice(idx, 1);
  }
}
