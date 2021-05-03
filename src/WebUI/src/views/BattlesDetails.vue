<template>
  <section class="section">
    <router-link class="is-flex is-align-items-center mb-3" :to="{ name: 'battles' }">
      <i class="fas fa-chevron-left mr-1"></i>
      Back
    </router-link>
    <div class="columns">
      <div class="column mx-6">
        <article class="tile is-child box is-flex is-justify-content-center">
          <p class="subtitle">Attackers</p>
          <p class="subtitle"></p>
        </article>
      </div>
      <div class="column mx-6">
        <article class="tile is-child box is-flex is-justify-content-center">
          <p class="subtitle">Defenders</p>
          <p class="subtitle"></p>
        </article>
      </div>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class BattlesDetails extends Vue {}
</script>

<style scoped lang="scss"></style>
