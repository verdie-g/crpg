<script setup lang="ts">
import { type UserItem } from '@/models/user';
import { getItemImage } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    item: UserItem;
    equipped: boolean;
  }>(),
  {
    equipped: false,
  }
);

// TODO: colors
const rarityColor = computed(() => {
  if (props.item.rank === 1) {
    return '#1eff00';
  }

  if (props.item.rank === 2) {
    return '#0070dd';
  }

  if (props.item.rank === 3) {
    return '#a335ee';
  }

  return;
});
</script>

<template>
  <article
    class="relative w-full cursor-grab items-center justify-center rounded-lg bg-base-200 p-1.5 ring ring-transparent hover:ring-border-200"
  >
    <div class="group relative h-full w-full cursor-grab">
      <img
        :src="getItemImage(item.baseItem.id)"
        :alt="item.baseItem.name"
        class="h-full w-full select-none object-contain"
        v-tooltip.bottom="item.baseItem.name"
        data-aq-item-card-thumb
      />

      <div
        v-if="item.rank !== 0"
        class="absolute -top-0.5 -left-0.5 z-10 cursor-default opacity-80 hover:opacity-100"
      >
        <OIcon
          v-if="item.rank < 0"
          icon="error"
          size="lg"
          class="text-status-danger"
          v-tooltip="'Item is broken'"
        />

        <!-- TODO: i18n -->
        <OIcon
          v-else
          icon="rare-duotone"
          size="xs"
          v-tooltip="'Item rarity'"
          :style="{
            '--fa-primary-opacity': 0.15,
            '--fa-primary-color': '#fff',
            '--fa-secondary-opacity': 0.75,
            '--fa-secondary-color': rarityColor,
          }"
        />
      </div>

      <Tag
        v-if="equipped"
        class="absolute bottom-0 right-0 z-10 cursor-default opacity-75 hover:opacity-100"
        rounded
        variant="primary"
        icon="check"
        v-tooltip="$t('character.inventory.item.equipped')"
      />
    </div>
  </article>
</template>
