<template>
  <div>
    <nav v-if="user">
      <b-navbar fixed-top :close-on-click="false">
        <template slot="brand">
          <b-navbar-item tag="router-link" :to="{ path: '/' }">cRPG</b-navbar-item>
        </template>

        <template slot="start">
          <b-navbar-item tag="router-link" :to="{ path: '/characters' }">Characters</b-navbar-item>
          <b-navbar-item tag="router-link" :to="{ path: '/shop' }">Shop</b-navbar-item>
          <b-navbar-item tag="router-link" :to="{ path: '/clans' }">Clans</b-navbar-item>
          <b-navbar-dropdown label="Strategus">
            <b-navbar-item tag="router-link" :to="{ path: '/strategus' }">Map</b-navbar-item>
            <b-navbar-item tag="router-link" :to="{ path: '/battles' }">Battles</b-navbar-item>
          </b-navbar-dropdown>
        </template>

        <template slot="end">
          <b-navbar-item tag="div" v-if="user">
            <div class="media">
              <div class="media-content">
                <p>
                  <strong>{{ user.name }}</strong>
                  <br />
                  <b-icon icon="coins" size="is-small" style="margin-right: 6px" />
                  {{ user.gold }}
                </p>
              </div>
              <figure class="media-right">
                <b-dropdown aria-role="list" position="is-bottom-left">
                  <p class="image" slot="trigger" style="cursor: pointer">
                    <img v-bind:src="user.avatarSmall" alt="avatar" />
                  </p>

                  <b-dropdown-item has-link aria-role="menuitem" v-if="isModeratorOrAdmin">
                    <router-link to="/admin">
                      <b-icon icon="user-shield" />
                      Administration
                    </router-link>
                  </b-dropdown-item>

                  <b-dropdown-item has-link aria-role="menuitem">
                    <router-link to="/settings">
                      <b-icon icon="cog" />
                      Settings
                    </router-link>
                  </b-dropdown-item>

                  <b-dropdown-item value="home" aria-role="menuitem" @click="signOut">
                    <b-icon icon="sign-out-alt" />
                    Sign out
                  </b-dropdown-item>
                </b-dropdown>
              </figure>
            </div>
          </b-navbar-item>
        </template>
      </b-navbar>
    </nav>

    <main>
      <router-view />
    </main>
    <!-- Display or not the footer depending on the current page -->
    <footer v-if="$route.meta.footer === true || $route.meta.footer === undefined" class="footer">
      <div class="level">
        <div class="level-item">
          <a href="https://www.patreon.com/crpg" target="_blank" title="Donate on Patreon">
            <b-icon icon="patreon" pack="fab" size="is-large" aria-label="cRPG Patreon" />
          </a>
        </div>

        <div class="level-item">
          <a href="https://discord.gg/83RJDN9" target="_blank" title="Join our Discord">
            <b-icon icon="discord" pack="fab" size="is-large" aria-label="cRPG Discord" />
          </a>
        </div>

        <div class="level-item">
          <a href="https://forum.c-rpg.eu" target="_blank" title="Join our Forum">
            <b-icon icon="discourse" pack="fab" size="is-large" aria-label="cRPG Forum" />
          </a>
        </div>

        <div class="level-item">
          <a
            href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord"
            target="_blank"
            title="Buy the game"
          >
            <b-icon icon="steam" pack="fab" size="is-large" aria-label="Mount & Blade Steam page" />
          </a>
        </div>
      </div>
    </footer>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import User from '@/models/user';
import { signInCallback, signOut, signInSilent } from './services/auth-service';

@Component
export default class App extends Vue {
  get user(): User | null {
    return userModule.user;
  }

  get isModeratorOrAdmin() {
    return userModule.isModeratorOrAdmin;
  }

  async beforeCreate() {
    userModule.setUserLoading(true);
    try {
      // If the 'code' parameter is present in the query, this is the response
      // of the authorization endpoint and it should be processed
      if (this.$route.query.code !== undefined) {
        await signInCallback();
        this.$router.replace(''); // clear query parameters
        await userModule.getUser();
        return;
      }
      // Try to sign in the user if already signed in to the authorization server
      // & get user info if user is connected
      try {
        const token = await signInSilent();
        if (token !== null) {
          await userModule.getUser();
        }
      } catch {
        // The grant is probably not valid anymore because the server was restarted.
        if (this.$route.path !== '/') {
          this.$router.push('/');
        }
      }
    } finally {
      userModule.setUserLoading(false);
    }
  }
  
  signOut(): void {
    signOut();
  }
}
</script>
