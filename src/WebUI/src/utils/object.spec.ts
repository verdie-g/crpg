import { mergeObjectWithSum } from './object';

it('mergeObjectWithSum', () => {
  const obj1 = {
    applejack: 10,
    rarity: 1,
  };

  const obj2 = {
    applejack: 1,
    rarity: 0,
  };

  const obj3 = {
    applejack: 11,
    rarity: 1,
  };

  expect(mergeObjectWithSum(obj1, obj2)).toEqual(obj3);
});
