<template>
  <div class="container">
    <div class="section">
      <div class="mb-5">
        <h1 class="title">Restrictions</h1>
        <RestrictionsTable :data="restrictions" />
      </div>

      <div>
        <h2 class="title">Find User</h2>
        <SearchUserComponent />
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Restriction from '@/models/restriction';
import * as restrictionService from '@/services/restriction-service';
import RestrictionsTable from '@/components/RestrictionsTable';
import PlatformComponent from '@/components/Platform';
import SearchUserComponent from '@/components/SearchUserComponent';

@Component({
  components: { RestrictionsTable, Platform: PlatformComponent, SearchUserComponent },
})
export default class Administration extends Vue {
  restrictions: Restriction[] = [];

  async created() {
    this.restrictions = await restrictionService.getRestrictions();
  }
}
</script>

<style scoped lang="scss"></style>
