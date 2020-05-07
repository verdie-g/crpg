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
                       :value="character.nextLevelExperience - character.experience" :controls="false" />
      </b-field>
    </div>

    <div class="stats-section">
      <h2 class="title is-4">Attributes ({{stats.attributes.points + statsDelta.attributes.points}})</h2>
      <b-field horizontal label="Strength" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('attributes', 'strength')"
                       @input="onInput('attributes', 'strength', $event)" />
      </b-field>

      <b-field horizontal label="Agility" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('attributes', 'agility')"
                       @input="onInput('attributes', 'agility', $event)" />
      </b-field>
    </div>

    <div class="stats-section">
      <h2 class="title is-4">Skills ({{stats.skills.points + statsDelta.skills.points}})</h2>
      <b-field horizontal label="Athletics" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'athletics')"
                       @input="onInput('skills', 'athletics', $event)" />
      </b-field>

      <b-field horizontal label="Horse Archery" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'horseArchery')"
                       @input="onInput('skills', 'horseArchery', $event)" />
      </b-field>

      <b-field horizontal label="Iron Flesh" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'ironFlesh')"
                       @input="onInput('skills', 'ironFlesh', $event)" />
      </b-field>

      <b-field horizontal label="Power Draw" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'powerDraw')"
                       @input="onInput('skills', 'powerDraw', $event)" />
      </b-field>

      <b-field horizontal label="Power Strike" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'powerStrike')"
                       @input="onInput('skills', 'powerStrike', $event)" />
      </b-field>

      <b-field horizontal label="Power Throw" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'powerThrow')"
                       @input="onInput('skills', 'powerThrow', $event)" />
      </b-field>

      <b-field horizontal label="Riding" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'riding')"
                       @input="onInput('skills', 'riding', $event)" />
      </b-field>

      <b-field horizontal label="Shield" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'shield')"
                       @input="onInput('skills', 'shield', $event)" />
      </b-field>

      <b-field horizontal label="Weapon Master" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'weaponMaster')"
                       @input="onInput('skills', 'weaponMaster', $event)" />
      </b-field>
    </div>

    <div class="stats-section">
      <h2 class="title is-4">Weapon Proficiencies ({{stats.weaponProficiencies.points + statsDelta.weaponProficiencies.points}})</h2>
      <b-field horizontal label="One Handed" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('weaponProficiencies', 'oneHanded')"
                       @input="onInput('weaponProficiencies', 'oneHanded', $event)" />
      </b-field>

      <b-field horizontal label="Two Handed" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('weaponProficiencies', 'twoHanded')"
                       @input="onInput('weaponProficiencies', 'twoHanded', $event)" />
      </b-field>

      <b-field horizontal label="Polearm" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('weaponProficiencies', 'polearm')"
                       @input="onInput('weaponProficiencies', 'polearm', $event)" />
      </b-field>

      <b-field horizontal label="Bow" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('weaponProficiencies', 'bow')"
                       @input="onInput('weaponProficiencies', 'bow', $event)" />
      </b-field>

      <b-field horizontal label="Crossbow" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('weaponProficiencies', 'crossbow')"
                       @input="onInput('weaponProficiencies', 'crossbow', $event)" />
      </b-field>

      <b-field horizontal label="Throwing" class="stat-field">
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('weaponProficiencies', 'throwing')"
                       @input="onInput('weaponProficiencies', 'throwing', $event)" />
      </b-field>
    </div>

    <b-field horizontal>
      <p class="control">
        <b-button size="is-medium" icon-left="undo" :disabled="!wasChangeMade" @click="reset">Reset</b-button>
        <b-button size="is-medium" icon-left="check" :disabled="!isChangeValid" @click="commit">Commit</b-button>
      </p>
    </b-field>

  </div>
</template>

<script lang="ts">
import {
  Component, Prop, Vue, Watch,
} from 'vue-property-decorator';
import CharacterStatistics from '@/models/character-statistics';
import Character from '@/models/character';
import userModule from '@/store/user-module';
import { notify } from '@/services/notifications-service';

@Component
export default class CharacterStatsComponent extends Vue {
  @Prop(Object) readonly character: Character;

  statsDelta: CharacterStatistics = this.createEmptyStatistics();

  get stats(): CharacterStatistics {
    return this.character.statistics;
  }

  get wasChangeMade(): boolean {
    return this.statsDelta.attributes.points !== 0
      || this.statsDelta.skills.points !== 0
      || this.statsDelta.weaponProficiencies.points !== 0;
  }

  get isChangeValid(): boolean {
    return this.stats.attributes.points + this.statsDelta.attributes.points >= 0
      && this.stats.skills.points + this.statsDelta.skills.points >= 0
      && this.stats.weaponProficiencies.points + this.statsDelta.weaponProficiencies.points >= 0;
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

  getInputProps<
    TSection extends keyof CharacterStatistics,
    TStat extends keyof CharacterStatistics[TSection]>
  (statSectionKey: TSection, statKey: TStat) {
    // type assertion is needed because compiler doesn't understand it's necessarily a number
    const initialValue = <number><unknown>this.stats[statSectionKey][statKey];
    const deltaValue = <number><unknown>this.statsDelta[statSectionKey][statKey];
    const initialPoints = this.stats[statSectionKey].points;
    const deltaPoints = this.statsDelta[statSectionKey].points;

    return {
      value: initialValue + deltaValue,
      min: initialValue,
      max: initialValue + deltaValue + initialPoints + deltaPoints,
      controls: initialPoints !== 0, // hide controls (+/-) if there is no points to give
    };
  }

  onInput(statSectionKey: keyof CharacterStatistics, statKey: string, value: number) {
    // typing this function correctly was too hard
    const statInitialSection = this.stats[statSectionKey] as any;
    const statDeltaSection = this.statsDelta[statSectionKey] as any;

    const oldStatValue = statDeltaSection[statKey];
    statDeltaSection.points += statInitialSection[statKey] + statDeltaSection[statKey] - value;
    statDeltaSection[statKey] = value - statInitialSection[statKey];
    const newStatValue = statDeltaSection[statKey];

    if (statKey === 'agility') {
      this.statsDelta.weaponProficiencies.points += this.wppForAgility(newStatValue) - this.wppForAgility(oldStatValue);
    } else if (statKey === 'weaponMaster') {
      this.statsDelta.weaponProficiencies.points += this.wppForWeaponMaster(newStatValue) - this.wppForWeaponMaster(oldStatValue);
    }
  }

  wppForAgility(agility: number): number {
    return 14 * agility;
  }

  wppForWeaponMaster(weaponMaster: number): number {
    return weaponMaster === 0
      ? 0
      : 55 + 20 * weaponMaster;
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

  @Watch('character')
  onCharacterChange() {
    this.statsDelta = this.createEmptyStatistics();
  }
}
</script>

<style lang="scss">
.stat-field {
  &:not(:last-child) {
    margin-bottom: 0;
  }

  input {
    border: none;
  }

  .field-label {
    padding-top: 0;
  }

  button.button.is-small {
    border-radius: 0;
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
