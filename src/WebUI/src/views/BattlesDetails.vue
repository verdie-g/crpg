<template>
  <section class="section">
    <router-link class="is-flex is-align-items-center mb-3" :to="{ name: 'battles' }">
      <i class="fas fa-chevron-left mr-1"></i>
      Back
    </router-link>
    <router-link
      v-if="battle"
      class="is-flex is-align-items-center ml-6 my-3"
      :to="{
        name: 'strategus',
        params: {
          lat: battle.position.coordinates[1],
          lng: battle.position.coordinates[0],
        },
      }"
    >
      View on map
      <i class="far fa-eye ml-2"></i>
    </router-link>
    <div class="columns">
      <div class="column mx-6">
        <article class="tile is-child box is-flex is-align-items-center is-flex-direction-column">
          <p class="is-size-4">Attacker(s)</p>
          <p
            v-for="fighterAttacker in getFightersAttackers"
            :key="fighterAttacker.id"
            class="subtitle is-5"
          >
            <span v-if="fighterAttacker.side === sideModel.Attacker">
              {{ fighterAttacker.hero.name }}
            </span>
          </p>
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
          <p class="is-size-4">Defender(s)</p>
          <p
            v-for="fighterDefender in getFightersDefenders"
            :key="fighterDefender.id"
            class="subtitle is-5"
          >
            <span v-if="fighterDefender.side === sideModel.Defender && fighterDefender.hero">
              {{ fighterDefender.hero.name }}
            </span>
            <span v-if="fighterDefender.side === sideModel.Defender && fighterDefender.settlement">
              {{ fighterDefender.settlement.name }}
            </span>
          </p>
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
import BattleSide from '@/models/battle-side';
import Mercenaries from '@/models/mercenaries';
import Fighters from '@/models/fighters';

@Component
export default class BattlesDetails extends Vue {
  selectedAttacker: Character | null = null;
  selectedDefenser: Character | null = null;
  sideModel = BattleSide;

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

  get getFightersAttackers(): Fighters[] {
    return this.fighters.filter(figther => figther.hero && figther.side === BattleSide.Attacker);
  }

  get getFightersDefenders(): Fighters[] {
    return this.fighters.filter(
      figther =>
        (figther.hero && figther.side === BattleSide.Defender) ||
        (figther.settlement && figther.side === BattleSide.Defender)
    );
  }

  async created() {
    await strategusModule.getUpdate();
    await strategusModule.getBattle(Number(this.$route.params.id));
    await strategusModule.getFighters(Number(this.$route.params.id));
    if (this.haveCharacterInBattle()) {
      await strategusModule.getMercenaries(Number(this.$route.params.id));
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

  applyToBattleAsMercenary(battleId: number, characterId: number, side: BattleSide) {
    strategusModule.applyToBattleAsMercenary({ battleId, characterId, side });
  }
}
</script>

<style scoped lang="scss"></style>
