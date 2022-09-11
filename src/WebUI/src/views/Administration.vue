<template>
  <div class="container">
    <div class="section">
      <h2 class="title">Restrictions</h2>
      <b-table
        :data="restrictionsData"
        :columns="restrictionsColumns"
        v-if="restrictionsData.length" />
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import restrictionModule from '@/store/restriction-module';
import { timestampToTimeString } from '@/utils/date';

@Component
export default class Administration extends Vue {
  created(): void {
    if (restrictionModule.restrictions.length === 0) {
      restrictionModule.getRestrictions();
    }
  }

  get restrictionsData() {
    return restrictionModule.restrictions.map(r => ({
      id: r.id,
      restrictedUser: `${r.restrictedUser!.name} (${r.restrictedUser!.platformUserId})`,
      createdAt: r.createdAt.toDateString(),
      duration: timestampToTimeString(r.duration),
      reason: r.reason,
      restrictedByUser: `${r.restrictedByUser.name} (${r.restrictedByUser.platformUserId})`,
    }));
  }

  get restrictionsColumns() {
    return [
      {
        field: 'id',
        label: 'ID',
        numeric: true,
      },
      {
        field: 'restrictedUser',
        label: 'User',
      },
      {
        field: 'createdAt',
        label: 'Created At',
      },
      {
        field: 'duration',
        label: 'Duration',
      },
      {
        field: 'reason',
        label: 'Reason',
      },
      {
        field: 'restrictedByUser',
        label: 'By',
      },
    ];
  }
}
</script>

<style scoped lang="scss"></style>
