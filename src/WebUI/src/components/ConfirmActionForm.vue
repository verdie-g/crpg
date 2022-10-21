<template>
  <form @submit.prevent="onSubmit">
    <div class="modal-card" style="width: auto">
      <header class="modal-card-head">
        <p class="modal-card-title">
          <slot name="title" />
        </p>
        <button type="button" class="delete" @click="$emit('close')" />
      </header>

      <section class="modal-card-body">
        <div class="media mb-4">
          <div class="media-left">
            <b-icon icon="exclamation-circle" size="is-large" type="is-danger" />
          </div>

          <div class="media-content">
            <slot name="description" />
          </div>
        </div>

        <p>Enter the following to confirm:</p>
        <p class="mb-1">
          <b>{{ confirmValue }}</b>
        </p>
        <b-field style="">
          <b-input v-model="confirmModel" required style="width: 250px" />
        </b-field>
      </section>

      <footer class="modal-card-foot is-justify-content-flex-end">
        <b-button label="Cancel" @click="$emit('close')" />
        <b-button label="Delete" native-type="submit" type="is-danger" :disabled="!isValid" />
      </footer>
    </div>
  </form>
</template>

<script lang="ts">
import { Vue, Component, Prop, Emit } from 'vue-property-decorator';

@Component
export default class ConfirmActionForm extends Vue {
  @Prop(String) readonly confirmValue: string;

  confirmModel = '';

  get isValid() {
    return this.confirmValue === this.confirmModel;
  }

  @Emit('submit')
  onSubmit() {
    return true;
  }
}
</script>

<style scoped lang="scss"></style>
