import { mockGet } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';
import {
  type ItemFlat,
  ItemType,
  type Item,
  WeaponClass,
  ItemUsage,
  ItemFlags,
  WeaponFlags,
  ItemFieldFormat,
  ItemFieldCompareRule,
  ItemSlot,
  ItemFamilyType,
} from '@/models/item';
import { type AggregationConfig, AggregationView } from '@/models/item-search';

vi.mock('@/services/item-search-service/aggregations.ts', () => ({
  aggregationsConfig: {
    type: {
      title: 'Type',
      view: AggregationView.Checkbox,
    },
    flags: {
      title: 'Flags',
      view: AggregationView.Checkbox,
      format: ItemFieldFormat.List,
    },
    price: {
      title: 'Price',
      view: AggregationView.Range,
      format: ItemFieldFormat.Number,
    },
    thrustDamage: {
      title: 'Thrust damage',
      view: AggregationView.Range,
      format: ItemFieldFormat.Damage,
    },
    swingDamage: {
      title: 'Swing damage',
      view: AggregationView.Range,
      format: ItemFieldFormat.Damage,
    },
    damage: {
      title: 'Damage',
      view: AggregationView.Range,
      format: ItemFieldFormat.Damage,
    },
  } as AggregationConfig,
}));

import mockItems from '@/__mocks__/items.json';

import {
  getItems,
  getItemImage,
  getAvailableSlotsByItem,
  hasWeaponClassesByItemType,
  getWeaponClassesByItemType,
  // itemFieldFormatter,
  getCompareItemsResult,
  // humanizeBucket,
  getItemFieldDiffStr,
} from './item-service';
import { UserItem } from '@/models/user';

it('getItems', async () => {
  mockGet('/items').willResolve(response<Item[]>(mockItems as Item[]));

  expect(await getItems()).toEqual(mockItems);
});

it('getItemImage', () => {
  expect(getItemImage('123')).toEqual('http://localhost:8081/items/123.png'); // TODO: FIXME:
});

it('getItemImage', () => {
  expect(getItemImage('123')).toEqual('http://localhost:8081/items/123.png'); // TODO: FIXME:
});

it.each<[Item, Record<ItemSlot, UserItem>, ItemSlot[]]>(
  // prettier-ignore
  [
    [
      { type: ItemType.MountHarness, armor: { familyType: ItemFamilyType.Horse }, } as Item,
      {} as Record<ItemSlot, UserItem>,
      [ItemSlot.MountHarness],
    ],
    [
      { type: ItemType.MountHarness, armor: { familyType: ItemFamilyType.Horse }, } as Item,
      { [ItemSlot.Mount]: { baseItem: { mount: { familyType: ItemFamilyType.Horse }}} } as Record<ItemSlot, UserItem>,
      [ItemSlot.MountHarness],
    ],
    [
      { type: ItemType.MountHarness, armor: { familyType: ItemFamilyType.Camel }, } as Item,
      {} as Record<ItemSlot, UserItem>,
      [ItemSlot.MountHarness],
    ],
    [
      { type: ItemType.MountHarness, armor: { familyType: ItemFamilyType.Camel }, } as Item,
      { [ItemSlot.Mount]: { baseItem: { mount: { familyType: ItemFamilyType.Horse }}} } as Record<ItemSlot, UserItem>,
      [],
    ],
    [
      { type: ItemType.MountHarness, armor: { familyType: ItemFamilyType.Horse }, } as Item,
      { [ItemSlot.Mount]: { baseItem: { mount: { familyType: ItemFamilyType.Camel }}} } as Record<ItemSlot, UserItem>,
      [],
    ],
    [
      { type: ItemType.OneHandedWeapon } as Item,
      {} as Record<ItemSlot, UserItem>,
      [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3],
    ],
    [
      { type: ItemType.Banner } as Item,
      {} as Record<ItemSlot, UserItem>,
      [ItemSlot.WeaponExtra],
    ],
  ]
)('getAvailableSlotsByItem - item: %j, equipedItems: %j', (item, equipedItems, expectation) => {
  expect(getAvailableSlotsByItem(item, equipedItems)).toEqual(expectation);
});

it.each([
  [ItemType.Bow, false],
  [ItemType.Polearm, true],
])('hasWeaponClassesByItemType - itemType: %s', (itemType, expectation) => {
  expect(hasWeaponClassesByItemType(itemType)).toEqual(expectation);
});

