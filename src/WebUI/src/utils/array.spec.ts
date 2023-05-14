import { range } from './array';

it('range', () => {
  expect(range(3, 6)).toEqual([3, 4, 5, 6]);
});
