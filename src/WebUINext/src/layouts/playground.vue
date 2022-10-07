<template>
  <main class="px-4 py-10">
    <div class="mb-12 flex flex-wrap gap-4">
      <router-link :to="{ name: 'oruga-playground' }">oruga-playground</router-link>
      <router-link :to="{ name: 'naiveui-playground' }">naiveui-playground</router-link>
    </div>
    <RouterView />
  </main>
</template>
