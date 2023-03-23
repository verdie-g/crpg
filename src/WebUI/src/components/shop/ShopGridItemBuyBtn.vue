<script setup lang="ts">
import { t } from '@/services/translate-service';
import { computeAverageRepairCostByHour } from '@/services/item-service';

const props = defineProps<{
  price: number;
  inInventory: boolean;
  notEnoughGold: boolean;
}>();

const emit = defineEmits<{
  (e: 'buy'): void;
}>();

const avgRepairCostPerHour = computed(() => computeAverageRepairCostByHour(props.price));

const tooltipTitle = computed(() => {
  if (props.inInventory) {
    return t('shop.item.buy.tooltip.inInventory');
  }

  if (props.notEnoughGold) {
    return t('shop.item.buy.tooltip.notEnoughGold');
  }

  return t('shop.item.buy.tooltip.buy');
});
</script>

<template>
  <VTooltip>
    <div>
      <OButton
        variant="primary"
        outlined
        size="sm"
        :disabled="inInventory || notEnoughGold"
        @click="
          () => {
            emit('buy');
          }
        "
      >
        <Coin :value="price" :class="{ 'opacity-50': inInventory || notEnoughGold }" />
      </OButton>
    </div>

    <template #popper>
      <div class="space-y-4">
        <div>{{ tooltipTitle }}</div>
        <div class="item-center flex gap-2">
          {{ $t('item.aggregations.repairCost.title') }}:
          <div class="inline-flex gap-1.5 align-middle font-bold text-primary">
            <SvgSpriteImg name="coin" viewBox="0 0 18 18" class="w-4" />
            {{ $n(avgRepairCostPerHour) }} / {{ $t('dateTime.hours.short') }}
          </div>
        </div>
      </div>
    </template>
  </VTooltip>
</template>
