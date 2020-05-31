<template>
  <div>

    <nav v-if="isSignedIn">
      <b-navbar fixed-top>
        <template slot="brand">
          <b-navbar-item tag="router-link" :to="{ path: '/' }">cRPG</b-navbar-item>
        </template>

        <template slot="start">
          <b-navbar-item tag="router-link" :to="{ path: '/characters' }">Characters</b-navbar-item>
          <b-navbar-item tag="router-link" :to="{ path: '/shop' }">Shop</b-navbar-item>
        </template>

        <template slot="end">
          <b-navbar-item tag="div" v-if="user">
              <div class="media">
                <div class="media-content">
                  <p>
                    <strong>{{user.userName}}</strong><br>
                    <b-icon icon="coins" size="is-small" style="margin-right: 6px" />{{user.gold}}
                  </p>
                </div>
                <figure class="media-right">
                  <b-dropdown aria-role="list" position="is-bottom-left">
                    <p class="image" slot="trigger" style="cursor: pointer">
                      <img v-bind:src="user.avatarSmall" alt="avatar" />
                    </p>

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
      <router-view/>
    </main>

    <footer class="footer">
      <div class="level">
        <div class="level-item">
          <a href="https://github.com/verdie-g/cRPG" target="_blank" title="Contribute to the project on Github">
            <b-icon icon="github" pack="fab" size="is-medium" />
          </a>
        </div>

        <div class="level-item">
          <a href="https://www.patreon.com" target="_blank" title="Donate on Patreon">
            <b-icon icon="patreon" pack="fab" size="is-medium" />
          </a>
        </div>

        <div class="level-item">
          <a href="https://discord.gg/83RJDN9" target="_blank" title="Join our Discord">
            <b-icon icon="discord" pack="fab" size="is-medium" />
          </a>
        </div>

        <div class="level-item">
          <a href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord" target="_blank" title="Buy the game">
            <b-icon icon="steam" pack="fab" size="is-medium" />
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
import { getToken, setToken, clearToken } from './services/auth-service';

@Component
export default class App extends Vue {
  get isSignedIn(): boolean {
    return userModule.isSignedIn;
  }

  get user(): User | null {
    return userModule.user;
  }

  created(): void {
    this.handleAuthenticationCallback();
    if (getToken() !== undefined) {
      userModule.signIn();
      userModule.getUser();
    }
  }

  signOut(): void {
    clearToken();
    userModule.signOut();
    if (this.$router.currentRoute.fullPath !== '/') {
      this.$router.push('/');
    }
  }

  handleAuthenticationCallback(): void {
    const token = this.$route.query.token as string;
    if (token === undefined) {
      return;
    }

    this.$router.replace('');
    setToken(token);
  }
}
</script>

<style lang="scss">
</style>
