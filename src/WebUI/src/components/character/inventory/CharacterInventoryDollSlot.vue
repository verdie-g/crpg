<script setup lang="ts">
import { FontAwesomeLayersText } from '@fortawesome/vue-fontawesome';
import { type CharacterArmorOverall } from '@/models/character';
import { ItemSlot } from '@/models/item';
import { type UserItem } from '@/models/user';
import { getItemImage } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    slot: ItemSlot;
    placeholder: string;
    userItem?: UserItem;
    armorOverall?: CharacterArmorOverall;
    notMeetRequirement: boolean;
    // slot state
    available?: boolean;
    focused?: boolean;
    invalid?: boolean;
    remove?: boolean;
  }>(),
  {
    notMeetRequirement: false,
    available: false,
    focused: false,
    invalid: false,
    remove: false,
  }
);

const emit = defineEmits<{
  (e: 'unEquip'): void;
}>();
</script>

<template>
  <div
    class="group relative flex h-28 items-center justify-center rounded-lg bg-base-200 p-1.5 ring"
    :class="[
      [available ? 'ring-border-300' : 'ring-transparent hover:ring-border-200'],
      {
        '!ring-status-success': props.focused,
        '!ring-status-warning': props.invalid,
        '!ring-status-danger': props.remove,
      },
    ]"
  >
    <template v-if="userItem !== undefined">
      <img
        class="h-full w-full cursor-grab select-none object-contain"
        :src="getItemImage(userItem.item.id)"
        :alt="userItem.item.name"
        data-aq-character-slot-item-thumb
      />

      <div class="absolute left-1.5 top-1.5 z-10 cursor-default opacity-80 hover:opacity-100">
        <ItemRankIcon
          v-if="userItem !== undefined && userItem.item.rank > 1"
          :rank="userItem.item.rank"
        />
      </div>

      <div class="absolute bottom-1.5 right-1.5 z-10 cursor-default opacity-80 hover:opacity-100">
        <Tag
          v-if="notMeetRequirement"
          rounded
          variant="danger"
          icon="alert"
          v-tooltip="$t('character.inventory.item.requirement.tooltip.title')"
        />
      </div>
    </template>

    <OIcon
      v-else
      class="select-none"
      :icon="props.placeholder"
      size="5x"
      v-tooltip.bottom="$t(`character.doll.slot.${props.slot}`)"
      data-aq-character-slot-item-placeholder
    />

    <div
      v-if="armorOverall !== undefined"
      class="group absolute right-0 top-0 flex -translate-y-3/4 translate-x-1/2 transform cursor-default"
      v-tooltip.bottom="$t(`character.doll.armorOverall.${armorOverall.key}`)"
    >
      <FontAwesomeLayers class="fa-4x">
        <FontAwesomeIcon
          :icon="['crpg', 'fa-shield-duotone']"
          size="4x"
          class="text-base-400 group-hover:text-base-500"
          :style="{
            '--fa-secondary-opacity': 0.85,
          }"
        />
        <FontAwesomeLayersText
          :value="armorOverall.value"
          class="text-xs font-bold text-content-200 group-hover:text-content-100"
        />
      </FontAwesomeLayers>
    </div>
  </div>
</template>
