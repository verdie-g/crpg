<script setup lang="ts">
const router = useRouter();
const { t } = useI18n();
</script>

<template>
  <main class="px-4 py-10 text-center">
    <RouterView />
    <div>
      <button @click="router.back()">
        {{ t('button.back') }}
      </button>
    </div>
    <div class="mx-auto mt-5 text-center">[404 Layout]</div>
  </main>
</template>
