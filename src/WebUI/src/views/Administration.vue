<template>
  <div class="container">

    <div class="section">
      <h2 class="title">Bans</h2>
      <b-table :data="bansData" :columns="bansColumns" v-if="bansData.length" />
    </div>

  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import banModule from '@/store/ban-module';
import { timestampToTimeString } from '@/utils/date';

@Component
export default class Administration extends Vue {
  created(): void {
    if (banModule.bans.length === 0) {
      banModule.getBans();
    }
  }

  get bansData() {
    return banModule.bans.map(b => ({
      id: b.id,
      bannedUser: `${b.bannedUser!.userName} (${b.bannedUser!.steamId})`,
      createdAt: b.createdAt.toDateString(),
      duration: timestampToTimeString(b.duration),
      reason: b.reason,
      bannedBy: `${b.bannedByUser.userName} (${b.bannedByUser.steamId})`,
    }));
  }

  get bansColumns() {
    return [
      {
        field: 'id',
        label: 'ID',
        numeric: true,
      },
      {
        field: 'bannedUser',
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
        field: 'bannedBy',
        label: 'By',
      },
    ];
  }
}
</script>

<style scoped lang="scss">
</style>
