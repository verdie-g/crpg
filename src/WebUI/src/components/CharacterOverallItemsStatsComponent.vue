<template>
  <div>
    <table width="300px">
      <tr>
        <td><b>Armor Set Requirement</b></td>
        <td>{{ itemStats.armorSetRequirement.toLocaleString('en-US') }} STR</td>
      </tr>
      <tr>
        <td><b>Price</b></td>
        <td>
          {{ itemStats.price.toLocaleString('en-US') }}
          <b-icon icon="coins" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Max repair costs</b></td>
        <td>
          {{ itemStats.maxRepairCost.toLocaleString('en-US') }}
          <b-icon icon="coins" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Average repair costs</b></td>
        <td>
          {{ itemStats.averageRepairCost.toLocaleString('en-US') }}
          <b-icon icon="coins" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Weight</b></td>
        <td>
          {{ itemStats.weight.toLocaleString('en-US') }}
          <b-icon icon="weight-hanging" size="is-small" />
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

      <tr>
        <td><b>Leg Armor</b></td>
        <td>
          {{ itemStats.legArmor }}
          <b-icon icon="shield-alt" size="is-small" />
        </td>
      </tr>

      <template v-if="speedStats">
        <tr>
          <td><b>Free weight</b></td>
          <td>
            {{ speedStats.freeWeight.toLocaleString('en-US') }}
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
import {
  computeArmorSetPieceStrengthRequirement,
  computeAverageRepairCost,
  computeMaxRepairCost,
} from '@/services/item-service';
import { computeSpeedStats } from '@/services/characters-service';

@Component
export default class CharacterOverallItemsStatsComponent extends Vue {
  @Prop(Object) readonly character: Character;
  @Prop(Array) readonly equippedItems: EquippedItem[] | null;

  get characteristics() {
    return this.$store.state.user.characteristicsByCharacterId[
      this.character.id
    ] as CharacterCharacteristics;
  }

  get speedStats(): CharacterSpeedStats | null {
    if (!this.characteristics) return null;
    return computeSpeedStats({
      strength: this.characteristics.attributes.strength,
      agility: this.characteristics.attributes.agility,
      athletics: this.characteristics.skills.athletics,
      totalEncumbrance: this.itemStats.weight,
    });
  }

  get itemStats(): Record<string, number> {
    const result = {
      armorSetRequirement: 0,
      price: 0,
      maxRepairCost: 0,
      averageRepairCost: 0,
      weight: 0,
      headArmor: 0,
      bodyArmor: 0,
      armArmor: 0,
      legArmor: 0,
    };

    if (!this.equippedItems) return result;
    result.armorSetRequirement = computeArmorSetPieceStrengthRequirement(this.equippedItems);
    result.maxRepairCost = computeMaxRepairCost(
      this.equippedItems.map(item => item.userItem.baseItem)
    );
    result.averageRepairCost = computeAverageRepairCost(
      this.equippedItems.map(item => item.userItem.baseItem)
    );
    this.equippedItems.forEach(item => {
      const armor = item.userItem.baseItem.armor;
      result.price += item.userItem.baseItem.price;
      result.weight += Number.parseFloat(item.userItem.baseItem.weight.toFixed(2));

      if (armor) {
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
