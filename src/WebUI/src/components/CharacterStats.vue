<template>
  <div>

    <div class="stats-section">
      <div class="character-name">
        <h1 class="title is-3">{{character.name}}</h1>
        <b-icon icon="pencil-alt" class="character-name-edit" @click.native="openEditCharacterDialog" />
      </div>

      <b-field horizontal label="Level" class="stat-field is-marginless">
          <b-numberinput size="is-small" :editable="false" controls-position="compact"
                         :value="character.level" :controls="false" />
      </b-field>
      <b-field horizontal label="Experience" class="stat-field is-marginless">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       :value="character.experience" :controls="false" />
      </b-field>
      <b-field horizontal label="Level up in" class="stat-field is-marginless">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       :value="character.nextLevelExperience - character.experience " :controls="false" />
      </b-field>
    </div>

    <div v-for="statSection in statsSections" class="stats-section">
      <h2 class="title is-4">{{statSection.name}} ({{statSection.getPoints(stats) + statSection.getPoints(statsDelta)}})</h2>
      <b-field v-for="stat in statSection.stats" horizontal :label="stat.name" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       :value="stat.get(stats) + stat.get(statsDelta)"
                       :min="stat.get(stats)"
                       :max="stat.get(stats) + stat.get(statsDelta) + statSection.getPoints(stats) + statSection.getPoints(statsDelta)"
                       :controls="statSection.getPoints(stats) !== 0"
                       @input="statSection.setPoints(statsDelta, statSection.getPoints(statsDelta) - $event + stat.get(stats) + stat.get(statsDelta));
                               stat.set(statsDelta, $event - stat.get(stats))" />
      </b-field>
    </div>

    <b-field horizontal>
      <p class="control">
        <b-button size="is-medium" icon-left="undo" :disabled="!wasChangeMade" @click="reset">Reset</b-button>
        <b-button size="is-medium" icon-left="check" :disabled="!wasChangeMade" @click="commit">Commit</b-button>
      </p>
    </b-field>

  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import CharacterStatistics from '@/models/character-statistics';
import Character from '@/models/character';
import userModule from '@/store/user-module';
import { notify } from '@/services/notifications-service';

@Component
export default class CharacterStats extends Vue {
  @Prop(Object) readonly character: Character;

  // might be over-engineered just to avoid some duplicate code
  statsSections = [
    {
      name: 'Attributes',
      getPoints: (stats: CharacterStatistics) => stats.attributes.points,
      setPoints: (stats: CharacterStatistics, points: number) => stats.attributes.points = points,
      stats: [
        {
          name: 'Strength',
          get: (stats: CharacterStatistics) => stats.attributes.strength,
          set: (stats: CharacterStatistics, val: number) => stats.attributes.strength = val,
        },
        {
          name: 'Agility',
          get: (stats: CharacterStatistics) => stats.attributes.agility,
          set: (stats: CharacterStatistics, val: number) => stats.attributes.agility = val,
        },
      ],
    },
    {
      name: 'Skills',
      getPoints: (stats: CharacterStatistics) => stats.skills.points,
      setPoints: (stats: CharacterStatistics, points: number) => stats.skills.points = points,
      stats: [
        {
          name: 'Athletics',
          get: (stats: CharacterStatistics) => stats.skills.athletics,
          set: (stats: CharacterStatistics, val: number) => stats.skills.athletics = val,
        },
        {
          name: 'Horse Harchery',
          get: (stats: CharacterStatistics) => stats.skills.horseArchery,
          set: (stats: CharacterStatistics, val: number) => stats.skills.horseArchery = val,
        },
        {
          name: 'Iron Flesh',
          get: (stats: CharacterStatistics) => stats.skills.ironFlesh,
          set: (stats: CharacterStatistics, val: number) => stats.skills.ironFlesh = val,
        },
        {
          name: 'Power Draw',
          get: (stats: CharacterStatistics) => stats.skills.powerDraw,
          set: (stats: CharacterStatistics, val: number) => stats.skills.powerDraw = val,
        },
        {
          name: 'Power Strike',
          get: (stats: CharacterStatistics) => stats.skills.powerStrike,
          set: (stats: CharacterStatistics, val: number) => stats.skills.powerStrike = val,
        },
        {
          name: 'Power Throw',
          get: (stats: CharacterStatistics) => stats.skills.powerThrow,
          set: (stats: CharacterStatistics, val: number) => stats.skills.powerThrow = val,
        },
        {
          name: 'Riding',
          get: (stats: CharacterStatistics) => stats.skills.riding,
          set: (stats: CharacterStatistics, val: number) => stats.skills.riding = val,
        },
        {
          name: 'Shield',
          get: (stats: CharacterStatistics) => stats.skills.shield,
          set: (stats: CharacterStatistics, val: number) => stats.skills.shield = val,
        },
        {
          name: 'Weapon Master',
          get: (stats: CharacterStatistics) => stats.skills.weaponMaster,
          set: (stats: CharacterStatistics, val: number) => stats.skills.weaponMaster = val,
        },
      ],
    },
    {
      name: 'Weapon Proficiencies',
      getPoints: (stats: CharacterStatistics) => stats.weaponProficiencies.points,
      setPoints: (stats: CharacterStatistics, points: number) => stats.weaponProficiencies.points = points,
      stats: [
        {
          name: 'One Handed',
          get: (stats: CharacterStatistics) => stats.weaponProficiencies.oneHanded,
          set: (stats: CharacterStatistics, val: number) => stats.weaponProficiencies.oneHanded = val,
        },
        {
          name: 'Two Handed',
          get: (stats: CharacterStatistics) => stats.weaponProficiencies.twoHanded,
          set: (stats: CharacterStatistics, val: number) => stats.weaponProficiencies.twoHanded = val,
        },
        {
          name: 'Polearm',
          get: (stats: CharacterStatistics) => stats.weaponProficiencies.polearm,
          set: (stats: CharacterStatistics, val: number) => stats.weaponProficiencies.polearm = val,
        },
        {
          name: 'Bow',
          get: (stats: CharacterStatistics) => stats.weaponProficiencies.bow,
          set: (stats: CharacterStatistics, val: number) => stats.weaponProficiencies.bow = val,
        },
        {
          name: 'Crossbow',
          get: (stats: CharacterStatistics) => stats.weaponProficiencies.crossbow,
          set: (stats: CharacterStatistics, val: number) => stats.weaponProficiencies.crossbow = val,
        },
        {
          name: 'Throwing',
          get: (stats: CharacterStatistics) => stats.weaponProficiencies.throwing,
          set: (stats: CharacterStatistics, val: number) => stats.weaponProficiencies.throwing = val,
        },
      ],
    },
  ];

