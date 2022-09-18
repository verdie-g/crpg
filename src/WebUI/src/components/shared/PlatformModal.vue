<template>
  <b-modal v-if="isModalActive" :active.sync="isModalActive" scroll="keep">
    <div class="card">
      <div v-if="icon || title" class="card-header">
        <div class="card-header-title is-flex is-align-items-center">
          <div class="is-flex-grow-0">
            <b-icon v-if="icon" :icon="icon" size="is-medium" class="mr-1" />
          </div>
          <div class="is-flex-grow-1">
            <span class="is-size-4">
              {{ title }}
            </span>
          </div>
          <div class="is-flex-grow-0">
            <b-icon
              icon="times"
              size="is-medium"
              class="is-clickable p-1"
              @click.native="isModalActive = false"
            />
          </div>
        </div>
      </div>
      <div class="card-content">
        <slot name="default" />
      </div>
    </div>
  </b-modal>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class PlatformModal extends Vue {
  @Prop(Boolean) readonly value: boolean;
  @Prop(String) readonly title: string;
  @Prop(String) readonly icon: string;

  get isModalActive(): boolean {
    return this.value;
  }

  set isModalActive(val: boolean) {
    this.$emit('input', val);
  }
}
</script>

<style scoped lang="scss"></style>
