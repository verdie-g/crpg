<template>
  <div class="section">
    <div class="columns">
      <b-menu class="column is-narrow is-paddingless" style="width: 200px" v-if="characters.length">
        <b-menu-list>
          <b-menu-item
            v-for="character in characters"
            v-bind:key="character.id"
            :active="selectedCharacter && character.id === selectedCharacter.id"
            :label="`${character.name} (lvl ${character.level})`"
            @click="selectCharacter(character)"
          />
        </b-menu-list>
      </b-menu>

      <div class="column">
        <div v-if="this.characters.length !== 0">
          <template v-for="character in characters">
            <character-component
              v-if="character === selectedCharacter"
              :key="character.id"
              :character="selectedCharacter"
            />
          </template>
        </div>
        <div v-else>
          <!-- if no character -->
          To create a character, simply launch a
          <a
            href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2208?tab=files"
            target="_blank"
          >
            DefendTheVirgin
          </a>
          game.
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
    userModule.getCharacters().then(c => (this.selectedCharacterId = c.length > 0 ? c[0].id : -1));
  }

  selectCharacter(character: Character): void {
    this.selectedCharacterId = character.id;
  }
}
</script>

<style scoped lang="scss"></style>
