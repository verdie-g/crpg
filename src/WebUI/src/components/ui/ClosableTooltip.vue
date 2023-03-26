<script setup lang="ts"></script>

<template>
  <VTooltip>
    <slot />
    <template #popper="{ hide }">
      <div class="max-w-xs pr-7">
        <OButton
          class="absolute -right-2 -top-2"
          iconRight="close"
          rounded
          size="2xs"
          variant="secondary"
          @click="hide"
        />

        <slot name="popper" />
      </div>
    </template>
  </VTooltip>
</template>
