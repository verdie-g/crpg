<template>
  <b-table
    :data="data"
    striped
    hoverable
    paginated
    :per-page="10"
    default-sort="id"
    default-sort-direction="desc"
    sort-icon-size="is-small"
  >
    <b-table-column
      v-if="!hiddenCols.includes('id')"
      field="id"
      label="ID"
      sortable
      width="60"
      v-slot="props"
    >
      {{ props.row.id }}
    </b-table-column>

    <b-table-column label="Status" width="80" v-slot="props">
      <b-tag
        v-if="checkIsDateExpired(props.row.createdAt, props.row.duration)"
        rounded
        type="is-primary is-light"
      >
        expired
      </b-tag>

      <template v-else>
        <b-tooltip
          :label="`
              ${timestampToTimeString(
                computeLeftMs(props.row.createdAt, props.row.duration)
              )} left `"
          type="is-primary is-light"
        >
          <b-tag rounded type="is-success">active</b-tag>
        </b-tooltip>
      </template>
    </b-table-column>

    <b-table-column
      v-if="!hiddenCols.includes('restrictedUser')"
      field="restrictedUser"
      label="User"
      searchable
      :custom-search="searchByName('restrictedUser')"
      sortable
      :custom-sort="sortByName('restrictedUser')"
      width="200"
    >
      <template #searchable="props">
        <b-input placeholder="Search" v-model="props.filters['restrictedUser']" size="is-small" />
      </template>

      <template #default="props">
        <router-link
          :to="{
            name: 'admin-user-restrictions',
            params: {
              id: props.row.restrictedUser.id,
            },
          }"
        >
          {{ props.row.restrictedUser.name }}
        </router-link>
      </template>
    </b-table-column>

    <b-table-column field="createdAt" label="Created At" sortable width="180" v-slot="props">
      {{ new Date(props.row.createdAt).toDateString() }}
    </b-table-column>

    <b-table-column field="duration" label="Duration" width="120" v-slot="props">
      {{ timestampToTimeString(props.row.duration) }}
    </b-table-column>

    <b-table-column field="type" label="Type" sortable width="80" v-slot="props">
      {{ props.row.type }}
    </b-table-column>

    <b-table-column field="reason" label="Reason" v-slot="colProps">
      <template v-if="colProps.row.reason.length <= 50">
        {{ colProps.row.reason }}
      </template>

      <b-collapse v-else :open="false" position="is-bottom">
        <template #trigger="props">
          <template v-if="!props.open">
            {{ colProps.row.reason.substring(0, 50) }}...
            <a :aria-expanded="props.open">Expand</a>
          </template>
          <template v-else>
            <a :aria-expanded="props.open">Collapse</a>
          </template>
        </template>
        {{ colProps.row.reason }}
      </b-collapse>
    </b-table-column>

    <b-table-column
      v-if="!hiddenCols.includes('restrictedByUser')"
      field=""
      label="By"
      sortable
      :custom-sort="sortByName('restrictedByUser')"
      width="200"
      v-slot="props"
    >
      {{ props.row.restrictedByUser.name }}
    </b-table-column>

    <template #empty>
      <div class="has-text-centered">No records</div>
    </template>
  </b-table>
</template>

<script lang="ts">
import { Vue, Component, Prop } from 'vue-property-decorator';
import Restriction from '@/models/restriction';
import { timestampToTimeString, computeLeftMs, checkIsDateExpired } from '@/utils/date';

type WithName = Record<string, { name: string }>;

@Component
export default class RestrictionsTableComponent extends Vue {
  @Prop({ type: Array, default: () => [] }) readonly data: Restriction[];
  @Prop({ type: Array, default: () => [] }) readonly hiddenCols: Array<string>;

  timestampToTimeString = timestampToTimeString;
  computeLeftMs = computeLeftMs;
  checkIsDateExpired = checkIsDateExpired;

  searchByName(fieldName: string) {
    return <T extends WithName>(row: T, input: string) =>
      row[fieldName].name.toLowerCase().includes(input.toLowerCase());
  }

  sortByName(fieldName: string) {
    return <T extends WithName>(rowA: T, rowB: T, isAsc: boolean) =>
      isAsc
        ? rowA[fieldName].name.localeCompare(rowB[fieldName].name)
        : rowB[fieldName].name.localeCompare(rowA[fieldName].name);
  }
}
</script>

<style scoped lang="scss"></style>
