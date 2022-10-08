<template>
  <section class="section">
    <div class="container" v-if="clan !== null">
      <div class="columns is-vcentered">
        <h1 class="column is-size-2">[{{ clan.tag }}] {{ clan.name }} - Settings</h1>
        <div class="column is-narrow">
          <b-button
            type="is-link"
            size="is-medium"
            tag="router-link"
            :to="{ name: 'clan', params: { id: $route.params.id } }"
            :disabled="clan === null"
          >
            My clan
          </b-button>
        </div>
      </div>

      <ClanFormComponent
        v-bind="{ ...clan, mode: clanFormMode, isLoading: updatingClan }"
        @submit="update"
      />
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import * as clanService from '@/services/clan-service';
import Clan from '@/models/clan';
import { ClanFormMode, ClanFormModeVariant } from '@/models/clan-form';
import { notify } from '@/services/notifications-service';
import ClanFormComponent from '@/components/ClanForm.vue';

@Component({
  components: { ClanFormComponent },
})
export default class ClanSettings extends Vue {
  clan: Clan | null = null;
  clanFormMode: ClanFormMode = ClanFormModeVariant.Update;
  updatingClan = false;

  async created() {
    const clanId = parseInt(this.$route.params.id as string);

    if (Number.isNaN(clanId)) {
      this.$router.push({ name: 'not-found' });
      return;
    }

    const clan = await clanService.getClan(clanId);

    this.clan = clan;
  }

  async update(payload: Clan): Promise<void> {
    if (this.clan?.id) {
      this.updatingClan = true;
      const clan = await clanService.updateClan(this.clan.id, payload);
      this.clan = clan;

      this.updatingClan = false;
      notify('Clan updated!');
      return;
    }

    notify('Clan not found!');
  }
}
</script>

<style scoped lang="scss"></style>
