<template>
  <section class="section">
    <div class="container">
      <b-table :data="clans" :hoverable="true" :loading="clansLoading">
        <b-table-column field="tag" label="Tag" v-slot="props">
          {{ props.row.tag }}
        </b-table-column>

        <b-table-column field="name" label="Name" v-slot="props">
          {{ props.row.name }}
        </b-table-column>
      </b-table>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import clanModule from '@/store/clan-module';
import ClanLite from '@/models/clan-lite';

@Component({
  components: {},
})
export default class Clans extends Vue {
  clansLoading = false;

  get clans(): ClanLite[] {
    return clanModule.clans;
  }

  created(): void {
    clanModule.getClans();
  }
}
</script>

<style scoped lang="scss"></style>
