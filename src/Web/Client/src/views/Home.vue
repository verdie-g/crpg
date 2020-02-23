<template>
  <section class="section">
    <div class="container content is-large">
      <h1>cRPG</h1>
      <p>
        cRPG is a mod for
        <a href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord" target="_blank">Mount & Blade II: Bannerlord</a>.
        It adds persistence to the multiplayer. Your start as a peasant and you'll develop your unique character with different stats and
        items.
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
import userModule from '@/store/user-module';

@Component
export default class Home extends Vue {
  get isSignedIn() {
    return userModule.user !== null;
  }

  onClick() {
    signIn();
  }
}
</script>

<style scoped lang="scss">
</style>
