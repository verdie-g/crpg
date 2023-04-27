import { mergeObjectWithSum, flatten } from './object';

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

interface TreeNode {
  id: number;
  children?: TreeNode[];
}

describe('flatten', () => {
  it('object', () => {
    const nested: TreeNode = { id: 1, children: [{ id: 2 }] };
    const flat = [{ id: 1 }, { id: 2 }];

    expect(flatten(nested)).toEqual(flat);
  });

  it('array', () => {
    const nested: TreeNode[] = [{ id: 1, children: [{ id: 2 }] }];
    const flat = [{ id: 1 }, { id: 2 }];

    expect(flatten(nested)).toEqual(flat);
  });
});
