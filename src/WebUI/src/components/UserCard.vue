<template>
  <div class="media">
    <div class="media-left">
      <figure class="image is-64x64">
        <img :src="user.avatar" :alt="user.name" />
      </figure>
    </div>
    <div class="media-content">
      <p class="title is-4">
        <router-link
          v-if="useLink"
          class="is-inline-flex is-align-items-end"
          :to="{
            name: 'admin-user-restrictions',
            params: { id: user.id },
          }"
        >
          <span>{{ user.name }}</span>
          <b-icon icon="external-link-alt" class="is-size-6" />
        </router-link>
        <span v-else>{{ user.name }}</span>
      </p>
      <p class="subtitle is-6">
        <span>Id: {{ user.id }}, {{ user.platform }}: {{ user.platformUserId }}</span>
        <platform :platform="user.platform" :platformUserId="user.platformUserId" />
      </p>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import UserPublic from '@/models/user-public';
import PlatformComponent from '@/components/Platform.vue';

@Component({
  components: { Platform: PlatformComponent },
})
export default class UserCard extends Vue {
  @Prop() readonly user: UserPublic;
  @Prop({ type: Boolean, default: false }) readonly useLink: boolean;
}
</script>

<style scoped lang="scss"></style>
