<template>
    <router-view/>
</template>

<script lang="ts">
import { Vue } from "vue-property-decorator";
import Cookies from 'js-cookie';

export default class App extends Vue {
  beforeCreate() {
    // After authenticating to steam a cookie named 'jwt' is generated containing the jwt.
    const jwt = Cookies.get('jwt');
    if (jwt !== undefined) {
      localStorage.jwt = jwt;
      Cookies.remove('jwt');
    }
  }
}
</script>

<style lang="scss">
</style>
