<template>
  <div class="container">
    <div class="section">
      <h2 class="title">{{ $t('administrationRestrictions') }}</h2>
      <b-table
        :data="restrictionsData"
        :columns="restrictionsColumns"
        v-if="restrictionsData.length"
      />
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
      type: r.type,
      reason: r.reason,
      restrictedByUser: `${r.restrictedByUser.name} (${r.restrictedByUser.platformUserId})`,
    }));
  }

  get restrictionsColumns() {
    return [
      {
        field: 'id',
        label: this.$t('administrationRestrictionColumnID'),
        numeric: true,
      },
      {
        field: 'restrictedUser',
        label: this.$t('administrationRestrictionColumnRestrictedUser'),
      },
      {
        field: 'createdAt',
        label: this.$t('administrationRestrictionColumnCreatedAt'),
      },
      {
        field: 'duration',
        label: this.$t('administrationRestrictionColumnDuration'),
      },
      {
        field: 'type',
        label: this.$t('administrationRestrictionColumnType'),
      },
      {
        field: 'reason',
        label: this.$t('administrationRestrictionColumnReason'),
      },
      {
        field: 'restrictedByUser',
        label: this.$t('administrationRestrictionColumnRestrictedByUser'),
      },
    ];
  }
}
</script>

<style scoped lang="scss"></style>
