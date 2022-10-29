export function applyPolynomialFunction(x: number, coefficients: number[]): number {
  let r = 0;
  for (let degree = 0; degree < coefficients.length; degree += 1) {
    r += coefficients[coefficients.length - degree - 1] * x ** degree;
  }

  return r;
}

export function clamp(num: number, min: number, max: number): number {
  return Math.min(Math.max(num, min), max);
}
