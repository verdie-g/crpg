<template>
  <div class="container">
    <div class="section">
      <h2 class="title">Restrictions</h2>
      <div class="is-flex is-justify-content-end mb-3">
        <b-button
          type="is-primary"
          icon-left="ban"
          @click="isAddRestrictionModalActive = true"
        >
          Restrict User
        </b-button>
      </div>
      <b-table
        :data="restrictionsData"
        :columns="restrictionsColumns"
        v-if="restrictionsData.length"
      />
    </div>
    <restrict-user-modal
      v-if="isAddRestrictionModalActive"
      v-model="isAddRestrictionModalActive"
      @created="fetchRestrictions"
    />
  </div>
</template>

<script lang="ts">
import RestrictUserModal from '@/components/admin/restrictions/RestrictUserModal.vue'
import { Component, Vue } from 'vue-property-decorator';
import restrictionModule from '@/store/restriction-module';
import { timestampToTimeString } from '@/utils/date';

@Component({
  components: {
    RestrictUserModal
  },
})
export default class Administration extends Vue {
  isAddRestrictionModalActive = false;

  created(): void {
    if (restrictionModule.restrictions.length === 0) {
      this.fetchRestrictions();
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
        field: 'type',
        label: 'Type',
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

  fetchRestrictions () {
    restrictionModule.getRestrictions();
  }
}
</script>

<style scoped lang="scss"></style>
