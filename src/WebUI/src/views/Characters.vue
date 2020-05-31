<template>
  <div class="section">
    <div class="columns">
      <div class="column is-narrow is-paddingless" style="width: 200px;">
        <div class="list is-hoverable" v-if="characters.length">
          <a class="list-item" v-for="character in characters" v-bind:key="character.id"
             v-bind:class="{ 'is-active': selectedCharacter && character.id === selectedCharacter.id }" @click="selectCharacter(character)">
            <p class="title is-5">{{character.name}}</p>
            <p class="subtitle is-6">Level {{character.level}}</p>
          </a>
        </div>
      </div>

      <div class="column">
        <div v-if="selectedCharacter">
          <character-component :character="selectedCharacter"  />
        </div>
        <div v-else> <!-- if no character -->
          To create a character, simply connect to one of the cRPG servers.
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import Character from '@/models/character';
import CharacterComponent from '@/components/CharacterComponent.vue';

@Component({
  components: { CharacterComponent },
})
export default class CharactersComponent extends Vue {
  selectedCharacterId = -1;

  get characters(): Character[] {
    return userModule.characters;
  }

  get selectedCharacter(): Character | null {
    return this.selectedCharacterId === -1
      ? null
      : this.characters.find(c => c.id === this.selectedCharacterId)!;
  }

  created(): void {
    userModule.getCharacters().then(c => this.selectedCharacterId = c.length > 0 ? c[0].id : -1);
  }

  selectCharacter(character: Character): void {
    this.selectedCharacterId = character.id;
  }
}
</script>

<style scoped lang="scss">
</style>
