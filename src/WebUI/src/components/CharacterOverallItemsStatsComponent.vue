<template>
  <div>
    <table width="300px">
      <td><b>Price</b></td>
      <td>
        {{ itemStats.price.toLocaleString('en-US') }}
        <b-icon icon="coins" size="is-small" />
      </td>
      <tr>
        <td><b>Average repair cost</b></td>
        <td>
          {{ itemStats.averageRepairCost.toLocaleString('en-US', { maximumFractionDigits: 2 }) }} /
          hour
          <b-icon icon="coins" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Health Points</b></td>
        <td>
          {{ healthPoints }}
          <b-icon icon="heart" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Head Armor</b></td>
        <td>
          {{ itemStats.headArmor }}
          <b-icon icon="shield-alt" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Body Armor</b></td>
        <td>
          {{ itemStats.bodyArmor }}
          <b-icon icon="shield-alt" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Arm Armor</b></td>
        <td>
          {{ itemStats.armArmor }}
          <b-icon icon="shield-alt" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Leg Armor</b></td>
        <td>
          {{ itemStats.legArmor }}
          <b-icon icon="shield-alt" size="is-small" />
        </td>
      </tr>

      <template v-if="speedStats">
        <tr>
          <td><b>Weight</b></td>
          <td>
            {{ itemStats.weight.toLocaleString('en-US', { maximumFractionDigits: 2 }) }}
            <b-icon icon="weight-hanging" size="is-small" />
          </td>
        </tr>
        <tr>
          <td><b>Free weight</b></td>
          <td>
            {{
              Math.min(itemStats.weight, speedStats.freeWeight).toLocaleString('en-US') +
              ' / ' +
              speedStats.freeWeight.toLocaleString('en-US')
            }}
            <b-icon icon="weight-hanging" size="is-small" />
          </td>
        </tr>
        <tr>
          <b-tooltip
            label="The remaining weight after deduction by Free Weight , get reduced by your Strenght Attribute"
            multilined
          >
            <td><b>Weight Reduction</b></td>
          </b-tooltip>
          <td>
            {{ ((1 - speedStats.weightReductionFactor) * 100).toLocaleString('en-US') + ' %' }}
            <b-icon icon="weight-hanging" size="is-small" />
          </td>
        </tr>
        <tr>
          <td><b>Perceived weight</b></td>
          <td>
            {{ speedStats.perceivedWeight.toLocaleString('en-US') }}
            <b-icon icon="weight-hanging" size="is-small" />
          </td>
        </tr>

        <tr>
          <td><b>Time to max speed</b></td>
          <td>
            {{ speedStats.timeToMaxSpeed.toLocaleString('en-US') }} s
            <b-icon icon="time" size="is-small" />
          </td>
        </tr>

        <tr>
          <td><b>Naked Speed</b></td>
          <td>
            {{ speedStats.nakedSpeed.toLocaleString('en-US') }}
            <b-icon icon="running" size="is-small" />
          </td>
        </tr>

        <tr>
          <td><b>Current Speed</b></td>
          <td>
            {{ speedStats.сurrentSpeed.toLocaleString('en-US') }}
            <b-icon icon="running" size="is-small" />
          </td>
        </tr>
        <tr>
          <td><b>Max Weapon Length Without Penalty</b></td>
          <td>
            {{ speedStats.maxWeaponLength.toLocaleString('en-US') }}
            <b-icon icon="running" size="is-small" />
          </td>
        </tr>
        <tr>
          <td><b>Extra Movement Speed When Attacking Penalty</b></td>
          <td>
            {{ speedStats.movementSpeedPenaltyWhenAttacking.toLocaleString('en-US') }} %
            <b-icon icon="running" size="is-small" />
          </td>
        </tr>
      </template>
    </table>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import EquippedItem from '@/models/equipped-item';
import Character from '@/models/character';
import type CharacterCharacteristics from '@/models/character-characteristics';
import type CharacterSpeedStats from '@/models/сharacter-speed-stats';
import { computeAverageRepairCostByHour } from '@/services/item-service';
import { computeHealthPoints, computeSpeedStats } from '@/services/characters-service';
import ItemType from '@/models/item-type';

@Component
export default class CharacterOverallItemsStatsComponent extends Vue {
  @Prop(Object) readonly character: Character;
  @Prop(Array) readonly equippedItems: EquippedItem[] | null;

  get characteristics() {
    return this.$store.state.user.characteristicsByCharacterId[
      this.character.id
    ] as CharacterCharacteristics;
  }

  get healthPoints(): number {
    if (!this.characteristics) {
      return 0;
    }

    return computeHealthPoints(
      this.characteristics.skills.ironFlesh,
      this.characteristics.attributes.strength
    );
  }

  get speedStats(): CharacterSpeedStats | null {
    if (!this.characteristics) return null;
    return computeSpeedStats({
      strength: this.characteristics.attributes.strength,
      agility: this.characteristics.attributes.agility,
      athletics: this.characteristics.skills.athletics,
      totalEncumbrance: this.itemStats.weight,
      longestWeaponLength: this.itemStats.longestWeaponLength,
    });
  }

  get itemStats(): Record<string, number> {
    const result = {
      price: 0,
      averageRepairCost: 0,
      headArmor: 0,
      bodyArmor: 0,
      armArmor: 0,
      legArmor: 0,
      weight: 0,
      longestWeaponLength: 0,
    };

    if (!this.equippedItems) return result;
    result.averageRepairCost = computeAverageRepairCostByHour(
      this.equippedItems.map(item => item.userItem.baseItem)
    );
    this.equippedItems.forEach(ei => {
      result.price += ei.userItem.baseItem.price;
      if (
        ei.userItem.baseItem.type !== ItemType.Mount &&
        ei.userItem.baseItem.type !== ItemType.MountHarness
      ) {
        if (
          ei.userItem.baseItem.type == ItemType.Thrown ||
          ei.userItem.baseItem.type == ItemType.Bolts ||
          ei.userItem.baseItem.type == ItemType.Arrows
        ) {
          result.weight +=
            ei.userItem.baseItem.weight * ei.userItem.baseItem.weapons[0].stackAmount;
        } else {
          result.weight += ei.userItem.baseItem.weight;
        }
      }

      if (
        [ItemType.OneHandedWeapon, ItemType.TwoHandedWeapon, ItemType.Polearm].includes(
          ei.userItem.baseItem.type
        ) &&
        ei.userItem.baseItem.weapons.length !== 0
      ) {
        result.longestWeaponLength = Math.max(
          result.longestWeaponLength,
          ei.userItem.baseItem.weapons[0].length
        );
      }

      const armor = ei.userItem.baseItem.armor;
      if (armor && ei.userItem.baseItem.type !== ItemType.MountHarness) {
        result.headArmor += armor.headArmor;
        result.bodyArmor += armor.bodyArmor;
        result.armArmor += armor.armArmor;
        result.legArmor += armor.legArmor;
      }
    });

    return result as Record<string, number>;
  }
}
</script>

<style scoped lang="scss">
td {
  padding: 3px;
}
</style>
