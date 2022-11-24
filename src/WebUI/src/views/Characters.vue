<template>
  <div class="section">
    <div class="columns">
      <b-menu class="column is-narrow is-paddingless" style="width: 200px" v-if="characters.length">
        <b-menu-list>
          <b-menu-item
            v-for="character in characters"
            v-bind:key="character.id"
            :active="selectedCharacter && character.id === selectedCharacter.id"
            @click="selectCharacter(character)"
          >
            <template #label>
              {{ character.name }} (lvl {{ character.level }})
              <b-icon
                v-if="character.id === activeCharacterId"
                class="is-pulled-right"
                icon="check"
              ></b-icon>
            </template>
          </b-menu-item>
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
          To create a character, simply join a cRPG server.
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
import { useTimeoutPoll } from '@/utils/useTimeoutPoll';

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

  get activeCharacterId(): number | null {
    return userModule.user!.activeCharacterId;
  }

  created(): void {
    userModule.getCharacters().then(c => {
      this.selectedCharacterId = c.length > 0 ? c[0].id : -1;

      const { stop } = useTimeoutPoll(userModule.getCharacters, 1000 * 60 * 2);
      this.$once('hook:beforeDestroy', () => {
        stop();
      });
    });
  }

  selectCharacter(character: Character): void {
    this.selectedCharacterId = character.id;
  }
}
</script>

<style scoped lang="scss"></style>
