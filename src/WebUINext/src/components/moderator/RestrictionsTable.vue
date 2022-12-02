<script setup lang="ts">
import { RestrictionWithActive } from '@/models/restriction';
import { usePagination } from '@/composables/use-pagination';
import { parseTimestamp, computeLeftMs } from '@/utils/date';

const props = defineProps<{ restrictions: RestrictionWithActive[]; hiddenCols?: string[] }>();

const { pageModel } = usePagination();
const perPage = 10;
</script>

<template>
  <OTable
    v-model:current-page="pageModel"
    :data="restrictions"
    :perPage="perPage"
    :paginated="restrictions.length > perPage"
    hoverable
    bordered
    :debounceSearch="300"
    sortIcon="chevron-up"
    sortIconSize="xs"
    :defaultSort="['id', 'desc']"
  >
    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="id"
      label="Id"
      :width="40"
      sortable
    >
      {{ restriction.id }}
    </OTableColumn>

    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="active"
      :label="$t('restriction.table.column.status')"
      :width="160"
      sortable
    >
      <Tag
        v-if="restriction.active"
        :label="$t('restriction.status.active')"
        variant="success"
        size="sm"
        v-tooltip="
          $t('dateTimeFormat.dd:hh:mm', {
            ...parseTimestamp(computeLeftMs(restriction.createdAt, restriction.duration)),
          })
        "
      />
      <Tag v-else variant="info" size="sm" disabled :label="$t('restriction.status.inactive')" />
    </OTableColumn>

    <OTableColumn
      v-if="!hiddenCols?.includes('restrictedUser')"
      field="restrictedUser.name"
      :label="$t('restriction.table.column.user')"
      :width="160"
      searchable
    >
      <template #searchable="props">
        <o-input
          v-model="props.filters[props.column.field]"
          :placeholder="$t('action.search')"
          icon="search"
          class="w-44"
          size="sm"
          clearable
        />
      </template>

      <template #default="{ row: restriction }: { row: RestrictionWithActive }">
        <RouterLink
          :to="{
            name: 'ModeratorUserRestrictionsId',
            params: { id: restriction.restrictedUser.id },
          }"
          class="inline-block hover:text-content-100"
        >
          <UserMedia :user="restriction.restrictedUser" />
        </RouterLink>
      </template>
    </OTableColumn>

    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="createdAt"
      :label="$t('restriction.table.column.createdAt')"
      :width="180"
      sortable
    >
      {{ $d(restriction.createdAt, 'long') }}
    </OTableColumn>

    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="duration"
      :label="$t('restriction.table.column.duration')"
      :width="160"
    >
      {{
        $t('dateTimeFormat.dd:hh:mm', {
          ...parseTimestamp(restriction.duration),
        })
      }}
    </OTableColumn>

    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="type"
      :label="$t('restriction.table.column.type')"
      :width="60"
    >
      {{ $t(`restriction.type.${restriction.type}`) }}
    </OTableColumn>

    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="reason"
      :label="$t('restriction.table.column.reason')"
    >
      <template v-if="restriction.reason.length <= 50">
        {{ restriction.reason }}
      </template>

      <OCollapse v-else :open="false" position="bottom">
        <template #trigger="props">
          <template v-if="!props.open">
            {{ restriction.reason.substring(0, 50) }}...
            <OButton
              v-tooltip="$t('action.expand')"
              variant="secondary"
              rounded
              size="2xs"
              :aria-expanded="props.open"
              iconRight="chevron-down"
            />
          </template>
          <OButton
            v-else
            v-tooltip="$t('action.collapse')"
            variant="secondary"
            size="2xs"
            rounded
            :aria-expanded="props.open"
            iconRight="chevron-up"
            class="mt-1"
          />
        </template>
        {{ restriction.reason }}
      </OCollapse>
    </OTableColumn>

    <OTableColumn
      #default="{ row: restriction }: { row: RestrictionWithActive }"
      field="restrictedByUser.name"
      :label="$t('restriction.table.column.restrictedBy')"
      :width="180"
    >
      <UserMedia :user="restriction.restrictedByUser" />
    </OTableColumn>

    <template #empty>
      <ResultNotFound />
    </template>
  </OTable>
</template>
