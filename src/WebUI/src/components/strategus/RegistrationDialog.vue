<template>
  <div class="content is-medium">
    <h2>{{ $t('strategusRegistrationDialogWelcome') }}</h2>
    <p>{{ $t('strategusRegistrationDialogDescription') }}</p>

    <p>{{ $t('strategusRegistrationDialogSelectRegion') }}</p>
    <form @submit.prevent="onSubmit">
      <b-field>
        <b-select
          :placeholder="$t('strategusRegistrationDialogSelectRegionPlaceholder')"
          :icon="regionIcon"
          required
          v-model="selectedRegion"
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

      <b-button
        tag="input"
        native-type="submit"
        value="Start"
        size="is-medium"
        :loading="registering"
      />
    </form>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import * as strategusService from '@/services/strategus-service';
import strategusModule from '@/store/strategus-module';
import Region from '@/models/region';

@Component
export default class RegistrationDialog extends Vue {
  selectedRegion: Region | null = null;
  registering = false;

  get regions(): [string, string][] {
    return Object.entries(strategusService.regionToStr);
  }

  get regionIcon(): string {
    switch (this.selectedRegion) {
      case Region.Europe:
        return 'globe-europe';
      case Region.NorthAmerica:
        return 'globe-americas';
      case Region.Asia:
        return 'globe-asia';
      default:
        return 'globe';
    }
  }

  onSubmit() {
    this.registering = true;
    strategusModule.registerUser(this.selectedRegion!).then(() => {
      this.registering = false;
      strategusModule.popDialog();
      this.$emit('partySpawn');
    });
  }
}
</script>

<style scoped lang="scss"></style>
