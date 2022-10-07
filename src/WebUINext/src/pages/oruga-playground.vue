<script setup lang="ts"></script>

<route lang="yaml">
meta:
  layout: playground
</route>

<template>
  <div>
    <h1>Oruga playground</h1>

    <div class="flex gap-2">
      <OButton size="large" variant="primary" outlined>large primary outlined</OButton>
      <OButton variant="primary">primary</OButton>
    </div>

    <div class="flex gap-2">
      <ODropdown placeholder="Difficulty" aria-role="list">
        <template #trigger>
          <OButton variant="primary" type="button" class="w-32">
            <div class="flex justify-between">
              <span>dqdq</span>
              <o-icon icon="caret-down"></o-icon>
            </div>
          </OButton>
        </template>

        <ODropdownItem value="easy">
          <span>Easy</span>
        </ODropdownItem>
        <ODropdownItem value="medium">
          <span>Medium</span>
        </ODropdownItem>
        <ODropdownItem value="hard">
          <span>Hard</span>
        </ODropdownItem>
      </ODropdown>
    </div>
  </div>
</template>
