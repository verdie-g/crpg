<template>
  <section class="section">
    <div class="container">
      <h1 class="is-size-2">Create a new clan</h1>

      <ClanFormComponent
        v-bind="{ mode: clanFormMode, isLoading: creatingClan }"
        @submit="create"
      />
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { notify } from '@/services/notifications-service';
import * as clanService from '@/services/clan-service';
import { ClanEditionModes, ClanEditionMode } from '@/models/clan-edition';
import Clan from '@/models/clan';

import ClanFormComponent from '@/components/ClanForm.vue';

@Component({
  components: { ClanFormComponent },
})
export default class ClanCreationComponent extends Vue {
  clanFormMode: ClanEditionMode = ClanEditionModes.Create;
  creatingClan = false;

  async create(payload: Clan): Promise<void> {
    this.creatingClan = true;
    const clan = await clanService.createClan(payload);
    this.creatingClan = false;
    notify('Clan created');
    this.$router.push({ name: 'clan', params: { id: clan.id.toString() } });
  }
}
</script>

<style scoped lang="scss"></style>
