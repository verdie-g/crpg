<template>
    <router-view/>
</template>

<script lang="ts">
import { Vue } from 'vue-property-decorator';
import Cookies from 'js-cookie';
import { setToken } from './utils/auth';

export default class App extends Vue {
  created() {
    // After authenticating to steam a cookie named 'jwt' is generated containing the jwt.
    const token = Cookies.get('jwt');
    if (token !== undefined) {
      setToken(token);
      Cookies.remove('jwt');
    }
  }
}
</script>

<style lang="scss">
</style>
