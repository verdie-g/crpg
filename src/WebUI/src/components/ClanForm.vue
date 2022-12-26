<template>
  <section class="section">
    <div class="container">
      <form @submit.prevent="onSubmit">
        <b-field label="Region">
          <b-select
            placeholder="Select a region"
            :icon="selectedRegionIcon"
            required
            v-model="formModel.region"
          >
            <option
              v-for="[regionValue, regionStr] in regions"
              :value="regionValue"
              :key="regionValue"
            >
              {{ regionStr }}
            </option>
          </b-select>
        </b-field>

        <b-field grouped>
          <b-field label="Tag">
            <b-input
              v-model="formModel.tag"
              :minlength="tagMinLength"
              :maxlength="tagMaxLength"
              :pattern="tagRegex"
              required
            />
          </b-field>

          <b-field label="Primary Color">
            <b-input type="color" v-model="formModel.primaryColor" required style="width: 77px" />
          </b-field>

          <b-field label="Seconday Color">
            <b-input type="color" v-model="formModel.secondaryColor" required style="width: 77px" />
          </b-field>
        </b-field>

        <b-field label="Name">
          <b-input
            v-model="formModel.name"
            :minlength="nameMinLength"
            :maxlength="nameMaxLength"
            required
          />
        </b-field>

        <b-field>
          <template #label>
            Banner Key (generate one on
            <a href="https://bannerlord.party" target="_blank">bannerlord.party</a>
            )
          </template>
          <b-input
            v-model="formModel.bannerKey"
            :maxlength="bannerKeyMaxLength"
            :pattern="bannerKeyRegex"
            required
          />
        </b-field>

        <b-button
          type="is-primary"
          tag="input"
          native-type="submit"
          :value="mode === formModes.Create ? 'Create' : 'Save'"
          :loading="isLoading"
        />
      </form>
    </div>
  </section>
</template>

<script lang="ts">
import { Vue, Component, Prop, Emit } from 'vue-property-decorator';
import Constants from '../../../../data/constants.json';
import Region from '@/models/region';
import { ClanEditionModes, ClanEditionMode } from '@/models/clan-edition';
import Clan from '@/models/clan';
import { regionIcons, regionToStr } from '@/services/region-service';

@Component
export default class ClanFormComponent extends Vue {
  @Prop(String) readonly mode: ClanEditionMode;
  @Prop(Number) readonly id?: number;
  @Prop({ type: Boolean, default: false }) readonly isLoading: boolean;

  @Prop({
    type: Object,
    default: () => ({
      region: Region.Eu,
      name: '',
      tag: '',
      primaryColor: '#000000',
      secondaryColor: '#000000',
      bannerKey: '',
    }),
  })
  readonly clan: Omit<Clan, 'id'>;

  formModes = ClanEditionModes;

  formModel: Omit<Clan, 'id'> = {
    region: Region.Eu,
    name: '',
    tag: '',
    primaryColor: '',
    secondaryColor: '',
    bannerKey: '',
  };

  tagMinLength: number = Constants.clanTagMinLength;
  tagMaxLength: number = Constants.clanTagMaxLength;
  tagRegex: string = Constants.clanTagRegex;

  nameMinLength: number = Constants.clanNameMinLength;
  nameMaxLength: number = Constants.clanNameMaxLength;

  bannerKeyMaxLength: number = Constants.clanBannerKeyMaxLength;
  bannerKeyRegex: string = Constants.clanBannerKeyRegex;

  constructor() {
    super();
    this.formModel = { ...this.clan };
  }

  get regions(): [string, string][] {
    return Object.entries(regionToStr);
  }

  get selectedRegionIcon(): string {
    return regionIcons[this.formModel.region];
  }

  @Emit('submit')
  onSubmit() {
    return this.formModel;
  }
}
</script>

<style scoped lang="scss"></style>
