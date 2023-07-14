import { Item, WeaponUsage } from '@/models/item';
import {
  Longsword,
  Pike,
  Hoe,
  Shield,
  Bow,
  Bolts,
  ThrowingAxe,
  MountHarness,
  Mount,
  Helmet,
} from './mocks';

const { mockedIsLargeShield, mockedComputeAverageRepairCostPerHour } = vi.hoisted(() => ({
  mockedIsLargeShield: vi.fn(),
  mockedComputeAverageRepairCostPerHour: vi.fn((price: number) => price * 2),
}));

vi.mock('@/services/item-service', async () => ({
  ...(await vi.importActual<typeof import('@/services/item-service')>('@/services/item-service')),
  itemIsNewDays: 30,
  isLargeShield: mockedIsLargeShield,
  computeAverageRepairCostPerHour: mockedComputeAverageRepairCostPerHour,
}));

import { createItemIndex } from '../indexator';

describe('createItemIndex', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime('2023-07-13T21:43:44.0741909Z');
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('weapons > 1, same weaponClass', () => {
    const index = createItemIndex([Pike as Item], true);

    expect(index.length).toEqual(1);
    const [item] = index;

    expect(item.modId).toEqual('crpg_vlandia_pike_1_t5_Polearm_TwoHandedPolearm');
    // merge: weaponUsage + weaponFlags + flags => flags
    expect(item.flags).toContain('polearm_pike');
    expect(item.flags).toContain('polearm_bracing');
    expect(item.weaponUsage).toEqual([WeaponUsage.Primary]);
    expect(item.swingDamageType).toEqual(undefined);
  });

  it('weapons > 1, diff weaponClass - it is necessary to clone the item', () => {
    const index = createItemIndex([Hoe as Item], true);

    expect(index.length).toEqual(2);
    const [item1, item2] = index;

    expect(item1.modId).toEqual('crpg_peasant_2haxe_1_t1_OneHandedWeapon_OneHandedAxe');
    expect(item1.type).toEqual('OneHandedWeapon');
    expect(item1.weaponClass).toEqual('OneHandedAxe');
    expect(item1.weaponUsage).toEqual([WeaponUsage.Secondary]);
    expect(item1.thrustDamageType).toEqual(undefined);

    expect(item2.modId).toEqual('crpg_peasant_2haxe_1_t1_TwoHandedWeapon_TwoHandedAxe');
    expect(item2.type).toEqual('TwoHandedWeapon');
    expect(item2.weaponClass).toEqual('TwoHandedAxe');
    expect(item2.weaponUsage).toEqual([WeaponUsage.Primary]);
    expect(item2.thrustDamageType).toEqual(undefined);
  });

  it('shield', () => {
    mockedIsLargeShield.mockReturnValueOnce(true);
    const index = createItemIndex([Shield as Item]);

    const [item] = index;

    expect(item.shieldDurability).toEqual(70);
    expect(item.shieldSpeed).toEqual(82);
    expect(item.shieldArmor).toEqual(0);
    expect(item.weaponClass).toEqual('LargeShield');
    expect(item.weaponPrimaryClass).toEqual('LargeShield');
    expect(item.flags).contains('CantUseOnHorseback');
  });

  it('Shield - CantUseOnHorseback', () => {
    mockedIsLargeShield.mockReturnValueOnce(false);
    const index = createItemIndex([Shield as Item]);

    const [item] = index;

    expect(item.flags).not.contains('CantUseOnHorseback');
  });

  it('Shield - upkeep', () => {
    mockedIsLargeShield.mockReturnValueOnce(true);
    const index = createItemIndex([Shield as Item]);

    const [item] = index;

    expect(item.upkeep).toEqual(item.price * 2);
  });

  it('bow', () => {
    const index = createItemIndex([Bow as Item]);

    const [item] = index;

    expect(item.reloadSpeed).toEqual(98);
    expect(item.aimSpeed).toEqual(100);
    expect(item.damage).toEqual(14);
  });

  it('bolts', () => {
    const index = createItemIndex([Bolts as Item]);

    const [item] = index;

    expect(item.damage).toEqual(14);
    expect(item.damageType).toEqual('Pierce');
    expect(item.weight).toEqual(null);
    expect(item.stackWeight).toEqual(1.5);
  });

  it('throwing axe', () => {
    const index = createItemIndex([ThrowingAxe as Item], true);

    const [item1, item2] = index;

    expect(item1.type).toEqual('OneHandedWeapon');
    expect(item1.weaponClass).toEqual('OneHandedAxe');

    expect(item2.type).toEqual('Thrown');
    expect(item2.weaponClass).toEqual('ThrowingAxe');

    expect(item2.damage).toEqual(32);
    expect(item2.damageType).toEqual(undefined); // TODO:
    expect(item2.weight).toEqual(null);
    expect(item2.stackWeight).toEqual(3.75);
  });

  it('mount harness', () => {
    const index = createItemIndex([MountHarness as Item]);

    const [item] = index;

    expect(item.mountArmor).toEqual(6);
    expect(item.mountArmorFamilyType).toEqual(1);
  });

  it('mount', () => {
    const index = createItemIndex([Mount as Item]);

    const [item] = index;

    expect(item.bodyLength).toEqual(105);
    expect(item.chargeDamage).toEqual(1);
    expect(item.maneuver).toEqual(66);
    expect(item.speed).toEqual(40);
    expect(item.hitPoints).toEqual(172);
    expect(item.mountFamilyType).toEqual(1);
  });

  it('helmet', () => {
    const index = createItemIndex([Helmet as Item]);

    const [item] = index;

    expect(item.mountArmor).toEqual(null);
    expect(item.headArmor).toEqual(64);
    expect(item.modId).toEqual('crpg_sa_1ChurburghHelm_HeadArmor');
  });

  it('isNew', () => {
    const index = createItemIndex([Helmet, Longsword] as Item[]);

    const [item1, item2] = index;

    expect(item1.new).toEqual(1);
    expect(item2.new).toEqual(0);
  });
});
