<template>
  <div class="container">
    <section class="section">
      <div class="content is-large">
        <h1>cRPG</h1>
        <p>
          cRPG is a mod for
          <a href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord" target="_blank">Mount & Blade II: Bannerlord</a>.
          It adds persistence to the multiplayer. Your start as a peasant and you'll develop your unique character with different stats and
          items.
        </p>

        <p>
          The mod was announced recently in this <a href="https://forums.taleworlds.com/index.php?threads/crpg.432051" target="_blank">Forum thread</a>.
          As TaleWorlds haven't provided the tools to make multiplayer mods yet, only a singleplayer mod is available on
          <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2208?tab=files" target="_blank">Nexus Mods</a> mostly to test the system.
          Follow the development on the <a href="https://discord.gg/83RJDN9" target="_blank">cRPG Discord</a>.
        </p>
        <b-button size="is-large" icon-left="steam-symbol" icon-pack="fab" @click="onClick" v-if="!isSignedIn">
          Sign in through Steam
        </b-button>
      </div>

      <div class="content is-medium">
        <h2>F.A.Q</h2>
        <dl id="faq">
          <!--
          <dt>How to create a new character?</dt>
          <dd>Simply connect to a cRPG server and a character with your steam user name will be created.</dd>

          <dt>How to gain experience and gold?</dt>
          <dd>
            Experience and gold are gained by playing on cRPG servers. These earnings are multiplied when you win a round.
          </dd>
          -->

          <dt>I've renamed my steam account and a new character level 1 was created, how can I play with my original character?</dt>
          <dd>
            When you connect to a cRPG server, it searches for a character with the same name as your steam name. If it
            doesn't find one, a new character is created. To use your original character, rename it to your new steam name
            it in the Web UI.
          </dd>

          <dt>What is retiring?</dt>
          <dd>
            When a cRPG character reaches level 31, it can be retired. Retiring a character resets it to level 1 but
            grants an experience bonus multiplier and an heirloom point which can used on an item to increate its
            statistics.
          </dd>

          <dt>What is respecialization?</dt>
          <dd>
            If you want to play a different class of soldier, you can respecialize (or respec) to reset your character
            stats for the cost of half of your experience.
          </dd>

          <!--
          <dt>Can I host my own cRPG game server?</dt>
          <dd>For security and maintainability reasons, players won't be able to host their own server.</dd>
          -->

          <dt>Is the project open-source?</dt>
          <dd>Not decided yet.</dd>

          <dt>How can I help?</dt>
          <dd>
            If you want to help developping the mod and that you already have a <b>professional</b> experience with
            either .NET, game development, or a popular Javascript framework (Vue, React, Angular), please contact a
            developper on the cRPG Discord. You can also help by donating to cover server costs.
          </dd>

          <dt>How to donate?</dt>
          <dd>
            You can donate on the <a href="https://patreon.com/crpg" target="_blank">Patreon</a>. Note that donations
            will only be used to cover server costs.
          </dd>

        </dl>
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
  get isSignedIn(): boolean {
    return userModule.isSignedIn;
  }

  onClick(): void {
    signIn();
  }
}
</script>

<style scoped lang="scss">
#faq {
  dt {
    font-weight: bold;
  }
}
</style>
