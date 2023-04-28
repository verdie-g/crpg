<script setup lang="ts">
import { t } from '@/services/translate-service';
import { useUserStore } from '@/stores/user';

const props = defineProps<{
  price: number;
  upkeep: number;
  inInventory: boolean;
  notEnoughGold: boolean;
}>();

const userStore = useUserStore();

const emit = defineEmits<{
  (e: 'buy'): void;
}>();

const isExpensive = computed(() => userStore.user!.gold - props.price < props.upkeep);

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

        <Tag v-if="isExpensive" icon="alert" size="sm" variant="warning" rounded />
      </OButton>
    </div>

    <template #popper>
      <div class="space-y-4">
        <div>{{ tooltipTitle }}</div>

        <div class="item-center flex gap-2">
          {{ $t('item.aggregations.upkeep.title') }}:
          <Coin>
            {{ $t('item.format.upkeep', { upkeep: $n(upkeep as number) }) }}
          </Coin>
        </div>

        <div v-if="isExpensive" class="flex items-start gap-2">
          <Tag icon="alert" size="sm" variant="warning" rounded />
          {{ $t('shop.item.expensive') }}
        </div>
      </div>
    </template>
  </VTooltip>
</template>
