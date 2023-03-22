<script setup lang="ts">
import { WeaponUsage, type ItemFlat } from '@/models/item';
import { getItemImage, weaponClassToIcon } from '@/services/item-service';

const props = withDefaults(
  defineProps<{
    item: ItemFlat;
  }>(),
  {}
);
</script>

<template>
  <div class="flex items-center gap-4">
    <div class="relative h-16 w-32">
      <VTooltip placement="auto">
        <img :src="getItemImage(item.id)" class="h-full w-full object-contain" />
        <template #popper>
          <div class="mb-2 text-content-100">{{ item.name }}</div>
          <img :src="getItemImage(item.id)" class="h-full w-full object-contain" />
        </template>
      </VTooltip>

      <VTooltip placement="auto" class="absolute bottom-0 right-0">
        <Tag :label="String(item.tier)" size="sm" />

        <template #popper>
          <div class="prose prose-invert">
            <h5 class="text-content-100">
              {{ $t(`item.aggregations.tier.title`) }}
            </h5>
            <div v-html="$t(`item.aggregations.tier.description`)" />
          </div>
        </template>
      </VTooltip>

      <template v-if="item.weaponUsage.includes(WeaponUsage.Secondary)">
        <VTooltip class="absolute top-0 left-0" placement="auto">
          <Tag variant="warning" rounded icon="alert" size="sm" />

          <template #popper>
            <div class="prose prose-invert">
              <h5 class="text-status-warning">
                {{ $t(`shop.item.weaponUsage.title`) }}
              </h5>

              <p>
                {{ $t(`shop.item.weaponUsage.desc`) }}
              </p>

              <i18n-t scope="global" keypath="shop.item.weaponUsage.secondary" tag="p">
                <template #weaponClass>
                  <div class="flex items-center gap-1 font-bold text-content-100">
                    <OIcon size="lg" :icon="weaponClassToIcon[item.weaponClass!]" />
                    <span>{{ $t(`item.weaponClass.${item.weaponClass}`) }}</span>
                  </div>
                </template>
              </i18n-t>

              <i18n-t scope="global" keypath="shop.item.weaponUsage.primary" tag="p">
                <template #weaponClass>
                  <div class="flex items-center gap-1 font-bold text-content-100">
                    <OIcon size="lg" :icon="weaponClassToIcon[item.weaponPrimaryClass!]" />
                    <span>{{ $t(`item.weaponClass.${item.weaponPrimaryClass}`) }}</span>
                  </div>
                </template>
              </i18n-t>
            </div>
          </template>
        </VTooltip>
      </template>
    </div>

    <div class="flex-1 text-2xs text-content-100">{{ item.name }}</div>
  </div>
</template>
