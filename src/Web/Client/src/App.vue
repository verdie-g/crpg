<template>
    <router-view/>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import { setToken } from './utils/auth';

@Component
export default class App extends Vue {
  created() {
    this.handleAuthenticationCallback();
  }

  handleAuthenticationCallback() {
    const token = this.$route.query.token as string;
    if (token === undefined) {
      return;
    }

    this.$router.replace('');
    setToken(token);
    userModule.getUser();
  }
}
</script>

<style lang="scss">
</style>
