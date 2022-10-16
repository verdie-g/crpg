<script setup lang="ts"></script>

<template>
  <RouterView v-slot="{ Component }">
    <Suspense>
      <div>
        <component :is="Component" />
      </div>
    </Suspense>
  </RouterView>
</template>
