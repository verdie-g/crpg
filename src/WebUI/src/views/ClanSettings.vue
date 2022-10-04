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
      <b-field label="Region">
        <b-dropdown v-model="translatedRegion" position="is-bottom-left" aria-role="menu">
          <template #trigger>
            <a class="navbar-item" role="button">
              <span>{{ translatedRegion.translation }}</span>
              <b-icon icon="caret-down"></b-icon>
            </a>
          </template>

          <b-dropdown-item v-for="t in translatedRegions" :value="t" :key="t.region">
            {{ t.translation }}
          </b-dropdown-item>
        </b-dropdown>
      </b-field>
      <b-field>
        <p class="control">
          <b-button
            size="is-medium"
            @click="submitClanUpdate"
            icon-left="check"
            :disabled="disableUpdateButton"
          >
            Update
          </b-button>
        </p>
      </b-field>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import * as clanService from '@/services/clan-service';
import Region, { TranslatedRegion } from '@/models/region';
import Clan from '@/models/clan';
import ClanUpdate from '@/models/clan-update';
import clanModule from '@/store/clan-module';
import { getTranslatedRegions } from '@/services/region-service';
import { notify } from '@/services/notifications-service';

@Component
export default class ClanSettings extends Vue {
  clan: Clan | null = null;
  translatedRegions = getTranslatedRegions();
  clanUpdate: ClanUpdate = {
    region: Region.Europe,
  };
  originalClanUpdate: ClanUpdate | null;

  get translatedRegion(): TranslatedRegion {
    const region = this.clanUpdate?.region || Region.Europe;
    return this.translatedRegions.find(tr => tr.region === region) || this.translatedRegions[1];
  }

  set translatedRegion(translatedRegion: TranslatedRegion) {
    this.clanUpdate = { ...this.clanUpdate, region: translatedRegion.region };
  }

  get disableUpdateButton(): boolean {
    return this.clanUpdate.region === this.originalClanUpdate?.region;
  }

  created() {
    const clanId = parseInt(this.$route.params.id as string);
    if (Number.isNaN(clanId)) {
      this.$router.push({ name: 'not-found' });
      return;
    }

    clanService.getClan(clanId).then(clan => {
      this.initClanUpdates(clan);
      this.clan = clan;
    });
  }

  submitClanUpdate(): void {
    clanModule
      .updateClan({
        clanId: this.clan!.id,
        clanUpdate: this.clanUpdate,
      })
      .then(clan => {
        this.clan = clan;
        this.initClanUpdates(clan);
        notify('Clan updated!');
      });
  }

  initClanUpdates(clan: Clan) {
    this.clanUpdate = { region: clan.region };
    this.originalClanUpdate = this.clanUpdate;
  }
}
</script>

<style scoped lang="scss"></style>
