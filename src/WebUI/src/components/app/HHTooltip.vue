<script setup lang="ts">
import { Region } from '@/models/region';

defineProps<{ region: Region }>();
</script>

<template>
  <VDropdown theme="dropdown-light">
    <template #default="slotProps">
      <slot name="default" v-bind="slotProps" />
    </template>

    <template #popper="{ hide }">
      <div class="px-4 py-3 text-content-200">
        <OButton
          class="!absolute right-2 top-1"
          iconRight="close"
          rounded
          size="2xs"
          variant="secondary"
          @click="hide"
        />

        <i18n-t
          scope="global"
          keypath="hh.tooltip-content.template"
          tag="div"
          class="prose prose-invert max-w-xs pr-6"
        >
          <template #title>
            <h4 class="text-content-100">{{ $t('hh.tooltip-content.title') }}</h4>
          </template>

          <template #desc>
            <i18n-t scope="global" keypath="hh.tooltip-content.desc" tag="p" class="text-2xs">
              <template #descHighlight>
                <span class="text-content-100">
                  {{ $t('hh.tooltip-content.descHighlight') }}
                </span>
              </template>
            </i18n-t>
          </template>

          <template #region>
            <i18n-t scope="global" keypath="hh.tooltip-content.region" tag="p" class="text-2xs">
              <template #currentRegion>
                <span class="text-content-100">
                  {{ $t(`region.${region}`, 0) }}
                </span>
              </template>
            </i18n-t>
          </template>
        </i18n-t>
      </div>
    </template>
  </VDropdown>
</template>
