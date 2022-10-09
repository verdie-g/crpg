<script setup lang="ts">
const checkboxModel = ref<boolean>(true);
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

        <OButton variant="todo">
          <SvgIcon name="download" viewBox="0 0 24 24" class="w-4"></SvgIcon>
          Download cRPG
        </OButton>

        <SvgIcon name="steam" viewBox="0 0 40 40" class="w-10 text-todo"></SvgIcon>
        <SvgIcon name="discord" viewBox="0 0 46 36" class="w-10 text-todo"></SvgIcon>
        <SvgIcon name="patreon" viewBox="0 0 40 40" class="w-10 text-todo"></SvgIcon>
      </div>

      <br />

      <div class="flex gap-2">
        <ODropdown placeholder="Difficulty" aria-role="list">
          <template #trigger>
            <OButton variant="primary" type="button">
              Dropdown
              <SvgIcon name="chevron-down" viewBox="0 0 24 24" class="w-6"></SvgIcon>
            </OButton>
          </template>
          <ODropdownItem v-for="dropdownItem in dropDownList" :value="dropdownItem.value" disabled>
            <span>{{ dropdownItem.title }}</span>
          </ODropdownItem>
        </ODropdown>
      </div>

      <br />

      <div class="flex w-1/4 flex-col gap-4">
        <OCheckbox>Show owned items</OCheckbox>
        <OCheckbox v-model="checkboxModel">Show only affordable items</OCheckbox>

        <OField label="Name" horizontal>
          <OInput v-model="inputModelName" expanded />
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
              <OButton variant="primary">
                <SvgIcon name="shevron" viewBox="0 0 19 21" class="w-4"></SvgIcon>
                Respecialize
              </OButton>

              <OButton variant="secondary">
                <SvgIcon name="child" viewBox="0 0 18 24" class="w-4"></SvgIcon>
                Retire
              </OButton>

              <OButton variant="danger">
                <SvgIcon name="trash" viewBox="0 0 21 24" class="w-4"></SvgIcon>
                Delete
              </OButton>
            </div>
          </div>

          <div class="col-span-1">
            <div class="grid grid-cols-2 gap-2">
              <OButton variant="neutral">
                <SvgIcon name="reset" viewBox="0 0 24 24" class="w-4"></SvgIcon>
                Reset
              </OButton>

              <OButton variant="neutral">
                <SvgIcon name="check" viewBox="0 0 22 16" class="w-4"></SvgIcon>
                Commit
              </OButton>
            </div>
          </div>
        </div>
      </div>

      <br />

      <!--  -->
    </div>
  </div>
</template>
