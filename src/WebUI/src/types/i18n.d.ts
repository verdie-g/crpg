import { type DefineLocaleMessage } from 'vue-i18n';

import {
  ItemFamilyType,
  ItemFlags,
  ItemType,
  ItemUsage,
  WeaponClass,
  WeaponFlags,
  type ItemFlat,
} from '@/models/item';
import { Region } from '@/models/region';

declare module 'vue-i18n' {
  // define the locale messages schema
  export interface DefineLocaleMessage {
    hello: string;
    item: {
      type: Record<ItemType, string>;
      flags: Record<ItemFlags, string>;
      usage: Record<ItemUsage, string>;
      weaponClass: Record<WeaponClass, string>;
      weaponFlags: Record<WeaponFlags, string>;
      familyType: Record<ItemFamilyType, any>;
      aggregations: Record<
        keyof ItemFlat,
        {
          title: string;
          description: string;
        }
      >;
    };
    region: Record<Region, string>;
  }

  // // define the datetime format schema
  // export interface DefineDateTimeFormat {
  //   short: {
  //     hour: 'numeric'
  //     minute: 'numeric'
  //     second: 'numeric'
  //     timeZoneName: 'short'
  //     timezone: string
  //   }
  // }

  // // define the number format schema
  // export interface DefineNumberFormat {
  //   currency: {
  //     style: 'currency'
  //     currencyDisplay: 'symbol'
  //     currency: string
  //   }
  // }
}
