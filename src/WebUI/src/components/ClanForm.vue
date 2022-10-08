<template>
  <section class="section">
    <div class="container">
      <form @submit.prevent="onSubmit">
        <b-field label="Region">
          <b-dropdown @change="setRegion" aria-role="menu">
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
import Region, { TranslatedRegion } from '@/models/region';
import { ClanFormMode, ClanEditionMode } from '@/models/clan-form';
import Clan from '@/models/clan';
import { getTranslatedRegions } from '@/services/region-service';

@Component
export default class ClanFormComponent extends Vue {
  @Prop(String) readonly mode: ClanFormMode;
  @Prop(Number) readonly id?: number;
  @Prop({ type: Boolean, default: false }) readonly isLoading: boolean;

  @Prop({
    type: Object,
    default: () => ({
      region: Region.Europe,
      name: '',
      tag: '',
      primaryColor: '#000000',
      secondaryColor: '#000000',
      bannerKey: '',
    }),
  })
  readonly clan: Omit<Clan, 'id'>;

  formModes = ClanEditionMode;

  formModel: Omit<Clan, 'id'> = {
    region: Region.Europe,
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

  translatedRegions = getTranslatedRegions();

  constructor() {
    super();
    this.formModel = { ...this.clan };
  }

  get translatedRegion(): TranslatedRegion {
    return (
      this.translatedRegions.find(tr => tr.region === this.formModel.region) ||
      this.translatedRegions[0]
    );
  }

  setRegion(val: TranslatedRegion): void {
    this.formModel.region = val.region;
  }

  @Emit('submit')
  onSubmit() {
    return this.formModel;
  }
}
</script>

<style scoped lang="scss"></style>
