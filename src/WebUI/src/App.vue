<script setup lang="ts">
// import { library } from '@fortawesome/fontawesome-svg-core';
</script>

<template>
  <RouterView v-slot="{ Component }">
    <Suspense>
      <!-- TODO: to sandbox, only dev mode page -->
      <!-- <div class="grid grid-cols-12 gap-2">
          <div
            v-for="(key, icon) in library.definitions.crpg"
            class="flex flex-col items-center justify-center gap-2 hover:ring"
          >
            <OIcon :icon="icon" size="5x" class="text-content-100" />
            <div class="text-center text-[8px]">{{ icon }}</div>
          </div>
        </div> -->

      <ErrorBoundary>
        <component :is="Component" />
      </ErrorBoundary>

      <template #fallback>
        <OLoading fullPage active iconSize="xl" />
      </template>
    </Suspense>
  </RouterView>
</template>
