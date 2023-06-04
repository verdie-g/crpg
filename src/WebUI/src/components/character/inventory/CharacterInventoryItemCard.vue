<script setup lang="ts">
import { type UserItem } from '@/models/user';
import { getItemImage } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    item: UserItem;
    equipped: boolean;
    notMeetRequirement: boolean;
    isNew: boolean;
  }>(),
  {
    equipped: false,
    notMeetRequirement: false,
    isNew: false,
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

// computed
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

      <div class="absolute left-0 top-0 z-10 cursor-default opacity-80 hover:opacity-100">
        <Tag
          v-if="item.isBroken"
          rounded
          variant="danger"
          icon="error"
          v-tooltip="$t('character.inventory.item.broken.tooltip.title')"
        />

        <!-- TODO: i18n -->
        <OIcon
          v-if="item.rank > 0"
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

      <div class="absolute bottom-0 left-0 z-10 cursor-default opacity-80 hover:opacity-100">
        <Tag v-if="isNew" variant="success" label="new" />
      </div>

      <div class="absolute bottom-0 right-0 z-10 cursor-default opacity-80 hover:opacity-100">
        <Tag
          v-if="notMeetRequirement"
          rounded
          variant="danger"
          icon="alert"
          v-tooltip="$t('character.inventory.item.requirement.tooltip.title')"
        />
        <Tag
          v-if="equipped"
          rounded
          variant="primary"
          icon="check"
          v-tooltip="$t('character.inventory.item.equipped')"
        />
      </div>
    </div>
  </article>
</template>
