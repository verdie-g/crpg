<template>
  <div v-if="userItem">
    <b>{{ userItem.baseItem.name }}</b>
    <table width="200px">
      <tr>
        <td>Price</td>
        <td>{{ userItem.baseItem.price.toLocaleString('en-US') }}</td>
      </tr>
      <tr>
        <td>Max repair costs</td>
        <td>
          {{ maxRepairCosts }}
        </td>
      </tr>
      <tr>
        <td>Average repair costs</td>
        <td>
          {{ averageRepairCosts }}
        </td>
      </tr>
    </table>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import UserItem from '@/models/user-item';
import { computeAverageRepairCost, computeMaxRepairCost } from '@/services/characters-service';

@Component
export default class ItemTooltipComponent extends Vue {
  @Prop(Object) readonly userItem: UserItem;

  get maxRepairCosts() {
    return parseFloat(computeMaxRepairCost([this.userItem]).toFixed(2)).toLocaleString('en-US');
  }

  get averageRepairCosts() {
    return parseFloat(computeAverageRepairCost([this.userItem]).toFixed(2)).toLocaleString('en-US');
  }
}
</script>

<style scoped lang="scss"></style>
