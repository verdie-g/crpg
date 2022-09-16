export function mapFilter<K extends keyof any, T>(m: Map<K, T>, f: (value: K) => boolean) {
  m.forEach((value, key) => {
    if (!f(key)) m.delete(key);
  });
  return m;
}
