export function recordFilter<K extends keyof any, T>(r: Record<K, T>, f: (value: K) => boolean) {
  return Object.fromEntries((Object.entries(r) as Array<[K, T]>).filter(([k]) => f(k)));
}
