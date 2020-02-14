<template>
  <div class="columns">
    <div class="column is-narrow is-paddingless" style="width: 200px;">
      <div class="list is-hoverable">
        <a class="list-item" v-for="character in characters" v-bind:key="character.id"
           v-bind:class="{ 'is-active': character === selectedCharacter }" @click="selectCharacter(character)">
          <p class="title is-5">{{character.name}}</p>
          <p class="subtitle is-6">Level {{character.level}}</p>
        </a>
      </div>
    </div>

    <div class="column columns">

      <div class="column">
      </div>

      <div class="column is-narrow columns item-boxes">
        <div class="column is-narrow gear-column">
          <div class="box item-box"></div>
          <div class="box item-box"></div>
          <div class="box item-box"></div>
          <div class="box item-box"></div>
        </div>

        <div class="column is-narrow horse-column">
          <div class="box item-box"></div>
        </div>

        <div class="column is-narrow weapon-column">
          <div class="box item-box"></div>
          <div class="box item-box"></div>
          <div class="box item-box"></div>
          <div class="box item-box"></div>
        </div>
      </div>

      <div class="column">
      </div>

    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';

  @Component
export default class Character extends Vue {
    selectedCharacter: Character | null = null;

    get characters() {
      return userModule.characters;
    }

    created() {
      userModule.getCharacters();
    }

    selectCharacter(character: Character) {
      this.selectedCharacter = character;
    }
}
</script>

<style scoped lang="scss">
  .item-boxes {
    background-image: url("../assets/body-silhouette.svg");
    background-repeat: no-repeat;
    background-size: 215px;
    background-position: 148px 15px;
  }

  .horse-column {
    width: 250px;
    display: flex;
    justify-content: flex-end;
    flex-direction: column;
    align-items: center;
  }

  .item-box {
    width: 100px;
    height: 100px;
  }
</style>
