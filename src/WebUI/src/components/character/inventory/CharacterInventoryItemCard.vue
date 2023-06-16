<script setup lang="ts">
import { type UserItem } from '@/models/user';
import { getItemImage } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    userItem: UserItem;
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
</script>

<template>
  <article
    class="relative w-full cursor-grab items-center justify-center rounded-lg bg-base-200 p-1.5 ring ring-transparent hover:ring-border-200"
  >
    <div class="group relative h-full w-full cursor-grab">
      <img
        :src="getItemImage(userItem.item.id)"
        :alt="userItem.item.name"
        class="h-full w-full select-none object-contain"
        v-tooltip.bottom="userItem.item.name"
        data-aq-item-card-thumb
      />

      <div class="absolute left-0 top-0 z-10">
        <Tag
          v-if="userItem.isBroken"
          rounded
          variant="danger"
          icon="error"
          v-tooltip="$t('character.inventory.item.broken.tooltip.title')"
          class="cursor-default opacity-80 hover:opacity-100"
        />

        <ItemRankIcon
          v-if="userItem.item.rank > 0"
          :rank="userItem.item.rank"
          class="cursor-default opacity-80 hover:opacity-100"
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
