export function hexColorToArgbInt(hexColor: string): number {
  return parseInt(hexColor.substr(1), 16);
}

export function argbIntToHexColor(argb: number) {
  return '#' + argb.toString(16).padStart(8, '0');
}
