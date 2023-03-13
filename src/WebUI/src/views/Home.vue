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

        <div class="buttons">
          <b-button
            size="is-large"
            type="is-link"
            icon-left="download"
            tag="a"
            href="https://steamcommunity.com/sharedfiles/filedetails/?id=2878356589"
            target="_blank"
          >
            Download
          </b-button>
        <b-button
          size="is-large"
          type="is-link"
          icon-left="steam-symbol"
          icon-pack="fab"
          @click="onClick"
          :loading="isSigningIn"
          v-if="!isSignedIn"
        >
          Sign in through Steam
        </b-button>
        </div>

        <p>
          Join our community on
          <a href="https://discord.gg/c-rpg" target="_blank">Discord</a>
          or on
          <a href="https://www.reddit.com/r/CRPG_Bannerlord" target="_blank">Reddit</a>.
        </p>

        <p>
          <b-icon icon="circle" size="is-small" class="mr-3 has-text-primary" />
          {{ gameServerStats.total.playingCount }} online
        </p>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { signIn } from '@/services/auth-service';
import userModule from '@/store/user-module';
import { getGameServerStats } from '@/services/game-server-statistics-service';
import { GameServerStats } from '@/models/game-server-stats';
import Region from '@/models/region';

@Component
export default class Home extends Vue {
  signingIn = false;
  gameServerStats: GameServerStats = {
    total: { playingCount: 0 },
    regions: {
      [Region.Eu]: { playingCount: 0 },
      [Region.Na]: { playingCount: 0 },
      [Region.As]: { playingCount: 0 },
      [Region.Oc]: { playingCount: 0 },
    },
  };

  get isSignedIn(): boolean {
    return userModule.isSignedIn;
  }

  get isSigningIn(): boolean {
    return userModule.userLoading || this.signingIn;
  }

  async created() {
    this.gameServerStats = await getGameServerStats();
  }

  onClick(): void {
    this.signingIn = true;
    signIn();
  }
}
</script>

<style scoped lang="scss"></style>
