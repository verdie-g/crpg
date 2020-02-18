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

    <div class="column">
      <div class="columns" v-if="selectedCharacter">

        <div class="column content character-stats">
          <h1>{{selectedCharacter.name}}</h1>
          <p>
            <strong>Level:</strong> {{selectedCharacter.level}}<br />
            <strong>Experience:</strong> {{selectedCharacter.experience}}<br />
            <strong>Next Level:</strong> 4214
          </p>

          <h2>Attributes</h2>
          <p>
            <strong>Strength:</strong> 28<br />
            <strong>Perception:</strong> 14<br />
            <strong>Endurance:</strong> 12
          </p>

          <h2>Skills</h2>
          <p>
            <strong>One Handed:</strong> 0<br />  <!-- Mastery of fighting with one-handed weapons either with a shield or without. -->
            <strong>Two Handed:</strong> 0<br /> <!-- Mastery of fighting with two-handed weapons of average length such as bigger axes and swords. -->
            <strong>Polearm:</strong> 0<br /> <!-- Mastery of the spear, lance, staff and other polearms, both one-handed and two-handed. -->
            <strong>Bow:</strong> 0<br /> <!-- Familiarity with bows and physical conditioning to shoot with them effectively. -->
            <strong>Throwing:</strong> 0<br /> <!-- Mastery of throwing projectiles accurately and with power. -->
            <strong>Crossbow:</strong> 0<br /> <!-- Knowledge of operating and maintaining crossbows. -->
            <strong>Riding:</strong> 0<br /> <!-- The ability to control a horse, to keep your balance when it moves suddenly or unexpectedly. -->
            <strong>Athletics:</strong> 0 <!-- Physical fitness, speed and balance. -->
          </p>
        </div>

        <div class="column character-items">
          <div class="columns item-boxes">
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
        </div>

      </div>

      <div v-else> <!-- if no character -->
        Explain how to create a character
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import Character from '@/models/character';

  @Component
export default class Characters extends Vue {
    selectedCharacter: Character | null = null;

    get characters() {
      return userModule.characters;
    }

    async created() {
      const characters = await userModule.getCharacters();
      this.selectedCharacter = characters.length > 0 ? characters[0] : null;
    }

    selectCharacter(character: Character) {
      this.selectedCharacter = character;
    }
}
</script>

<style scoped lang="scss">
  .character-stats {
    strong {
      display: inline-block;
      width: 200px;
    }
  }

  .item-boxes {
    background-image: url("../assets/body-silhouette.svg");
    background-repeat: no-repeat;
    background-size: 215px;
    background-position: 138px 15px;
    padding-bottom: 8px; // so the silhouette's feet ain't cropped
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
