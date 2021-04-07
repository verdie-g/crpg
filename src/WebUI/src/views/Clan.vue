<template>
  <section class="section">
    <div class="container" v-if="clan !== null">
      <h1 class="is-size-2">[{{ clan.tag }}] {{ clan.name }}</h1>
      <b-table :data="clan.members" :hoverable="true">
        <b-table-column field="name" label="Name" v-slot="props">
          {{ props.row.user.name }}
          <platform
            :platform="props.row.user.platform"
            :platformUserId="props.row.user.platformUserId"
          />
        </b-table-column>

        <b-table-column field="role" label="Role" v-slot="props">
          {{ props.row.role }}
        </b-table-column>
      </b-table>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import * as clanService from '@/services/clan-service';
import ClanWithMembers from '@/models/clan-with-members';
import PlatformComponent from '@/components/Platform.vue';

@Component({ components: { platform: PlatformComponent } })
export default class ClanComponent extends Vue {
  clan: ClanWithMembers | null = null;

  async created() {
    this.clan = await clanService.getClan(parseInt(this.$route.params.id as string));
  }
}
</script>

<style scoped lang="scss"></style>
