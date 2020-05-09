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
      <h2 class="title is-4">
        Attributes ({{stats.attributes.points + statsDelta.attributes.points}})
        <b-tooltip label="Convert 1 attribute point to 2 skill points" class="convert-button"
                   v-if="stats.attributes.points + statsDelta.attributes.points >= 1">
          <b-icon icon="exchange-alt" size="is-small" @click.native="convertStats(statisticConversion.AttributesToSkills)" />
        </b-tooltip>
      </h2>

      <b-field horizontal class="stat-field">
        <template slot="label">
          <b-tooltip label="Increases your health points by 1 per level. Allows you to use higher tier weapons and armor."
                     position="is-left" multilined>Strength</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('attributes', 'strength')"
                       @input="onInput('attributes', 'strength', $event)" />
      </b-field>

      <b-field horizontal class="stat-field">
        <template slot="label">
          <b-tooltip label="Increases your weapon points by 14 per level. Makes you move a bit faster."
                     position="is-left" multilined>Agility</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('attributes', 'agility')"
                       @input="onInput('attributes', 'agility', $event)" />
      </b-field>
    </div>

    <div class="stats-section">
      <h2 class="title is-4">
        Skills ({{stats.skills.points + statsDelta.skills.points}})
        <b-tooltip label="Convert 2 skills points to 1 attribute point" class="convert-button"
                   v-if="stats.skills.points + statsDelta.skills.points >= 2">
          <b-icon icon="exchange-alt" size="is-small" @click.native="convertStats(statisticConversion.SkillsToAttributes)" />
        </b-tooltip>
      </h2>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('ironFlesh') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Increases your health by 2 points per level and reduces the negative impact armor has on weapon points. Requires 3 strength per level."
                     position="is-left" multilined>Iron Flesh</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'ironFlesh')"
                       @input="onInput('skills', 'ironFlesh', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('powerStrike') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Increases melee damage by 8% per level. Requires 3 strength per level."
                     position="is-left" multilined>Power Strike</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'powerStrike')"
                       @input="onInput('skills', 'powerStrike', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('powerDraw') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Increases bow damage by 14% per level (capped at 4 above used bow's difficulty). Allows you to use higher tiers bow. Requires 3 strength per level."
                     position="is-left" multilined>Power Draw</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'powerDraw')"
                       @input="onInput('skills', 'powerDraw', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('powerThrow') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Increases throw damage by 10% per level. Allows you to use higher tier weapons. Requires 3 strength per level."
                     position="is-left" multilined>Power Throw</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'powerThrow')"
                       @input="onInput('skills', 'powerThrow', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('athletics') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Increases running speed. Requires 3 agility per level." position="is-left" multilined>Athletics</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'athletics')"
                       @input="onInput('skills', 'athletics', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('riding') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Increases riding speed, acceleration and maneuver. Allows you to ride higher tier horses. Requires 3 agility per level."
                     position="is-left" multilined>Riding</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'riding')"
                       @input="onInput('skills', 'riding', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('weaponMaster') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Gives you level*20+55 weapon points per level. Requires 3 agility per level." position="is-left" multilined>Weapon Master</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'weaponMaster')"
                       @input="onInput('skills', 'weaponMaster', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('horseArchery') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Reduces penalty for using ranged weapons on a moving horse by 10% per level. Requires 6 agility per level."
                     position="is-left" multilined>Horse Archery</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'horseArchery')"
                       @input="onInput('skills', 'horseArchery', $event)" />
      </b-field>

      <b-field horizontal class="stat-field" :type="currentSkillRequirementsSatisfied('shield') ? 'is-primary' : 'is-danger'">
        <template slot="label">
          <b-tooltip label="Reduces damage to shields by 16% per level, improves shield speed and increases coverage form ranged attacks. Allows you to use higher tier shields. Requires 6 agility per level."
                     position="is-left" multilined>Riding</b-tooltip>
        </template>
        <b-numberinput size="is-small" :editable="false" controls-position="compact"
                       v-bind="getInputProps('skills', 'shield')"
                       @input="onInput('skills', 'shield', $event)" />
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
        <b-button size="is-medium" icon-left="check" :disabled="!wasChangeMade || !isChangeValid"
                  @click="commit" :loading="updatingStats">Commit</b-button>
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
import CharacterAttributes from '@/models/character-attributes';
import CharacterSkills from '@/models/character-skills';
import CharacterWeaponProficiencies from '@/models/character-weapon-proficiencies';
import StatisticConversion from '@/models/statistic-conversion';

type StatSectionKey = keyof CharacterStatistics;
type AttributeKey = keyof CharacterAttributes;
type SkillKey = keyof CharacterSkills;
type WeaponProficienciesKey = keyof CharacterWeaponProficiencies;
type StatKey = AttributeKey | SkillKey | WeaponProficienciesKey;

@Component
export default class CharacterStatsComponent extends Vue {
  @Prop(Object) character: Character;

  updatingStats = false;
  statsDelta: CharacterStatistics = this.createEmptyStatistics();

