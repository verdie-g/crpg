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
            <strong>Agility:</strong> 14
          </p>

          <h2>Weapon</h2>
          <p>
            <strong>One Handed:</strong> 0<br />
            <strong>Two Handed:</strong> 0<br />
            <strong>Polearm:</strong> 0<br />
            <strong>Archery:</strong> 0<br />
            <strong>Crossbow:</strong> 0<br />
            <strong>Throwing:</strong> 0
          </p>

          <h2>Skills</h2>
          <p>
            <strong>Ironflesh:</strong> 0<br />
            <strong>Power Strike:</strong> 0<br />
            <strong>Shield:</strong> 0<br />
            <strong>Athletics:</strong> 0<br />
            <strong>Riding:</strong> 0<br />
            <strong>Horse Archery:</strong> 0<br />
            <strong>Power Draw:</strong> 0<br />
            <strong>Weapon Master:</strong> 0
          </p>
        </div>

        <div class="column character-equipments">
          <div class="columns item-boxes">
            <div class="column is-narrow gear-column">
              <div class="box equipment-box"></div>
              <div class="box equipment-box"></div>
              <div class="box equipment-box"></div>
              <div class="box equipment-box"></div>
            </div>

            <div class="column is-narrow horse-column">
              <div class="box equipment-box"></div>
            </div>

            <div class="column is-narrow weapon-column">
              <div class="box equipment-box"></div>
              <div class="box equipment-box"></div>
              <div class="box equipment-box"></div>
              <div class="box equipment-box"></div>
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
  import {Component, Vue} from 'vue-property-decorator';
  import userModule from '@/store/user-module';

  @Component
  export default class Character extends Vue {
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

  .equipment-box {
    width: 100px;
    height: 100px;
  }
</style>
