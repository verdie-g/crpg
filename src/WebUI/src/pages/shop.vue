<script setup lang="ts">
import { WeaponUsage, type ItemFlat } from '@/models/item';
import {
  getItems,
  getCompareItemsResult,
  canUpgrade,
  itemIsNewDays,
} from '@/services/item-service';
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
    noStickyHeader: true,
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const userStore = useUserStore();
const userItemsIds = computed(() => userStore.userItems.map(ui => ui.item.id));

const { state: items, execute: loadItems } = useAsyncState(() => getItems(), [], {
  immediate: false,
});

await Promise.all([loadItems(), userStore.fetchUserItems()]);

const {
  itemTypeModel,
  weaponClassModel,
  filterModel,
  updateFilter,
  hideOwnedItemsModel,
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
  isCompare,
  toggleCompare,
  compareList,
  toggleToCompareList,
  addAllToCompareList,
  removeAllFromCompareList,
} = useItemsCompare();

const searchResult = computed(() =>
  getSearchResult({
    items: filteredByClassFlatItems.value,
    userItemsIds: hideOwnedItemsModel.value ? userItemsIds.value : [],
    aggregationConfig: aggregationsConfig.value,
    sortingConfig: sortingConfig.value,
    sort: sortingModel.value,
    page: pageModel.value,
    perPage: perPageModel.value,
    query: searchModel.value,
    filter: {
      ...filterModel.value,
      ...(isCompare.value && { modId: compareList.value }),
    },
  })
);

const compareItemsResult = computed(() =>
  !isCompare.value
    ? null
    : getCompareItemsResult(searchResult.value.data.items, aggregationsConfig.value)
);

const buyItem = async (item: ItemFlat) => {
  await userStore.buyItem(item.id);

  notify(t('shop.item.buy.notify.success'));
};

const isUpgradableCategory = computed(() => canUpgrade(itemTypeModel.value));
</script>

