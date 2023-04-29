<script setup lang="ts">
import { itemSellCostCoefs } from '@root/data/constants.json';
import { type CompareItemsResult, type ItemFlat } from '@/models/item';
import { type UserItem } from '@/models/user';
import {
  getAggregationsConfig,
  getVisibleAggregationsConfig,
} from '@/services/item-search-service';
import {
  getItemImage,
  computeSalePrice,
  computeBrokenItemRepairCost,
} from '@/services/item-service';
import { parseTimestamp } from '@/utils/date';
import { omitPredicate } from '@/utils/object';

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
  const { price, graceTimeEnd } = computeSalePrice(props.userItem);

  return {
    price,
    graceTimeEnd:
      graceTimeEnd === null ? null : parseTimestamp(graceTimeEnd.valueOf() - new Date().valueOf()),
  };
});

const repairCost = computed(() => computeBrokenItemRepairCost(props.item.price));

const emit = defineEmits<{
  (e: 'sell'): void;
  (e: 'repair'): void;
  // (e: 'upgrade'): void; // TODO:
}>();

const omitEmptyParam = (field: keyof ItemFlat) => {
  if (Array.isArray(props.item[field]) && (props.item[field] as string[]).length === 0) {
    return false;
  }

  if (props.item[field] === 0) {
    return false;
  }

  return true;
};

const isBroken = computed(() => props.userItem.rank < 0);

const aggregationsConfig = computed(() => omitPredicate(
    getVisibleAggregationsConfig(getAggregationsConfig(props.item.type, props.item.weaponClass)),
    (key: keyof ItemFlat) => omitEmptyParam(key)
  ));
</script>

<template>
  <article class="relative">
    <div class="relative mb-3">
      <img
        :src="getItemImage(props.item.id)"
        :alt="props.item.name"
        :title="props.item.name"
        class="pointer-events-none w-full select-none object-contain"
      />

      <div
        v-if="userItem.rank !== 0"
        class="absolute -left-0.5 -top-0.5 z-10 cursor-default opacity-80 hover:opacity-100"
      >
        <OIcon
          v-if="isBroken"
          icon="error"
          size="2xl"
          class="text-status-danger"
          v-tooltip="'Item is broken'"
        />
      </div>

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

      <div v-for="(_agg, field) in aggregationsConfig" class="space-y-1">
        <VTooltip :delay="{ show: 600 }">
          <h6 class="text-2xs text-content-300">
            {{ $t(`item.aggregations.${field}.title`) }}
          </h6>

          <template #popper>
            <div class="prose prose-invert">
              <h5 class="text-content-100">
                {{ $t(`item.aggregations.${field}.title`) }}
              </h5>
              <p v-if="$t(`item.aggregations.${field}.description`)">
                {{ $t(`item.aggregations.${field}.description`) }}
              </p>
            </div>
          </template>
        </VTooltip>

        <ItemParam
          :item="item"
          :field="field"
          :compareMode="compareResult !== undefined"
          :bestValue="compareResult !== undefined ? compareResult[field] : undefined"
        >
          <template v-if="field === 'price'" #default="{ rawBuckets }">
            <Coin :value="(rawBuckets as number)" />
          </template>

          <template v-if="field === 'upkeep'" #default="{ rawBuckets }">
            <Coin>
              {{ $t('item.format.upkeep', { upkeep: $n(rawBuckets as number) }) }}
            </Coin>
          </template>
        </ItemParam>
      </div>
    </div>

    <div class="-mx-4 -mb-4 mt-6 flex items-center gap-2 bg-base-400 p-2">
      <ConfirmActionTooltip
        class="flex-1"
        :confirmLabel="$t('action.sell')"
        :title="$t('character.inventory.item.sell.confirm')"
        @confirm="emit('sell')"
      >
        <OButton variant="secondary" expanded rounded size="lg">
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
      </ConfirmActionTooltip>

      <ConfirmActionTooltip v-if="isBroken" @confirm="emit('repair')">
        <VTooltip>
          <OButton iconRight="repair" variant="secondary" size="lg" rounded />

          <template #popper>
            <i18n-t
              scope="global"
              keypath="character.inventory.item.repair.tooltip.title"
              tag="span"
              class="flex gap-2"
            >
              <template #price>
                <Coin :value="repairCost" />
              </template>
            </i18n-t>
          </template>
        </VTooltip>
      </ConfirmActionTooltip>
    </div>
  </article>
</template>
