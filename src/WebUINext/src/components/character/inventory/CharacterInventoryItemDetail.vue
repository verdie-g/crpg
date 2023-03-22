<script setup lang="ts">
import { itemSellCostCoefs } from '@root/data/constants.json';

import { type CompareItemsResult, type ItemFlat } from '@/models/item';
import { type UserItem } from '@/models/user';
import {
  getAggregationsConfig,
  getVisibleAggregationsConfig,
} from '@/services/item-search-service';
import { getItemImage, computeSalePrice } from '@/services/item-service';
import { parseTimestamp } from '@/utils/date';

const props = withDefaults(
  defineProps<{
    item: ItemFlat;
    userItem: UserItem;
    compareResult: CompareItemsResult | undefined;
    equipped?: boolean;
  }>(),
  {
    equipped: false,
  }
);

const userItemToReplaceSalePrice = computed(() => {
  const salePrice = computeSalePrice(props.userItem);

  return {
    price: salePrice.price,
    graceTimeEnd:
      salePrice.graceTimeEnd === null
        ? null
        : parseTimestamp(salePrice.graceTimeEnd.valueOf() - new Date().valueOf()),
  };
});

const emit = defineEmits<{
  (e: 'sell'): void;
  // (e: 'upgrade'): void; // TODO:
}>();

const omitEmptyParam = (field: keyof ItemFlat) => {
  if (Array.isArray(props.item[field]) && (props.item[field] as string[]).length === 0) {
    return false;
  }

  return true;
};

const aggregationsConfig = computed(() =>
  getVisibleAggregationsConfig(getAggregationsConfig(props.item.type, props.item.weaponClass))
);
</script>

<template>
  <article class="relative">
    <div class="relative mb-3">
      <img
        :src="getItemImage(props.item.id)"
        :alt="props.item.name"
        :title="props.item.name"
        class="pointer-events-none aspect-video w-full select-none object-contain"
      />

      <Tag
        v-if="equipped"
        class="absolute bottom-0 right-0 z-10 cursor-default text-primary opacity-75 hover:opacity-100"
        icon="check"
        variant="primary"
        rounded
        v-tooltip="$t('character.inventory.item.equipped')"
      />
    </div>

    <h3 class="mb-6 font-semibold text-content-100">
      {{ props.item.name }}
    </h3>

    <div class="grid grid-cols-2 gap-4">
      <div class="space-y-1">
        <h6 class="text-2xs text-content-300">Type/Class</h6>
        <div class="flex flex-wrap gap-2">
          <ItemParam :item="item" field="type" />
          <ItemParam v-if="item.weaponClass !== null" :item="item" field="weaponClass" />
        </div>
      </div>

      <!-- TODO: filter aggregationsConfig instead v-show !!! -->
      <div
        v-for="(agg, field) in aggregationsConfig"
        v-show="omitEmptyParam(field)"
        class="space-y-1"
      >
        <h6 class="text-2xs text-content-300">{{ $t(`item.aggregations.${field}.title`) }}</h6>

        <ItemParam
          :item="item"
          :field="field"
          :compareMode="compareResult !== undefined"
          :bestValue="compareResult !== undefined ? compareResult[field] : undefined"
        >
          <template v-if="field === 'price'" #default="{ rawBuckets }">
            <Coin :value="(rawBuckets as number)" />
          </template>
        </ItemParam>
      </div>
    </div>

    <div class="-mx-4 -mb-4 mt-6 bg-base-400 p-2">
      <VTooltip :triggers="['click']">
        <OButton variant="secondary" rounded size="lg">
          <i18n-t
            scope="global"
            keypath="character.inventory.item.sell.title"
            tag="span"
            class="flex gap-2"
          >
            <template #price>
              <Coin :value="userItemToReplaceSalePrice.price" />
            </template>
          </i18n-t>

          <VTooltip>
            <Tag
              v-if="userItemToReplaceSalePrice.graceTimeEnd !== null"
              size="sm"
              variant="success"
              label="100%"
            />
            <Tag
              v-else
              size="sm"
              variant="danger"
              :label="$n(itemSellCostCoefs[0] - 1, 'percent', { minimumFractionDigits: 0 })"
            />

            <template #popper>
              <i18n-t
                v-if="userItemToReplaceSalePrice.graceTimeEnd !== null"
                scope="global"
                keypath="character.inventory.item.sell.freeRefund"
                tag="div"
              >
                <template #dateTime>
                  <span class="font-bold">
                    {{ $t('dateTimeFormat.mm', { ...userItemToReplaceSalePrice.graceTimeEnd }) }}
                  </span>
                </template>
              </i18n-t>

              <i18n-t
                v-else
                scope="global"
                keypath="character.inventory.item.sell.penaltyRefund"
                tag="div"
              >
                <template #penalty>
                  <span class="font-bold text-status-danger">
                    {{ $n(itemSellCostCoefs[0], 'percent', { minimumFractionDigits: 0 }) }}
                  </span>
                </template>
              </i18n-t>
            </template>
          </VTooltip>
        </OButton>

        <template #popper="{ hide }">
          <div class="space-y-3">
            <div>
              {{ $t('character.inventory.item.sell.confirm') }}
            </div>

            <div class="flex items-center gap-2">
              <OButton
                variant="success"
                size="2xs"
                iconLeft="check"
                :label="$t('action.sell')"
                @click="
                  () => {
                    emit('sell');
                    hide();
                  }
                "
              />
              <OButton
                variant="danger"
                size="2xs"
                iconLeft="close"
                :label="$t('action.cancel')"
                @click="hide"
              />
            </div>
          </div>
        </template>
      </VTooltip>
    </div>
  </article>
</template>
