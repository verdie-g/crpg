<script setup lang="ts">
import { type ItemFlat } from '@/models/item';
import { type AggregationConfig } from '@/models/item-search';
import { useItemUpgrades } from '@/composables/item/use-item-upgrades';
import { useUserStore } from '@/stores/user';
import { getRankColor } from '@/services/item-service';
import { clamp } from '@/utils/math';

const userStore = useUserStore();

const { item, cols } = defineProps<{
  item: ItemFlat;
  cols: AggregationConfig;
}>();

const emit = defineEmits<{
  upgrade: [];
}>();

const { compareItemsResult, itemUpgrades } = useItemUpgrades(item, cols);

const currentItem = computed(() => itemUpgrades.value.find(iu => iu.id === item.id));
const nextItem = computed(() => {
  const currItemIdx = itemUpgrades.value.findIndex(iu => iu.id === item.id);
  return itemUpgrades.value[clamp(currItemIdx + 1, 0, 3)];
});

const canUpgrade = computed(() => item.rank !== 3 && userStore.user!.heirloomPoints! > 0);
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center gap-4 px-4">
      <h3 class="text-xl font-semibold">
        {{ $t('character.inventory.item.upgrade.upgradesTitle') }}
      </h3>

      <div
        class="flex items-center gap-2 font-bold text-primary"
        v-tooltip.bottom="$t('user.field.heirloom')"
      >
        <OIcon icon="blacksmith" size="lg" />
        {{ userStore.user!.heirloomPoints }}
      </div>

      <Modal v-if="canUpgrade">
        <OButton variant="primary" size="sm" :label="$t('action.upgrade')" />
        <template #popper="{ hide }">
          <ConfirmActionForm
            :title="$t('action.confirmation')"
            :name="$t('character.inventory.item.upgrade.confirm.value')"
            :confirmLabel="$t('action.confirm')"
            @cancel="hide"
            @confirm="
              () => {
                hide();
                emit('upgrade');
              }
            "
          >
            <template #description>
              <i18n-t
                scope="global"
                keypath="character.inventory.item.upgrade.confirm.description"
                tag="div"
              >
                <template #loomPoints>
                  <Loom :point="1" />
                </template>

                <template #oldItem>
                  <span class="font-bold" :style="{ color: getRankColor(item.rank) }">
                    {{ item.name }}
                  </span>
                </template>

                <template #newItem>
                  <span class="font-bold" :style="{ color: getRankColor(nextItem.rank) }">
                    {{ nextItem.name }}
                  </span>
                </template>
              </i18n-t>
            </template>
          </ConfirmActionForm>
        </template>
      </Modal>
    </div>

    <OTable
      :data="itemUpgrades"
      bordered
      narrowed
      hoverable
      :selected="currentItem"
      customRowKey="id"
    >
      <OTableColumn field="name" #default="{ row: rowItem }: { row: ItemFlat }">
        <div class="relative">
          <ShopGridItemName :item="rowItem">
            <template v-if="currentItem?.id === rowItem.id" #bottom-right>
              <Tag
                rounded
                variant="primary"
                icon="check"
                v-tooltip="$t('character.inventory.item.upgrade.currentItem')"
              />
            </template>
          </ShopGridItemName>
        </div>
      </OTableColumn>

      <OTableColumn
        v-for="field in (Object.keys(cols) as Array<keyof ItemFlat>)"
        #default="{ row: rowItem }: { row: ItemFlat }"
        :field="field"
        :label="$t(`item.aggregations.${field}.title`)"
        :width="120"
      >
        <ItemParam
          :item="rowItem"
          :field="field"
          :bestValue="compareItemsResult[field]"
          compareMode
        />
      </OTableColumn>

      <template #empty>
        <div class="relative min-h-[10rem]">
          <OLoading active iconSize="xl" :fullPage="false" />
        </div>
      </template>
    </OTable>
  </div>
</template>
