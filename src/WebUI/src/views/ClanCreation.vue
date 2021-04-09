<template>
  <section class="section">
    <div class="container">
      <h1 class="is-size-2">Create a new clan</h1>
      <form @submit.prevent="onSubmit">
        <b-field label="Tag">
          <b-input
            v-model="tag"
            :minlength="tagMinLength"
            :maxlength="tagMaxLength"
            :pattern="tagRegex"
            required
          />
        </b-field>

        <b-field label="Name">
          <b-input v-model="name" :minlength="nameMinLength" :maxlength="nameMaxLength" required />
        </b-field>

        <b-button
          type="is-primary"
          tag="input"
          native-type="submit"
          value="Create"
          :loading="creatingClan"
        />
      </form>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Constants from '../../../../data/constants.json';
import clanModule from '@/store/clan-module';
import { notify } from '@/services/notifications-service';

@Component
export default class ClanCreationComponent extends Vue {
  creatingClan = false;

  tag = '';
  tagMinLength = Constants.clanTagMinLength;
  tagMaxLength = Constants.clanTagMaxLength;
  tagRegex = Constants.clanTagRegex;

  name = '';
  nameMinLength = Constants.clanNameMinLength;
  nameMaxLength = Constants.clanNameMaxLength;

  onSubmit() {
    this.creatingClan = true;
    clanModule
      .createClan({ tag: this.tag, name: this.name })
      .then(clan => {
        notify('Clan created');
        this.$router.push({ name: 'clan', params: { id: clan.id.toString() } });
      })
      .finally(() => (this.creatingClan = false));
  }
}
</script>

<style scoped lang="scss"></style>