  // reset when changing char
  statsDelta: CharacterStatistics = this.createEmptyStatistics();

  get stats(): CharacterStatistics {
    return this.character.statistics;
  }

  get wasChangeMade(): boolean {
    return this.statsDelta.attributes.points !== 0
      || this.statsDelta.skills.points !== 0
      || this.statsDelta.weaponProficiencies.points !== 0;
  }

  createEmptyStatistics(): CharacterStatistics {
    return {
      attributes: {
        points: 0,
        strength: 0,
        agility: 0,
      },
      skills: {
        athletics: 0,
        horseArchery: 0,
        ironFlesh: 0,
        points: 0,
        powerDraw: 0,
        powerStrike: 0,
        powerThrow: 0,
        riding: 0,
        shield: 0,
        weaponMaster: 0,
      },
      weaponProficiencies: {
        points: 0,
        oneHanded: 0,
        twoHanded: 0,
        polearm: 0,
        bow: 0,
        throwing: 0,
        crossbow: 0,
      },
    };
  }

  reset() {
    this.statsDelta = this.createEmptyStatistics();
  }

  commit() {
    userModule.updateCharacterStats({
      characterId: this.character.id,
      stats: {
        attributes: {
          points: this.stats.attributes.points + this.statsDelta.attributes.points,
          strength: this.stats.attributes.strength + this.statsDelta.attributes.strength,
          agility: this.stats.attributes.agility + this.statsDelta.attributes.agility,
        },
        skills: {
          athletics: this.stats.skills.athletics + this.statsDelta.skills.athletics,
          horseArchery: this.stats.skills.horseArchery + this.statsDelta.skills.horseArchery,
          ironFlesh: this.stats.skills.ironFlesh + this.statsDelta.skills.ironFlesh,
          points: this.stats.skills.points + this.statsDelta.skills.points,
          powerDraw: this.stats.skills.powerDraw + this.statsDelta.skills.powerDraw,
          powerStrike: this.stats.skills.powerStrike + this.stats.skills.powerStrike,
          powerThrow: this.stats.skills.powerThrow + this.statsDelta.skills.powerThrow,
          riding: this.stats.skills.riding + this.statsDelta.skills.riding,
          shield: this.stats.skills.shield + this.statsDelta.skills.shield,
          weaponMaster: this.stats.skills.weaponMaster + this.statsDelta.skills.weaponMaster,
        },
        weaponProficiencies: {
          points: this.stats.weaponProficiencies.points + this.statsDelta.weaponProficiencies.points,
          oneHanded: this.stats.weaponProficiencies.oneHanded + this.statsDelta.weaponProficiencies.oneHanded,
          twoHanded: this.stats.weaponProficiencies.twoHanded + this.statsDelta.weaponProficiencies.twoHanded,
          polearm: this.stats.weaponProficiencies.polearm + this.statsDelta.weaponProficiencies.polearm,
          bow: this.stats.weaponProficiencies.bow + this.statsDelta.weaponProficiencies.bow,
          throwing: this.stats.weaponProficiencies.throwing + this.statsDelta.weaponProficiencies.throwing,
          crossbow: this.stats.weaponProficiencies.crossbow + this.statsDelta.weaponProficiencies.crossbow,
        },
      },
    }).then(() => notify('Character statistics updated'));
    this.reset();
  }

  openEditCharacterDialog() {
    this.$buefy.dialog.prompt({
      message: 'New name',
      inputAttrs: {
        value: this.character.name,
        minlength: 2,
        maxlength: 32,
      },
      trapFocus: true,
      onConfirm: (newName) => {
        userModule.renameCharacter({ character: this.character, newName });
        notify('Character renamed');
      },
    });
  }
}
</script>

<style lang="scss">
.stat-field {
  input {
    border: none;
  }

  .field-label {
    padding-top: 0;
  }
}
</style>

<style scoped lang="scss">
.character-name {
  display: flex;

  .character-name-edit {
    margin-left: 8px;
    display: none;
    cursor: pointer;
  }

  &:hover {
    .character-name-edit {
      display: inline;
    }
  }
}

.stats-section {
  margin-bottom: 20px;

  h2 {
    margin-bottom: 1rem;
  }
}
</style>
