<template>
  <section class="section">
    <div class="container content is-large">
      <h1>tRPG</h1>
      <p>
        Inspired by <a href="https://c-rpg.net" target="_blank">cRPG</a>, tRPG is a mod for
        <a href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord" target="_blank">Mount & Blade II: Bannerlord</a>.
        It adds persistence to the multiplayer. Your start as a peasant and you'll develop your unique character with different stats and
        equipments.
      </p>
      <b-button size="is-large" icon-left="steam-symbol" icon-pack="fab" @click="onClick" v-if="!isSignedIn">
        Sign in through Steam
      </b-button>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { isSignedIn, signIn } from '@/services/auth-service';

@Component
export default class Home extends Vue {
  get isSignedIn() {
    return isSignedIn();
  }

  onClick() {
    signIn();
  }
}
</script>

<style scoped lang="scss">
</style>
