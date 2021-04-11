<template>
  <a :href="'https://steamcommunity.com/profiles/' + platformUserId" target="_blank">
    <b-icon icon="steam" pack="fab" />
  </a>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import Platform from '@/models/platform';

@Component
export default class PlatformComponent extends Vue {
  @Prop(String) readonly platform: Platform;
  @Prop(String) readonly platformUserId: string;
}
</script>

<style scoped lang="scss"></style>
