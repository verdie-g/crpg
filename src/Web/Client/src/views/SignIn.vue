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
      <b-button size="is-large" icon-left="steam-symbol" icon-pack="fab" @click="onClick">Sign in through Steam</b-button>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class SignIn extends Vue {
  onClick() {
    window.location.href = 'http://localhost:5000/api/auth/signIn?redirectUri=' + encodeURIComponent(window.location.origin);
  }
}
</script>

<style scoped lang="scss">
</style>
