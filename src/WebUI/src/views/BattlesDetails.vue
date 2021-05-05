<template>
  <section class="section">
    <router-link class="is-flex is-align-items-center mb-3" :to="{ name: 'battles' }">
      <i class="fas fa-chevron-left mr-1"></i>
      Back
    </router-link>
    <div class="columns">
      <div class="column mx-6">
        <article class="tile is-child box is-flex is-align-items-center is-flex-direction-column">
          <p class="subtitle">Attackers</p>
          <b-field v-if="!haveCharacterInBattle()">
            <b-select
              v-model="selectedAttacker"
              placeholder="Select character"
              size="is-small"
              expanded
            >
              <option
                v-for="characterAttacker in characters"
                :key="characterAttacker.id"
                :value="characterAttacker"
              >
                {{ characterAttacker.name }} ({{ characterAttacker.level }})
              </option>
            </b-select>
            <b-button
              class="ml-2"
              :disabled="selectedAttacker === null"
              size="is-small"
              @click="applyToBattleAsMercenary($route.params.id, selectedAttacker.id, 'Attacker')"
            >
              Join as mercenary
            </b-button>
          </b-field>
        </article>
      </div>
      <div class="column mx-6">
        <article class="tile is-child box is-flex is-align-items-center is-flex-direction-column">
          <p class="subtitle">Defenders</p>
          <b-field v-if="!haveCharacterInBattle()">
            <b-select
              v-model="selectedDefenser"
              placeholder="Select character"
              size="is-small"
              expanded
            >
              <option
                v-for="characterDefender in characters"
                :key="characterDefender.id"
                :value="characterDefender"
              >
                {{ characterDefender.name }} ({{ characterDefender.level }})
              </option>
            </b-select>
            <b-button
              class="ml-2"
              size="is-small"
              :disabled="selectedDefenser === null"
              @click="applyToBattleAsMercenary($route.params.id, selectedDefenser.id, 'Defender')"
            >
              Join as mercenary
            </b-button>
          </b-field>
        </article>
      </div>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import strategusModule from '@/store/strategus-module';
import userModule from '@/store/user-module';
import Battle from '@/models/battle-detailed';
import Character from '@/models/character';
import Side from '@/models/side';
import Mercenaries from '@/models/mercenaries';
import Fighters from '@/models/fighters';

@Component
export default class BattlesDetails extends Vue {
  selectedAttacker: Character | null = null;
  selectedDefenser: Character | null = null;

  get battle(): Battle | null {
    return strategusModule.battle;
  }

  get characters(): Character[] {
    return userModule.characters;
  }

  get fighters(): Fighters[] {
    return strategusModule.fighters;
  }

  get mercenaries(): Mercenaries[] {
    return strategusModule.mercenaries;
  }

  async created() {
    await strategusModule.getUpdate();
    await strategusModule.getBattle(this.$route.params.id);
    await strategusModule.getFighters(this.$route.params.id);
    if (this.haveCharacterInBattle()) {
      await strategusModule.getMercenaries(this.$route.params.id);
    }
    await userModule.getCharacters();
  }

  haveCharacterInBattle(): boolean {
    for (const character of this.characters) {
      if (this.fighters.find(x => x.id === character.id)) {
        return true;
      }
    }
    return false;
  }

  applyToBattleAsMercenary(battleId: number, characterId: number, side: Side) {
    strategusModule.applyToBattleAsMercenary({ battleId, characterId, side });
  }
}
</script>

<style scoped lang="scss"></style>
