<script setup lang="ts"></script>

<template>
  <main>
    <RouterView v-slot="{ Component }">
      <Suspense>
        <component :is="Component" />
        <template #fallback>
          <OLoading fullPage active iconSize="xl" />
        </template>
      </Suspense>
    </RouterView>
  </main>
</template>
