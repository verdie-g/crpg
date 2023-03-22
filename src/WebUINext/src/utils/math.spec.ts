import { clamp, roundFLoat } from './math';

it.each([
  [[0, 0, 0], 0],
  [[1, 1, 1], 1],
  [[5, 1, 6], 5],
])('clamp', ([num, min, max], expectation) => {
  expect(clamp(num, min, max)).toEqual(expectation);
});

it.each([
  [0, 0],
  [1, 1],
  [0.0001, 0],
  [0.4999, 0.5],
  [0.5001, 0.5],
  [0.5499, 0.55],
  [0.545, 0.55],
  [0.5449, 0.54],
  [1.1, 1.1],
  [1.1222, 1.12],
  [1.0001, 1],
  [1.1029, 1.1],
])('roundFLoat', (num, expectation) => {
  expect(roundFLoat(num)).toEqual(expectation);
});