<template>
  <div class="relative space-y-2 px-6 pb-6 pt-6">
    <!-- <div class="fixed top-4 right-10 z-20 rounded-lg bg-white p-4 shadow-lg">
     <div>baseFilterModel: type: {{ itemTypeModel }} weaponClass: {{ weaponClassModel }}</div>
      <div>filterModel: {{ filterModel }}</div>
      <div>nameModel: {{ nameModel }}</div>
      <div>sortingModel: {{ sortingModel }}</div>
      <div>pageModel: {{ pageModel }}</div>
      <div>searchResult.pagination.total {{ searchResult.pagination.total }}</div>
    </div> -->

    <div class="mb-2 flex items-center gap-6 overflow-x-auto pb-2">
      <VDropdown :triggers="['click']" placement="bottom-end">
        <OButton variant="primary" outlined size="xl" rounded>
          <FontAwesomeLayers full-width class="fa-2x">
            <FontAwesomeIcon :icon="['crpg', 'dots']" />
            <FontAwesomeLayersText
              v-if="
                hideOwnedItemsModel ||
                ('weaponUsage' in filterModel && filterModel['weaponUsage']!.length > 1) ||
                ('new' in filterModel && filterModel['new']!.length)
              "
              counter
              value="●"
              position="top-right"
              :style="{ '--fa-counter-background-color': 'rgba(83, 188, 150, 1)' }"
            />
          </FontAwesomeLayers>
        </OButton>

        <template #popper="{ hide }">
          <DropdownItem>
            <Tooltip
              :title="$t('item.aggregations.new.title')"
              :description="$t('item.aggregations.new.description', { days: itemIsNewDays })"
            >
              <OCheckbox
                :nativeValue="1"
                :modelValue="filterModel['new']"
                @update:modelValue="(val: number) => updateFilter('new', val)"
                @change="hide"
              >
                {{ $t('item.aggregations.new.title') }}
              </OCheckbox>
            </Tooltip>
          </DropdownItem>

          <DropdownItem>
            <OCheckbox v-model="hideOwnedItemsModel" @change="hide">
              {{ $t('shop.hideOwnedItems.title') }}
            </OCheckbox>
          </DropdownItem>

          <DropdownItem v-if="'weaponUsage' in filterModel">
            <Tooltip
              :title="$t('shop.nonPrimaryWeaponMode.tooltip.title')"
              :description="$t('shop.nonPrimaryWeaponMode.tooltip.desc')"
            >
              <OCheckbox
                :nativeValue="WeaponUsage.Secondary"
                :modelValue="filterModel['weaponUsage']"
                @update:modelValue="(val: string) => updateFilter('weaponUsage', val)"
                @change="hide"
              >
                {{ $t('shop.nonPrimaryWeaponMode.title') }}
              </OCheckbox>
            </Tooltip>
          </DropdownItem>
        </template>
      </VDropdown>

      <div class="h-8 w-px select-none bg-border-200" />

      <div class="">
        <ShopItemTypeSelect
          v-model:itemType="itemTypeModel"
          v-model:weaponClass="weaponClassModel"
          :itemTypeBuckets="aggregationByType.data.buckets"
          :weaponClassBuckets="aggregationByClass.data.buckets"
        />
      </div>
    </div>

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
      :detailed="isUpgradableCategory"
      detailKey="id"
    >
      <OTableColumn field="compare" :width="36">
        <template #header>
          <span class="inline-flex items-center">
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
          </span>
        </template>
        <template #default="{ row: item }: { row: ItemFlat }">
          <span class="inline-flex items-center">
            <OCheckbox
              v-tooltip="
                compareList.includes(item.modId)
                  ? $t('shop.compare.remove')
                  : $t('shop.compare.add')
              "
              :modelValue="compareList.includes(item.modId)"
              :nativeValue="true"
              @update:modelValue="() => toggleToCompareList(item.modId)"
            />
          </span>
        </template>
      </OTableColumn>

      <OTableColumn field="name">
        <template #header>
          <div class="max-w-[220px]">
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
          <ShopGridItemName :item="item" showTier />
        </template>
      </OTableColumn>

      <OTableColumn
        v-for="field in (Object.keys(aggregationsConfigVisible) as Array<keyof ItemFlat>)"
        :field="field"
        :width="aggregationsConfigVisible[field]?.width ?? 140"
        :thAttrs="
          () => ({
            style: `max-width: ${aggregationsConfigVisible[field]?.width ?? 140}px`,
          })
        "
      >
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
            :isCompare="isCompare"
          >
            <template v-if="field === 'price'" #default="{ rawBuckets }">
              <ShopGridItemBuyBtn
                :price="(rawBuckets as number)"
                :upkeep="item.upkeep"
                :inInventory="userItemsIds.includes(item.id)"
                :notEnoughGold="userStore.user!.gold < item.price"
                @buy="buyItem(item)"
              />
            </template>
          </ItemParam>
        </template>
      </OTableColumn>

      <template #detail="{ row: item }: { row: ItemFlat }">
        <ShopGridUpgradesTable :item="item" :cols="aggregationsConfigVisible" />
      </template>

      <template #empty>
        <ResultNotFound />
      </template>

      <template #footer>
        <div class="space-y-4 bg-base-100 py-4 pr-2 backdrop-blur-sm">
          <div class="grid h-14 grid-cols-3 items-center gap-6">
            <div class="flex items-center gap-4">
              <OPagination
                v-model:current="pageModel"
                :total="searchResult.pagination.total"
                :rangeBefore="2"
                :rangeAfter="2"
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

              <OInput v-model="pageModel" size="sm" rounded class="w-20" clearable />
            </div>

            <div class="flex justify-center">
              <OButton
                v-if="compareList.length >= 2"
                variant="primary"
                size="lg"
                outlined
                :iconRight="isCompare ? 'close' : null"
                data-aq-shop-handler="toggle-compare"
                :label="$t('shop.compare.title')"
                @click="toggleCompare"
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
  </div>
</template>
