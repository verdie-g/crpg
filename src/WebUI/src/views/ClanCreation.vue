<template>
  <section class="section">
    <div class="container">
      <h1 class="is-size-2">Create a new clan</h1>
      <form @submit.prevent="onSubmit">
        <b-field label="Region">
          <b-dropdown v-model="translatedRegion" aria-role="menu">
            <template #trigger>
              <a class="navbar-item" role="button">
                <span>{{ translatedRegion.translation }}</span>
                <b-icon icon="caret-down"></b-icon>
              </a>
            </template>

            <b-dropdown-item
              v-for="translatedRegion in translatedRegions"
              :value="translatedRegion"
              :key="translatedRegion.region"
            >
              {{ translatedRegion.translation }}
            </b-dropdown-item>
          </b-dropdown>
        </b-field>
        <b-field grouped>
          <b-field label="Tag">
            <b-input
              v-model="tag"
              :minlength="tagMinLength"
              :maxlength="tagMaxLength"
              :pattern="tagRegex"
              required
            />
          </b-field>

          <b-field label="Primary Color">
            <b-input type="color" v-model="primaryColor" required style="width: 77px" />
          </b-field>

          <b-field label="Seconday Color">
            <b-input type="color" v-model="secondaryColor" required style="width: 77px" />
          </b-field>
        </b-field>

        <b-field label="Name">
          <b-input v-model="name" :minlength="nameMinLength" :maxlength="nameMaxLength" required />
        </b-field>

        <b-field>
          <template #label>
            Banner Key (generate one on
            <a href="https://bannerlord.party" target="_blank">bannerlord.party</a>
            )
          </template>
          <b-input
            v-model="bannerKey"
            :maxlength="bannerKeyMaxLength"
            :pattern="bannerKeyRegex"
            required
          />
        </b-field>

        <b-button
          type="is-primary"
          tag="input"
          native-type="submit"
          value="Create"
          :loading="creatingClan"
        />
      </form>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Constants from '../../../../data/constants.json';
import clanModule from '@/store/clan-module';
import { notify } from '@/services/notifications-service';
import { rgbHexColorToArgbInt } from '@/utils/color';
import { getTranslatedRegions } from '@/services/region-service';

@Component
export default class ClanCreationComponent extends Vue {
  creatingClan = false;

  tag = '';
  tagMinLength = Constants.clanTagMinLength;
  tagMaxLength = Constants.clanTagMaxLength;
  tagRegex = Constants.clanTagRegex;

  primaryColor = '#000000';
  secondaryColor = '#000000';

  name = '';
  nameMinLength = Constants.clanNameMinLength;
  nameMaxLength = Constants.clanNameMaxLength;

  bannerKey = '';
  bannerKeyMaxLength = Constants.clanBannerKeyMaxLength;
  bannerKeyRegex = Constants.clanBannerKeyRegex;

  translatedRegions = getTranslatedRegions();
  translatedRegion = this.translatedRegions[0];

  onSubmit() {
    this.creatingClan = true;
    clanModule
      .createClan({
        tag: this.tag,
        primaryColor: rgbHexColorToArgbInt(this.primaryColor),
        secondaryColor: rgbHexColorToArgbInt(this.secondaryColor),
        name: this.name,
        bannerKey: this.bannerKey,
        region: this.translatedRegion.region,
      })
      .then(clan => {
        notify('Clan created');
        this.$router.push({ name: 'clan', params: { id: clan.id.toString() } });
      })
      .finally(() => (this.creatingClan = false));
  }
}
</script>

<style scoped lang="scss"></style>
