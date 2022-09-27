export function applyPolynomialFunction(x: number, coefficients: number[]): number {
  let r = 0;
  for (let degree = 0; degree < coefficients.length; degree += 1) {
    r += coefficients[coefficients.length - degree - 1] * x ** degree;
  }

  return r;
}
export function generalizedMean(n: number, numbers: number[]): number {
  const normN =
    numbers.reduce((accumulator, current) => {
      return accumulator + Math.pow(current, n);
    }, 0) / n;
  const generalizedMean = Math.pow(normN, 1 / n);
  return generalizedMean;
}
