export function rgbHexColorToArgbInt(hexColor: string): number {
  return parseInt('FF' + hexColor.substring(1), 16);
}

export function argbIntToRgbHexColor(argb: number) {
  return '#' + (argb & 0xffffff).toString(16).padStart(6, '0');
}
