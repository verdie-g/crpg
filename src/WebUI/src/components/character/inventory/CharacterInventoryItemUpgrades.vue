<script setup lang="ts">
import { ItemCompareMode, ItemRank, type ItemFlat } from '@/models/item';
import { type AggregationConfig } from '@/models/item-search';
import { useItemUpgrades } from '@/composables/item/use-item-upgrades';
import { useItemReforge } from '@/composables/item/use-item-reforge';
import { useUserStore } from '@/stores/user';
import { getRankColor } from '@/services/item-service';

const userStore = useUserStore();

const { item, cols } = defineProps<{
  item: ItemFlat;
  cols: AggregationConfig;
}>();

const emit = defineEmits<{
  upgrade: [];
  reforge: [];
}>();

const {
  itemUpgrades,
  relativeEntries,
  currentItem,
  baseItem,
  nextItem,
  validation: upgradeValidation,
  canUpgrade,
} = useItemUpgrades(item, cols);

const {
  reforgeCost,
  reforgeCostTable,
  validation: reforgeValidation,
  canReforge,
} = useItemReforge(item);
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-12 pl-4 pr-16">
      <div class="flex items-center gap-4">
        <h3 class="text-xl font-semibold">
          {{ $t('character.inventory.item.upgrade.upgradesTitle') }}
        </h3>
        <Loom :point="userStore.user!.heirloomPoints" />
        <Coin :value="userStore.user!.gold" />
      </div>

      <div class="flex items-center gap-4">
        <Modal :disabled="!canUpgrade">
          <VTooltip>
            <div>
              <OButton variant="primary" outlined size="lg" :disabled="!canUpgrade">
                {{ $t('action.upgrade') }}
                <Loom :point="1" />
              </OButton>
            </div>
            <template #popper>
              <div class="prose prose-invert">
                <h4>
                  {{ $t('character.inventory.item.upgrade.tooltip.title') }}
                </h4>

                <p>
                  {{ $t('character.inventory.item.upgrade.tooltip.description') }}
                </p>

                <i18n-t
                  v-if="!upgradeValidation.maxRank"
                  scope="global"
                  keypath="character.inventory.item.upgrade.validation.maxRank"
                  class="text-status-danger"
                  tag="p"
                />

                <i18n-t
                  v-else-if="!upgradeValidation.points"
                  scope="global"
                  keypath="character.inventory.item.upgrade.validation.loomPoints"
                  class="text-status-danger"
                  tag="p"
                />

                <i18n-t
                  v-else-if="!upgradeValidation.exist"
                  scope="global"
                  keypath="character.inventory.item.upgrade.validation.exist"
                  class="text-status-danger"
                  tag="p"
                />
              </div>
            </template>
          </VTooltip>

          <template #popper="{ hide }">
            <ConfirmActionForm
              :title="$t('action.confirmation')"
              :name="'Upgrade item'"
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

        <Modal :disabled="!canReforge">
          <VTooltip>
            <div>
              <OButton variant="primary" outlined size="lg" :disabled="!canReforge">
                {{ $t('action.reforge') }}
                <Coin v-if="reforgeValidation.rank" :value="reforgeCost" />
              </OButton>
            </div>

            <template #popper>
              <div class="prose prose-invert">
                <h4>
                  {{ $t('character.inventory.item.reforge.tooltip.title') }}
                </h4>

                <p>
                  {{ $t('character.inventory.item.reforge.tooltip.description') }}
                </p>

                <OTable :data="reforgeCostTable" bordered narrowed>
                  <OTableColumn
                    #default="{ row }: { row: [string, number] }"
                    field="rank"
                    :label="'Item Rank'"
                  >
                    <span :style="{ color: getRankColor(Number(row[0]) as ItemRank) }">
                      +{{ row[0] }}
                    </span>
                  </OTableColumn>

                  <OTableColumn
                    #default="{ row }: { row: [string, number] }"
                    field="rank"
                    :label="'Cost'"
                  >
                    <Coin :value="row[1]" />
                  </OTableColumn>
                </OTable>

                <i18n-t
                  v-if="!reforgeValidation.rank"
                  scope="global"
                  keypath="character.inventory.item.reforge.validation.rank"
                  class="text-status-danger"
                  tag="p"
                >
                  <template #minimumRank>
                    <span class="font-bold">0</span>
                  </template>
                </i18n-t>

                <i18n-t
                  v-else-if="!reforgeValidation.gold"
                  scope="global"
                  keypath="character.inventory.item.reforge.validation.gold"
                  class="text-status-danger"
                  tag="p"
                />

                <i18n-t
                  v-else-if="!reforgeValidation.exist"
                  scope="global"
                  keypath="character.inventory.item.reforge.validation.exist"
                  class="text-status-danger"
                  tag="p"
                >
                  <template #existItem>
                    <span class="font-bold">{{ baseItem.name }}</span>
                  </template>
                </i18n-t>
              </div>
            </template>
          </VTooltip>

          <template #popper="{ hide }">
            <ConfirmActionForm
              :title="$t('action.confirmation')"
              :name="'Reforge item'"
              :confirmLabel="$t('action.confirm')"
              @cancel="hide"
              @confirm="
                () => {
                  hide();
                  emit('reforge');
                }
              "
            >
              <template #description>
                <i18n-t
                  scope="global"
                  keypath="character.inventory.item.reforge.confirm.description"
                  tag="div"
                >
                  <template #gold>
                    <Coin :value="reforgeCost" />
                  </template>

                  <template #loomPoints>
                    <Loom :point="1" />
                  </template>

                  <template #oldItem>
                    <span class="font-bold" :style="{ color: getRankColor(item.rank) }">
                      {{ item.name }}
                    </span>
                  </template>

                  <template #newItem>
                    <span class="font-bold" :style="{ color: getRankColor(baseItem.rank) }">
                      {{ baseItem.name }}
                    </span>
                  </template>
                </i18n-t>
              </template>
            </ConfirmActionForm>
          </template>
        </Modal>
      </div>
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
          isCompare
          :compareMode="ItemCompareMode.Relative"
          :relativeValue="relativeEntries[field]"
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
