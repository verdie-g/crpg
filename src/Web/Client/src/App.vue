<template>
  <div>

    <nav v-if="isSignedIn">
      <b-navbar fixed-top shadow>
        <template slot="brand">
          <b-navbar-item tag="router-link" :to="{ path: '/' }">tRPG</b-navbar-item>
        </template>

        <template slot="start">
          <b-navbar-item href="#">Characters</b-navbar-item>
          <b-navbar-item href="#">Documentation</b-navbar-item>
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
                <p class="image">
                  <img v-bind:src="user.avatar" alt="avatar" />
                </p>
              </figure>
            </div>
          </b-navbar-item>
        </template>
      </b-navbar>
    </nav>

    <main>
      <router-view/>
    </main>
  </div>
</template>

<script lang="ts">
  import { Component, Vue } from 'vue-property-decorator';
  import userModule from '@/store/user-module';
  import { setToken, isSignedIn } from './services/auth-service';

  @Component
  export default class App extends Vue {
    get isSignedIn() {
      return isSignedIn();
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