  get statisticConversion() {
    return StatisticConversion;
  }

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
      && this.stats.weaponProficiencies.points + this.statsDelta.weaponProficiencies.points >= 0
      && this.allCurrentSkillRequirementsSatisfied;
  }

  get allCurrentSkillRequirementsSatisfied(): boolean {
    return Object.keys(this.stats.skills)
      .filter(skillKey => skillKey !== 'points')
      .every(skillKey => this.currentSkillRequirementsSatisfied(skillKey as SkillKey));
  }

  createEmptyStatistics(): CharacterStatistics {
    return {
      attributes: {
        points: 0,
        strength: 0,
        agility: 0,
      },
      skills: {
        points: 0,
        ironFlesh: 0,
        powerStrike: 0,
        powerDraw: 0,
        powerThrow: 0,
        athletics: 0,
        riding: 0,
        weaponMaster: 0,
        horseArchery: 0,
        shield: 0,
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

  convertStats(conversion: StatisticConversion) {
    userModule.convertCharacterStats({ characterId: this.character.id, conversion });
  }

  getInputProps(statSectionKey: StatSectionKey, statKey: StatKey) {
    const initialValue = (this.stats[statSectionKey] as any)[statKey];
    const deltaValue = (this.statsDelta[statSectionKey] as any)[statKey];
    const initialPoints = this.stats[statSectionKey].points;
    const deltaPoints = this.statsDelta[statSectionKey].points;

    const value = initialValue + deltaValue;
    const points = initialPoints + deltaPoints;
    const costToIncrease = this.statCost(statSectionKey, statKey, value + 1) - this.statCost(statSectionKey, statKey, value);
    const requirementsSatisfied = this.statRequirementsSatisfied(statSectionKey, statKey, value + 1);
    return {
      value,
      min: initialValue,
      max: value + ((costToIncrease <= points && requirementsSatisfied) ? 1 : 0),
      controls: initialPoints !== 0, // hide controls (+/-) if there is no points to give
    };
  }

  onInput(statSectionKey: StatSectionKey, statKey: StatKey, value: number) {
    const statInitialSection = this.stats[statSectionKey] as any;
    const statDeltaSection = this.statsDelta[statSectionKey] as any;

    const oldStatValue = statDeltaSection[statKey];
    statDeltaSection.points += this.statCost(statSectionKey, statKey, statInitialSection[statKey] + statDeltaSection[statKey])
      - this.statCost(statSectionKey, statKey, value);
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

  statRequirementsSatisfied(statSectionKey: StatSectionKey, statKey: StatKey, stat: number): boolean {
    switch (statSectionKey) {
      case 'skills':
        return this.skillRequirementsSatisfied(statKey as SkillKey, stat);
      default:
        return true;
    }
  }

  currentSkillRequirementsSatisfied(skillKey: SkillKey): boolean {
    return this.skillRequirementsSatisfied(skillKey, this.stats.skills[skillKey] + this.statsDelta.skills[skillKey]);
  }

  skillRequirementsSatisfied(skillKey: SkillKey, skill: number): boolean {
    switch (skillKey) {
      case 'ironFlesh':
      case 'powerStrike':
      case 'powerDraw':
      case 'powerThrow':
        return skill <= Math.floor((this.stats.attributes.strength + this.statsDelta.attributes.strength) / 3);

      case 'athletics':
      case 'riding':
      case 'weaponMaster':
        return skill <= Math.floor((this.stats.attributes.agility + this.statsDelta.attributes.agility) / 3);

      case 'horseArchery':
      case 'shield':
        return skill <= Math.floor((this.stats.attributes.agility + this.statsDelta.attributes.agility) / 6);

      default:
        return false;
    }
  }

  statCost(statSectionKey: StatSectionKey, statKey: StatKey, stat: number): number {
    if (statSectionKey === 'weaponProficiencies') {
      const a = 0.0005;
      const b = 3;
      return Math.floor(a * stat * (stat + 1) * (2 * stat + 1) / 6 + b * stat);
    }

    return stat;
  }

  reset() {
    this.statsDelta = this.createEmptyStatistics();
  }

  commit() {
    this.updatingStats = true;
    userModule.updateCharacterStats({
      characterId: this.character.id,
      stats: {
        attributes: {
          points: this.stats.attributes.points + this.statsDelta.attributes.points,
          strength: this.stats.attributes.strength + this.statsDelta.attributes.strength,
          agility: this.stats.attributes.agility + this.statsDelta.attributes.agility,
        },
        skills: {
          points: this.stats.skills.points + this.statsDelta.skills.points,
          ironFlesh: this.stats.skills.ironFlesh + this.statsDelta.skills.ironFlesh,
          powerStrike: this.stats.skills.powerStrike + this.statsDelta.skills.powerStrike,
          powerDraw: this.stats.skills.powerDraw + this.statsDelta.skills.powerDraw,
          powerThrow: this.stats.skills.powerThrow + this.statsDelta.skills.powerThrow,
          athletics: this.stats.skills.athletics + this.statsDelta.skills.athletics,
          riding: this.stats.skills.riding + this.statsDelta.skills.riding,
          weaponMaster: this.stats.skills.weaponMaster + this.statsDelta.skills.weaponMaster,
          horseArchery: this.stats.skills.horseArchery + this.statsDelta.skills.horseArchery,
          shield: this.stats.skills.shield + this.statsDelta.skills.shield,
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
    }).then(() => {
      this.updatingStats = false;
      notify('Character statistics updated');
    });
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
      onConfirm: newName => {
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

    .convert-button {
      margin-left: 8px;
      cursor: pointer;
    }
  }
}
</style>