it.each<[ItemType, WeaponClass[]]>([
  [
    ItemType.OneHandedWeapon,
    [WeaponClass.OneHandedSword, WeaponClass.OneHandedAxe, WeaponClass.Mace, WeaponClass.Dagger],
  ],
  [ItemType.Bow, []],
])('getWeaponClassesByItemType - itemType: %s', (itemType, expectation) => {
  expect(getWeaponClassesByItemType(itemType)).toEqual(expectation);
});

// it.each<[keyof ItemFlat, string, string]>([
//   ['type', ItemType.OneHandedWeapon, 'item.type.OneHandedWeapon'],
//   ['weaponClass', WeaponClass.OneHandedPolearm, 'item.weaponClass.OneHandedPolearm'],
//   ['flags', ItemUsage.PolearmCouch, 'item.usage.polearm_couch'],
//   ['flags', ItemFlags.UseTeamColor, 'item.flags.UseTeamColor'],
//   ['flags', WeaponFlags.CanDismount, 'item.weaponFlags.CanDismount'],
//   ['handling', '123', '123'],
// ])('humanizeBucket - aggKey: %s, bucket: %s ', (aggKey, bucket, expectation) => {
//   expect(humanizeBucket(aggKey, bucket)).toEqual(expectation);
// });

// describe('itemFieldFormatter', () => {
//   const item = {
//     type: ItemType.OneHandedWeapon,
//     flags: ['UseTeamColor', 'Civilian'],
//     thrustDamage: 21,
//     thrustDamageType: 'Pierce',
//     swingDamage: 12,
//     swingDamageType: 'Cut',
//     damage: 12,
//     damageType: 'Blunt',
//     price: 1234,
//     length: 122,
//   } as ItemFlat;

//   it('list', () => {
//     expect(itemFieldFormatter('flags', item)).toEqual(
//       'item.flags.UseTeamColor, item.flags.Civilian'
//     );
//   });

//   it('damage', () => {
//     expect(itemFieldFormatter('thrustDamage', item)).toEqual('21 p');
//     expect(itemFieldFormatter('swingDamage', item)).toEqual('12 c');
//     expect(itemFieldFormatter('damage', item)).toEqual('12 p');
//   });

//   it('price', () => {
//     expect(itemFieldFormatter('price', item)).toEqual('1,234');
//   });

//   it('no format', () => {
//     expect(itemFieldFormatter('length', item)).toEqual(122);
//   });
//   it('no format + humanize', () => {
//     expect(itemFieldFormatter('type', item)).toEqual('item.type.OneHandedWeapon');
//   });
// });

it('compareItemsResult', () => {
  const items = [
    {
      weight: 2.22,
      length: 105,
      handling: 82,
      flags: ['UseTeamColor'],
    },
    {
      weight: 2.07,
      length: 99,
      handling: 86,
      flags: [],
    },
  ] as ItemFlat[];

  const aggregationConfig = {
    length: {
      title: 'Length',
      compareRule: 'Bigger',
    },
    flags: {
      title: 'Flags',
    },
    handling: {
      title: 'Handling',
      compareRule: 'Bigger',
    },
    weight: {
      title: 'Thrust damage',
      compareRule: 'Less',
    },
  } as AggregationConfig;

  expect(getCompareItemsResult(items, aggregationConfig)).toEqual({
    length: 105,
    handling: 86,
    weight: 2.07,
  });
});

it.each<[ItemFieldCompareRule, number, number, string]>([
  [ItemFieldCompareRule.Bigger, 0, 0, ''],
  [ItemFieldCompareRule.Bigger, -1, 0, '-1'],
  [ItemFieldCompareRule.Bigger, -1, -1, ''],

  [ItemFieldCompareRule.Bigger, 1, 1, ''],
  [ItemFieldCompareRule.Bigger, 2, 1, ''],
  [ItemFieldCompareRule.Bigger, 1, 2, '-1'],

  [ItemFieldCompareRule.Less, 0, 0, ''],
  [ItemFieldCompareRule.Less, 0, -1, '+1'],
  [ItemFieldCompareRule.Less, -1, -1, ''],

  [ItemFieldCompareRule.Less, 1, 1, ''],
  [ItemFieldCompareRule.Less, 1, 2, ''],
  [ItemFieldCompareRule.Less, 2, 1, '+1'],
])(
  'getItemFieldDiffStr - compareRule: %s, value: %s, bestValue: %s',
  (compareRule, value, bestValue, expectation) => {
    expect(getItemFieldDiffStr(compareRule, value, bestValue)).toEqual(expectation);
  }
);
