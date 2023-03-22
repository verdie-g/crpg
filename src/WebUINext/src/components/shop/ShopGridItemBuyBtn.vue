<script setup lang="ts">
import { t } from '@/services/translate-service';

const props = defineProps<{
  price: number;
  inInventory: boolean;
  notEnoughGold: boolean;
}>();

const emit = defineEmits<{
  (e: 'buy'): void;
}>();

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
  <div v-tooltip="tooltipTitle">
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
</template>
