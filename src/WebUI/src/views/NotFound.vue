<template>
  <div class="container">
    <div class="section">
      <h2 class="title">{{ $t('notFoundTitle') }}</h2>
      <h3 class="subtitle">{{ $t('notFoundDescription') }}</h3>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class NotFound extends Vue {}
</script>

<style scoped lang="scss"></style>
