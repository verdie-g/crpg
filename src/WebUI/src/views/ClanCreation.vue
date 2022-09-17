<template>
  <section class="section">
    <div class="container">
      <h1 class="is-size-2">{{ $t('clanCreationCreateNewClan') }}</h1>
      <form @submit.prevent="onSubmit">
        <b-field grouped>
          <b-field :label="$t('clanCreationTag')">
            <b-input
              v-model="tag"
              :minlength="tagMinLength"
              :maxlength="tagMaxLength"
              :pattern="tagRegex"
              required
            />
          </b-field>

          <b-field :label="$t('clanCreationPrimaryColor')">
            <b-input type="color" v-model="primaryColor" required style="width: 77px" />
          </b-field>

          <b-field :label="$t('clanCreationSecondaryColor')">
            <b-input type="color" v-model="secondaryColor" required style="width: 77px" />
          </b-field>
        </b-field>

        <b-field :label="$t('clanCreationName')">
          <b-input v-model="name" :minlength="nameMinLength" :maxlength="nameMaxLength" required />
        </b-field>

        <b-field>
          <template #label>
            <div v-html="$t('clanCreationGenerateDescription')" />
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
          :value="$t('clanCreationCreate')"
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
import { hexColorToArgbInt } from '@/utils/color';

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

  onSubmit() {
    this.creatingClan = true;
    clanModule
      .createClan({
        tag: this.tag,
        primaryColor: hexColorToArgbInt(this.primaryColor),
        secondaryColor: hexColorToArgbInt(this.secondaryColor),
        name: this.name,
        bannerKey: this.bannerKey,
      })
      .then(clan => {
        notify(this.$t('clanCreationCreated').toString());
        this.$router.push({ name: 'clan', params: { id: clan.id.toString() } });
      })
      .finally(() => (this.creatingClan = false));
  }
}
</script>

<style scoped lang="scss"></style>
