<template>
  <div>
    <table width="300px">
      <td><b>Price</b></td>
      <td>
        {{ itemStats.price.toLocaleString('en-US') }}
        <b-icon icon="coins" size="is-small" />
      </td>
      <tr>
        <td><b>Max repair cost</b></td>
        <td>
          {{ itemStats.maxRepairCost.toLocaleString('en-US', { maximumFractionDigits: 2 }) }} / min
          <b-icon icon="coins" size="is-small" />
        </td>
      </tr>
      <tr>
        <td><b>Average repair cost</b></td>
        <td>
          {{ itemStats.averageRepairCost.toLocaleString('en-US', { maximumFractionDigits: 2 }) }} /
          min
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
      <tr>
        <td><b>Mount Armor</b></td>
        <td>
          {{ itemStats.mountArmor }}
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
import type CharacterOverallItemsStats from '@/models/character-overall-items-stats';
import {
  computeAverageRepairCostByMinute,
  computeMaxRepairCostByMinute,
  computeOverallPrice,
  computeOverallWeight,
  computeOverallArmor,
} from '@/services/item-service';
import { computeHealthPoints, computeSpeedStats } from '@/services/characters-service';

@Component
export default class CharacterOverallItemsStatsComponent extends Vue {
  @Prop(Object) readonly character: Character;
  @Prop({ type: Array, default: () => [] }) readonly equippedItems: EquippedItem[];

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
    });
  }

  get itemStats(): CharacterOverallItemsStats {
    const items = this.equippedItems.map(equipedItem => equipedItem.userItem.baseItem);

    return {
      maxRepairCost: computeMaxRepairCostByMinute(items),
      averageRepairCost: computeAverageRepairCostByMinute(items),
      weight: computeOverallWeight(items),
      price: computeOverallPrice(items),
      ...computeOverallArmor(items),
    };
  }
}
</script>

<style scoped lang="scss">
td {
  padding: 3px;
}
</style>
