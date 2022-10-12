import { rgbHexColorToArgbInt, argbIntToRgbHexColor } from './color';

const CASES = [
  ['#ffffff', 4294967295],
  ['#000000', 4278190080],
  ['#cccccc', 4291611852],
  ['#f8c555', 4294493525],
  ['#7ec699', 4286498457],
];

describe('color', () => {
  it.each(CASES)('rgbHexColorToArgbInt', (a, b) => {
    expect(rgbHexColorToArgbInt(a as string)).toBe(b);
  });

  it.each(CASES)('argbIntToRgbHexColor', (a, b) => {
    expect(argbIntToRgbHexColor(b as number)).toBe(a);
  });
});
