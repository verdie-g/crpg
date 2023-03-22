<script setup lang="ts">
import { WeaponUsage, type ItemFlat } from '@/models/item';
import { getItems, getCompareItemsResult } from '@/services/item-service';
import { getSearchResult } from '@/services/item-search-service';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

import { useUserStore } from '@/stores/user';

import { useItemsFilter } from '@/composables/shop/use-filters';
import { useItemsSort } from '@/composables/shop/use-sort';
import { usePagination } from '@/composables/use-pagination';
import { useItemsCompare } from '@/composables/shop/use-compare';
import { useSearchDebounced } from '@/composables/use-search-debounce';

definePage({
  meta: {
    layout: 'default',
    showInNav: true,
    sortInNav: 90,
    noStickyHeader: true,
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const userStore = useUserStore();

const { state: items, execute: loadItems } = useAsyncState(() => getItems(), [], {
  immediate: false,
});

await Promise.all([loadItems(), userStore.fetchUserItems()]);

const {
  itemTypeModel,
  weaponClassModel,
  filterModel,
  updateFilter,

  filteredByClassFlatItems,

  aggregationsConfig,
  aggregationsConfigVisible,

  aggregationByType,
  aggregationByClass,
  scopeAggregations,
} = useItemsFilter(items.value);
const { searchModel } = useSearchDebounced();

const { pageModel, perPageModel, perPageConfig } = usePagination();
const { sortingModel, sortingConfig, getSortingConfigByField } = useItemsSort(aggregationsConfig);

const {
  compareMode,
  toggleCompareMode,
  compareList,
  toggleToCompareList,
  addAllToCompareList,
  removeAllFromCompareList,
} = useItemsCompare();

const searchResult = computed(() =>
  getSearchResult({
    items: filteredByClassFlatItems.value,
    aggregationConfig: aggregationsConfig.value,
    sortingConfig: sortingConfig.value,
    sort: sortingModel.value,
    page: pageModel.value,
    perPage: perPageModel.value,
    query: searchModel.value,
    filter: {
      ...filterModel.value,
      ...(compareMode.value && { modId: compareList.value }),
    },
  })
);

const compareItemsResult = computed(() =>
  !compareMode.value
    ? null
    : getCompareItemsResult(searchResult.value.data.items, aggregationsConfig.value)
);

const buyItem = async (item: ItemFlat) => {
  await userStore.buyItem(item.id);

  notify(t('shop.item.buy.notify.success'));
};
</script>

<template>
  <div class="relative space-y-2 px-6 pt-6 pb-6">
    <!-- <div class="fixed top-4 right-10 z-20 rounded-lg bg-white p-4 shadow-lg">
     <div>baseFilterModel: type: {{ itemTypeModel }} weaponClass: {{ weaponClassModel }}</div>
      <div>filterModel: {{ filterModel }}</div>
      <div>nameModel: {{ nameModel }}</div>
      <div>sortingModel: {{ sortingModel }}</div>
      <div>pageModel: {{ pageModel }}</div>
      <div>searchResult.pagination.total {{ searchResult.pagination.total }}</div>
    </div> -->

    <ShopItemTypeSelect
      v-model:itemType="itemTypeModel"
      v-model:weaponClass="weaponClassModel"
      :itemTypeBuckets="aggregationByType.data.buckets"
      :weaponClassBuckets="aggregationByClass.data.buckets"
    />

    <OTable
      v-model:current-page="pageModel"
      v-model:checked-rows="compareList"
      :data="searchResult.data.items"
      bordered
      narrowed
      hoverable
      sortIcon="chevron-up"
      sortIconSize="xs"
      sticky-header
    >
      <OTableColumn field="compare" :width="50">
        <template #header>
          <OCheckbox
            v-tooltip="
              compareList.length ? $t('shop.compare.removeAll') : $t('shop.compare.addAll')
            "
            :modelValue="compareList.length >= 1"
            :nativeValue="true"
            @update:modelValue="
              () =>
                compareList.length
                  ? removeAllFromCompareList()
                  : addAllToCompareList(searchResult.data.items.map(item => item.modId))
            "
          />
        </template>
        <template #default="{ row: item }: { row: ItemFlat }">
          <OCheckbox
            v-tooltip="
              compareList.includes(item.modId) ? $t('shop.compare.remove') : $t('shop.compare.add')
            "
            :modelValue="compareList.includes(item.modId)"
            :nativeValue="true"
            @update:modelValue="() => toggleToCompareList(item.modId)"
          />
        </template>
      </OTableColumn>

      <OTableColumn field="name" label="Name">
        <template #header>
          <div class="max-w-[220px] pr-6">
            <!-- TODO: unit -->
            <OInput
              v-model="searchModel"
              type="text"
              :placeholder="$t('action.search')"
              icon="search"
              rounded
              expanded
              clearable
              size="sm"
              iconRighclearabletClickable
              data-aq-search-shop-input
            />
          </div>
        </template>
        <template #default="{ row: item }: { row: ItemFlat }">
          <ShopGridItemName class="pr-6" :item="item" />
        </template>
      </OTableColumn>

      <OTableColumn
        v-for="field in (Object.keys(aggregationsConfigVisible) as Array<keyof ItemFlat>)"
        :field="field"
        :width="aggregationsConfigVisible[field]?.width ?? 100"
      >
        <!--  -->
        <template #header>
          <div class="relative mr-2 flex items-center gap-1">
            <ShopGridFilter
              v-if="field in searchResult.data.aggregations"
              :scopeAggregation="scopeAggregations[field]"
              :aggregation="searchResult.data.aggregations[field]"
              :aggregationConfig="aggregationsConfig[field]!"
              :modelValue="filterModel[field]!"
              @update:modelValue="val => updateFilter(field, val)"
            />
            <ShopGridSort
              v-if="Object.keys(getSortingConfigByField(field)).length !== 0"
              class="w-5"
              v-model:modelValue="sortingModel"
              :sortingConfig="getSortingConfigByField(field)"
            />
          </div>
        </template>

        <template #default="{ row: item }: { row: ItemFlat }">
          <ItemParam
            :item="item"
            :field="field"
            :bestValue="compareItemsResult !== null ? compareItemsResult[field] : undefined"
            :compareMode="compareMode"
          >
            <template v-if="field === 'price'" #default="{ rawBuckets }">
              <ShopGridItemBuyBtn
                :price="(rawBuckets as number)"
                :inInventory="userStore.userItems.some(ui => ui.baseItem.id === item.id)"
                :notEnoughGold="userStore.user!.gold < item.price"
                @buy="buyItem(item)"
              />
            </template>
          </ItemParam>
        </template>
      </OTableColumn>

      <template #empty>
        <ResultNotFound />
      </template>

      <template #footer>
        <div class="space-y-4 bg-base-100 py-4 pr-2 backdrop-blur-sm">
          <div class="grid grid-cols-3 items-center gap-6">
            <div class="flex items-center gap-4">
              <OPagination
                v-model:current="pageModel"
                :total="searchResult.pagination.total"
                :rangeBefore="1"
                :rangeAfter="1"
                :perPage="searchResult.pagination.per_page"
                order="left"
                aria-next-label="Next page"
                aria-previous-label="Previous page"
                aria-page-label="Page"
                aria-current-label="Current page"
              >
                <!-- hidden prev/next -->
                <template #next>
                  <span></span>
                </template>
                <template #previous>
                  <span></span>
                </template>
              </OPagination>
            </div>

            <div class="flex justify-center">
              <OButton
                v-if="compareList.length >= 2"
                variant="primary"
                size="lg"
                outlined
                :iconRight="compareMode ? 'close' : null"
                data-aq-shop-handler="toggle-compare-mode"
                :label="$t('shop.compare.title')"
                @click="toggleCompareMode"
              />
            </div>

            <div class="flex items-center justify-end gap-4">
              <div class="text-content-400">{{ $t('shop.pagination.perPage') }}</div>
              <OTabs v-model="perPageModel" size="xl" type="bordered-rounded" contentClass="hidden">
                <OTabItem v-for="pp in perPageConfig" :label="String(pp)" :value="pp" />
              </OTabs>
            </div>
          </div>
        </div>
      </template>
    </OTable>

    <!-- TODO: placement -->
    <div v-if="'weaponUsage' in filterModel" class="mt-4 flex">
      <VTooltip>
        <OCheckbox
          class="opacity-75"
          :nativeValue="WeaponUsage.Secondary"
          :modelValue="filterModel['weaponUsage']"
          @update:modelValue="(val: string) => updateFilter('weaponUsage', val)"
        >
          {{ $t('shop.nonPrimaryWeaponMode.title') }}
        </OCheckbox>

        <template #popper>
          <div class="prose prose-invert">
            <h5 class="text-content-100">
              {{ $t('shop.nonPrimaryWeaponMode.tooltip.title') }}
            </h5>
            <div v-html="$t('shop.nonPrimaryWeaponMode.tooltip.desc')" />
          </div>
        </template>
      </VTooltip>
    </div>
  </div>
</template>
