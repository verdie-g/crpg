<template>
  <div>
    <b-field horizontal label="Price" class="characteristic-field is-marginless">
      <b-input size="is-small" :editable="false" :value="itemStats.price.toLocaleString()" />
    </b-field>
    <b-field horizontal label="Max repair costs" class="characteristic-field is-marginless">
      <b-input
        size="is-small"
        :editable="false"
        :value="itemStats.maxRepairCost.toLocaleString()"
      />
    </b-field>
    <b-field horizontal label="Weight" class="characteristic-field is-marginless">
      <b-input size="is-small" :editable="false" :value="itemStats.weight" />
    </b-field>
    <b-field horizontal label="Head Armor" class="characteristic-field is-marginless">
      <b-input size="is-small" :editable="false" :value="itemStats.headArmor" />
    </b-field>
    <b-field horizontal label="Body Armor" class="characteristic-field is-marginless">
      <b-input size="is-small" :editable="false" :value="itemStats.bodyArmor" />
    </b-field>
    <b-field horizontal label="Arm Armor" class="characteristic-field is-marginless">
      <b-input size="is-small" :editable="false" :value="itemStats.armArmor" />
    </b-field>
    <b-field horizontal label="Leg Armor" class="characteristic-field is-marginless">
      <b-input size="is-small" :editable="false" :value="itemStats.legArmor" />
    </b-field>
  </div>
</template>

<script lang="ts">
import EquippedItem from '@/models/equipped-item';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { computeMaxRepairCost } from '@/services/characters-service';

@Component
export default class CharacterOverallItemsStatsComponent extends Vue {
  @Prop(Array) readonly equippedItems: EquippedItem[] | null;

  get itemStats(): Record<string, number> {
    const result = {
      price: 0,
      maxRepairCost: 0,
      weight: 0,
      headArmor: 0,
      bodyArmor: 0,
      armArmor: 0,
      legArmor: 0,
    };

    if (!this.equippedItems) return result;

    result.maxRepairCost = Math.floor(computeMaxRepairCost(this.equippedItems));
    this.equippedItems.forEach(item => {
      const armor = item.userItem.baseItem.armor;
      result.price += item.userItem.baseItem.price;
      result.weight += item.userItem.baseItem.weight;

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

<style scoped lang="scss"></style>
