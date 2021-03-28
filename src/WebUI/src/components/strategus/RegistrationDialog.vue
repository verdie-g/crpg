<template>
  <div class="content">
    <h2>Welcome to Strategus</h2>
    <p>
      Strategus is a multiplayer campaign for cRPG where players can aquire fiefs and land and
      gather armies on a real-time map. Strategus expands the world of cRPG to a browser-based map
      of Calradia, where players take their armies into battle against other players or serve as a
      mercenary in scheduled battles on the cRPG servers.
    </p>

    <p>To start playing, you must first select your region.</p>
    <form @submit.prevent="onSubmit">
      <b-field>
        <b-select
          placeholder="Select a region"
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
  registering: boolean = false;

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
    });
  }
}
</script>

<style scoped lang="scss"></style>
