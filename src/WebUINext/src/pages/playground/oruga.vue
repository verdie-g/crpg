<script setup lang="ts">
const checkboxModel = ref<boolean>(true);
const radioModel = ref<string>('Jack');
const switchModel = ref<boolean>(false);

const inputModelName = ref<string>('RainbowDash');
const inputModelPriceFrom = ref<number>(0);
const inputModelPriceTo = ref<number>(0);

const inputModelIronFlesh = ref<number>(4);

const dropDownList = ref([
  {
    value: 'easy',
    title: 'Easy',
  },
  {
    value: 'medium',
    title: 'Medium',
  },
  {
    value: 'hard',
    title: 'Hard',
  },
]);

const currentTablePage = ref<number>(1);

const tableData = ref([
  {
    id: 1,
    first_name: 'Jesse',
    last_name: 'Simmons',
    date: '2016-10-15 13:43:27',
    gender: 'Male',
  },
  {
    id: 2,
    first_name: 'John',
    last_name: 'Jacobs',
    date: '2016-12-15 06:00:53',
    gender: 'Male',
  },
  {
    id: 3,
    first_name: 'Tina',
    last_name: 'Gilbert',
    date: '2016-04-26 06:26:28',
    gender: 'Female',
  },
  {
    id: 4,
    first_name: 'Tina',
    last_name: 'Gilbert',
    date: '2016-04-26 06:26:28',
    gender: 'Female',
  },
]);

const tableColumns = ref([
  {
    field: 'id',
    label: 'ID',
    width: '60',
    numeric: true,
  },
  {
    field: 'first_name',
    label: 'First Name',
  },
  {
    field: 'last_name',
    label: 'Last Name',
  },
  {
    field: 'date',
    label: 'Date',
    position: 'centered',
  },
  {
    field: 'gender',
    label: 'Gender',
  },
]);

const activeTab = ref('0');
</script>

<route lang="yaml">
meta:
  layout: playground
</route>

<template>
  <div>
    <div class="bg-todo3 p-8">
      <h1>Oruga playground - Favoras theme</h1>

      <br />

      <div class="flex gap-2">
        <OButton variant="primary">primary</OButton>
        <OButton variant="secondary">secondary</OButton>
        <OButton variant="danger">danger</OButton>
        <OButton variant="neutral">neutral</OButton>
        <OButton variant="todo">todo</OButton>
        <OButton variant="todo2">todo2</OButton>

        <OButton variant="todo" icon-left="download">Download cRPG</OButton>

        <OIcon class="text-todo" icon="steam" size="large" />
        <OIcon class="text-todo" icon="discord" size="large" />
        <OIcon class="text-todo" icon="patreon" size="large" />
      </div>

      <br />

      <div class="flex gap-2">
        <ODropdown placeholder="Difficulty" aria-role="list">
          <template #trigger>
            <OButton variant="primary" type="button" icon-right="chevron-down">Dropdown</OButton>
          </template>
          <ODropdownItem v-for="dropdownItem in dropDownList" :value="dropdownItem.value" disabled>
            <span>{{ dropdownItem.title }}</span>
          </ODropdownItem>
        </ODropdown>
      </div>

      <br />

      <div class="flex w-1/4 flex-col gap-4">
        <OField>
          <OCheckbox>Show owned items</OCheckbox>
        </OField>

        <OField>
          <OCheckbox v-model="checkboxModel">Show only affordable items</OCheckbox>
        </OField>

        <OField>
          <ORadio v-model="radioModel" native-value="Flint">Radio example</ORadio>
          <ORadio v-model="radioModel" native-value="Jack">Radio example</ORadio>
        </OField>

        <OField>
          <OSwitch v-model="switchModel">Switch example</OSwitch>
        </OField>

        <OField label="Name" horizontal>
          <OInput v-model="inputModelName" expanded variant="info" />
        </OField>

        <OField label="Price" horizontal>
          <OInput v-model="inputModelPriceFrom" type="number" expanded />
          <OInput v-model="inputModelPriceTo" type="number" expanded />
        </OField>

        <div class="flex gap-x-8 gap-y-2">
          <div>Iron Flesh</div>
          <!-- TODO: to plus-minus-input component :)  -->
          <OField grouped>
            <OButton variant="neutral">-</OButton>
            <OInput v-model="inputModelIronFlesh" readonly variant="counter" />
            <OButton variant="neutral">+</OButton>
          </OField>
        </div>
      </div>

      <br />

      <div class="container">
        <div class="grid grid-cols-4 gap-8">
          <div class="col-span-3">
            <div class="grid grid-cols-3 gap-8">
              <OButton variant="primary" icon-left="chevron-down-double">Respecialize</OButton>
              <OButton variant="secondary" icon-left="child">Retire</OButton>
              <OButton variant="danger" icon-left="trash">Delete</OButton>
            </div>
          </div>

          <div class="col-span-1">
            <div class="grid grid-cols-2 gap-2">
              <OButton variant="neutral" icon-left="reset">Reset</OButton>
              <OButton variant="neutral" icon-left="check-2">Commit</OButton>
            </div>
          </div>
        </div>

        <br />

        <div class="">
          <!-- loading -->
          <OTable
            :data="tableData"
            v-model:current-page="currentTablePage"
            paginated
            :per-page="2"
            default-sort="id"
            default-sort-direction="asc"
            pagination-position="bottom"
            aria-next-label="Next page"
            aria-previous-label="Previous page"
            aria-page-label="Page"
            aria-current-label="Current page"
          >
            <OTableColumn
              v-for="column in tableColumns"
              v-bind="column"
              sortable
              #default="{ row }"
            >
              {{ row[column.field] }}
            </OTableColumn>
          </OTable>
        </div>
      </div>

      <br />

      <div class="bg-white p-4">
        <OTabs v-model="activeTab" type="boxed">
          <OTabItem value="0" label="Pictures">Lorem ipsum dolor sit amet.</OTabItem>
          <OTabItem value="1" label="Music">
            Lorem
            <br />
            ipsum
            <br />
            dolor
            <br />
            sit
            <br />
            amet.
          </OTabItem>
        </OTabs>
      </div>

      <br />
      <!--  -->
    </div>
  </div>
</template>
