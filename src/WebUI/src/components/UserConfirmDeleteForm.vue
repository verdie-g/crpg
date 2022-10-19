<template>
  <form @submit.prevent="onSubmit">
    <div class="modal-card" style="width: auto">
      <header class="modal-card-head">
        <p class="modal-card-title">Deleting account</p>
        <button type="button" class="delete" @click="$emit('close')" />
      </header>

      <section class="modal-card-body">
        <div class="media mb-4">
          <div class="media-left">
            <b-icon icon="exclamation-circle" size="is-large" type="is-danger" />
          </div>

          <div class="media-content">
            <p>
              Are you sure you want to delete your account?
              <br />
              This action cannot be undone.
            </p>
          </div>
        </div>

        <p>Enter the following to confirm:</p>
        <p class="mb-1">
          <b>{{ userName }}</b>
        </p>
        <b-field style="">
          <b-input v-model="userNameModel" required style="width: 250px" />
        </b-field>
      </section>

      <footer class="modal-card-foot is-justify-content-flex-end">
        <b-button label="Cancel" @click="$emit('close')" />
        <b-button
          label="Delete acount"
          native-type="submit"
          type="is-danger"
          :disabled="!isValid"
        />
      </footer>
    </div>
  </form>
</template>

<script lang="ts">
import { Vue, Component, Prop, Emit } from 'vue-property-decorator';

@Component
export default class ClanFormComponent extends Vue {
  @Prop(String) readonly userName: string;

  userNameModel: string = '';

  get isValid() {
    return this.userName === this.userNameModel;
  }

  @Emit('submit')
  onSubmit() {
    return true;
  }
}
</script>

<style scoped lang="scss"></style>
