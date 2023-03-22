<script setup lang="ts">
import { useModalCounter } from '@/composables/use-modal-count';
import { disableBodyScroll, enableBodyScroll } from 'body-scroll-lock';

defineProps<{ closable?: boolean }>();

const { counter, increase, decrease } = useModalCounter();

const onShow = () => {
  if (counter.value === 0) {
    disableBodyScroll(document.querySelector('body')!, { reserveScrollBarGap: true });
  }

  increase();
};

const onHide = () => {
  decrease();

  if (counter.value === 0) {
    enableBodyScroll(document.querySelector('body')!);
  }
};
</script>

<template>
  <VDropdown positioningDisabled @applyShow="onShow" @applyHide="onHide">
    <template #default="popper">
      <slot v-bind="popper" />
    </template>

    <template #popper="popper">
      <OButton
        v-if="closable"
        class="absolute right-4 top-4"
        iconRight="close"
        rounded
        size="sm"
        variant="secondary"
        @click="popper.hide"
      />
      <slot name="popper" v-bind="popper" />
    </template>
  </VDropdown>
</template>
