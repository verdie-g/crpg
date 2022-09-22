<template>
  <div class="container">
    <section class="section">
      <div class="content is-large">
        <h1>cRPG</h1>
        <p>
          cRPG is a mod for
          <a
            href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord"
            target="_blank"
          >
            Mount & Blade II: Bannerlord
          </a>
          . It adds persistence to the multiplayer. You start as a peasant and you'll develop your
          unique character with different stats and items.
        </p>

        <b-button
          size="is-large"
          icon-left="steam-symbol"
          icon-pack="fab"
          @click="onClick"
          :loading="isSigningIn"
          v-if="!isSignedIn"
        >
          Sign in through Steam
        </b-button>

        <p>
          Join our community on
          <a href="https://discord.gg/c-rpg" target="_blank">Discord</a>
          .
        </p>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { signIn } from '@/services/auth-service';
import userModule from '@/store/user-module';

@Component
export default class Home extends Vue {
  signingIn = false;

  get isSignedIn(): boolean {
    return userModule.isSignedIn;
  }

  get isSigningIn(): boolean {
    return userModule.userLoading || this.signingIn;
  }

  onClick(): void {
    this.signingIn = true;
    signIn();
  }
}
</script>

<style scoped lang="scss"></style>
