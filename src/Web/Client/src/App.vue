<template>
  <div>

    <nav v-if="isSignedIn" style="margin-bottom: 25px">
      <b-navbar fixed-top shadow>
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
                    <b-icon icon="coins" size="is-small" style="margin-right: 6px" />{{user.money}}
                  </p>
                </div>
                <figure class="media-right">
                  <b-dropdown aria-role="list" position="is-bottom-left">
                    <p class="image" slot="trigger" style="cursor: pointer">
                      <img v-bind:src="user.avatarSmall" alt="avatar" />
                    </p>

                    <b-dropdown-item aria-role="listitem" @click="signOut">
                      <div class="media">
                        <b-icon class="media-left" icon="sign-out-alt" />
                        <div class="media-content">
                          <h3>Sign out</h3>
                        </div>
                      </div>
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
          <a href="https://github.com/verdie-g/cRPG" target="_blank">
            <b-icon icon="github" pack="fab" size="is-medium" />
          </a>
        </div>

        <div class="level-item">
          <a href="https://discord.gg/kXaeNB8" target="_blank">
            <b-icon icon="discord" pack="fab" size="is-medium" />
          </a>
        </div>

        <div class="level-item">
          <a href="#" target="_blank">
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
import { isSignedIn, setToken, signOut } from './services/auth-service';

  @Component
export default class App extends Vue {
  get isSignedIn() {
    return userModule.user !== null;
  }

  get user() {
    return userModule.user;
  }

  created() {
    this.handleAuthenticationCallback();

    if (isSignedIn()) {
      userModule.getUser();
    }
  }

  signOut() {
    signOut();
    userModule.resetUser();
    if (this.$router.currentRoute.fullPath !== '/') {
      this.$router.push('/');
    }
  }

  handleAuthenticationCallback() {
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
