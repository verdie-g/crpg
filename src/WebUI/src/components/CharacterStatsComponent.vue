<template>
  <div>
    <div class="characteristic-section">
      <div class="character-name">
        <h1 class="title is-3">{{ character.name }}</h1>
        <b-icon
          icon="pencil-alt"
          class="character-name-edit"
          @click.native="openCharacterUpdateModal"
        />
      </div>

      <b-field horizontal class="characteristic-field is-marginless">
        <template v-slot:label>
          <b-tooltip
            label="Number of times you retired this character."
            position="is-left"
            multilined
          >
            Generation
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="character.generation"
          :controls="false"
        />
      </b-field>
      <b-field horizontal label="Level" class="characteristic-field is-marginless">
        <b-numberinput
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="character.level"
          :controls="false"
        />
      </b-field>
      <b-field horizontal label="Experience" class="characteristic-field is-marginless">
        <b-numberinput
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="character.experience"
          :controls="false"
        />
      </b-field>
      <b-field horizontal label="Next level in" class="characteristic-field is-marginless">
        <b-numberinput
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="experienceTillNextLevel()"
          :controls="false"
        />
      </b-field>
      <b-field horizontal label="HealthPoints" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="computeHealthPoints()"
          :controls="false"
        />
      </b-field>
      <!-- TODO: align correctly -->
      <b-field horizontal label="KDA" class="characteristic-field">
        <b-input size="is-small" :value="getKda()" readonly />
      </b-field>
    </div>

    <div class="characteristic-section" v-if="characteristics !== null">
      <h2 class="title is-4">
        Attributes ({{
          characteristics.attributes.points + characteristicsDelta.attributes.points
        }})
        <b-tooltip
          label="Convert 1 attribute point to 2 skill points"
          class="convert-button"
          v-if="characteristics.attributes.points + characteristicsDelta.attributes.points >= 1"
        >
          <b-icon
            icon="exchange-alt"
            size="is-small"
            type="is-primary"
            @click.native="convertCharacteristics(characteristicisticConversion.AttributesToSkills)"
          />
        </b-tooltip>
      </h2>

      <b-field horizontal class="characteristic-field">
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Strength
            <template v-slot:content>
              Increases your health points.
              <s>Allows you to use higher tier weapons and armor.</s>
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          controls-position="compact"
          :exponential="0.5"
          v-bind="getInputProps('attributes', 'strength')"
          @input="onInput('attributes', 'strength', $event)"
        />
      </b-field>

      <b-field horizontal class="characteristic-field">
        <template v-slot:label>
          <b-tooltip
            label="Increases your weapon points and makes you move a bit faster."
            position="is-left"
            multilined
          >
            Agility
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('attributes', 'agility')"
          @input="onInput('attributes', 'agility', $event)"
        />
      </b-field>
    </div>

    <div class="characteristic-section" v-if="characteristics !== null">
      <h2 class="title is-4">
        Skills ({{ characteristics.skills.points + characteristicsDelta.skills.points }})
        <b-tooltip
          label="Convert 2 skill points to 1 attribute point"
          class="convert-button"
          v-if="characteristics.skills.points + characteristicsDelta.skills.points >= 2"
        >
          <b-icon
            icon="exchange-alt"
            size="is-small"
            type="is-primary"
            @click.native="convertCharacteristics(characteristicisticConversion.SkillsToAttributes)"
          />
        </b-tooltip>
      </h2>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('ironFlesh') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Iron Flesh
            <template v-slot:content>
              Increases your health
              <s>and reduces the negative impact armor has on weapon points</s>
              . Requires 3 strength per level.
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'ironFlesh')"
          @input="onInput('skills', 'ironFlesh', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('powerStrike') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip
            label="Increases melee damage. Requires 3 strength per level."
            position="is-left"
            multilined
          >
            Power Strike
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'powerStrike')"
          @input="onInput('skills', 'powerStrike', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('powerDraw') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Power Draw
            <template v-slot:content>
              Increases bow damage.
              <s>Allows you to use higher tiers bow.</s>
              Requires 3 strength per level.
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'powerDraw')"
          @input="onInput('skills', 'powerDraw', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('powerThrow') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Power Throw
            <template v-slot:content>
              Increases throw damage.
              <s>Allows you to use higher tier weapons.</s>
              Requires 3 strength per level.
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'powerThrow')"
          @input="onInput('skills', 'powerThrow', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('athletics') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip
            label="Increases running speed. Requires 3 agility per level."
            position="is-left"
            multilined
          >
            Athletics
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'athletics')"
          @input="onInput('skills', 'athletics', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('riding') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Riding
            <template v-slot:content>
              Increases riding speed, acceleration and maneuver.
              <s>Allows you to ride higher tier mounts.</s>
              Requires 3 agility per level.
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'riding')"
          @input="onInput('skills', 'riding', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('weaponMaster') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip
            label="Gives weapon points. Requires 3 agility per level."
            position="is-left"
            multilined
          >
            Weapon Master
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'weaponMaster')"
          @input="onInput('skills', 'weaponMaster', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('mountedArchery') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Mounted Archery
            <template v-slot:content>
              <s>Reduces penalty for using ranged weapons on a moving mount by 10% per level.</s>
              Requires 6 agility per level.
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'mountedArchery')"
          @input="onInput('skills', 'mountedArchery', $event)"
        />
      </b-field>

      <b-field
        horizontal
        class="characteristic-field"
        :type="currentSkillRequirementsSatisfied('shield') ? 'is-primary' : 'is-danger'"
      >
        <template v-slot:label>
          <b-tooltip position="is-left" multilined>
            Shield
            <template v-slot:content>
              <s>Improves shield durability, shield speed and</s>
              increases coverage from ranged attacks.
              <s>Allows you to use higher tier shields.</s>
              Requires 6 agility per level.
            </template>
          </b-tooltip>
        </template>
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('skills', 'shield')"
          @input="onInput('skills', 'shield', $event)"
        />
      </b-field>
    </div>

    <div class="characteristic-section" v-if="characteristics !== null">
      <h2 class="title is-4">
        Weapon Proficiencies ({{
          characteristics.weaponProficiencies.points +
          characteristicsDelta.weaponProficiencies.points
        }})
      </h2>
      <b-field horizontal label="One Handed" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('weaponProficiencies', 'oneHanded')"
          @input="onInput('weaponProficiencies', 'oneHanded', $event)"
        />
      </b-field>

      <b-field horizontal label="Two Handed" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('weaponProficiencies', 'twoHanded')"
          @input="onInput('weaponProficiencies', 'twoHanded', $event)"
        />
      </b-field>

      <b-field horizontal label="Polearm" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('weaponProficiencies', 'polearm')"
          @input="onInput('weaponProficiencies', 'polearm', $event)"
        />
      </b-field>

      <b-field horizontal label="Bow" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('weaponProficiencies', 'bow')"
          @input="onInput('weaponProficiencies', 'bow', $event)"
        />
      </b-field>

      <b-field horizontal label="Crossbow" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('weaponProficiencies', 'crossbow')"
          @input="onInput('weaponProficiencies', 'crossbow', $event)"
        />
      </b-field>

      <b-field horizontal label="Throwing" class="characteristic-field">
        <b-numberinput
          size="is-small"
          :editable="false"
          :exponential="0.5"
          controls-position="compact"
          v-bind="getInputProps('weaponProficiencies', 'throwing')"
          @input="onInput('weaponProficiencies', 'throwing', $event)"
        />
      </b-field>
    </div>

    <b-field horizontal>
      <p class="control">
        <b-button size="is-medium" icon-left="undo" :disabled="!wasChangeMade" @click="reset">
          Reset
        </b-button>
        <b-button
          size="is-medium"
          icon-left="check"
          :disabled="!wasChangeMade || !isChangeValid"
          @click="commit"
          :loading="updatingCharacteristics"
        >
          Commit
        </b-button>
      </p>
    </b-field>

    <b-modal
      :active.sync="isCharacterUpdateModalActive"
      has-modal-card
      trap-focus
      aria-role="dialog"
      aria-modal
      ref="characterUpdateModal"
    >
      <template>
        <form @submit.prevent="onCharacterUpdateSubmit">
          <div class="modal-card" style="width: auto">
            <header class="modal-card-head">
              <p class="modal-card-title">Character Update</p>
            </header>

            <section class="modal-card-body">
              <b-field label="Name">
                <b-input
                  type="text"
                  v-model="characterUpdate.name"
                  minlength="2"
                  maxlength="32"
                  required
                />
              </b-field>
            </section>

            <footer class="modal-card-foot">
              <button class="button" type="button" @click="closeCharacterUpdateModal">Close</button>
              <button class="button is-primary">Update</button>
            </footer>
          </div>
        </form>
      </template>
    </b-modal>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import CharacterCharacteristics from '@/models/character-characteristics';
import Character from '@/models/character';
import userModule from '@/store/user-module';
import { notify } from '@/services/notifications-service';
import { computeHealthPoints, computeHowMuchXPTillNextLevel } from '@/services/characters-service';
import CharacterAttributes from '@/models/character-attributes';
import CharacterSkills from '@/models/character-skills';
import CharacterWeaponProficiencies from '@/models/character-weapon-proficiencies';
import CharacteristicConversion from '@/models/characteristic-conversion';
import CharacterUpdate from '@/models/character-update';
import { applyPolynomialFunction } from '@/utils/math';
import Constants from '../../../../data/constants.json';

type CharacteristicSectionKey = keyof CharacterCharacteristics;
type AttributeKey = keyof CharacterAttributes;
type SkillKey = keyof CharacterSkills;
type WeaponProficienciesKey = keyof CharacterWeaponProficiencies;
type CharacteristicKey = AttributeKey | SkillKey | WeaponProficienciesKey;

@Component
export default class CharacterCharacteristicsComponent extends Vue {
  @Prop(Object) character: Character;

  isCharacterUpdateModalActive = false;
  characterUpdate: CharacterUpdate = { name: '' };

  updatingCharacteristics = false;
  characteristicsDelta: CharacterCharacteristics = this.createEmptycharacteristics();

  get characteristicisticConversion(): typeof CharacteristicConversion {
    return CharacteristicConversion;
  }

  get characteristics(): CharacterCharacteristics {
    return userModule.characterCharacteristics(this.character.id)!;
  }

  get wasChangeMade(): boolean {
    return (
      this.characteristicsDelta.attributes.points !== 0 ||
      this.characteristicsDelta.skills.points !== 0 ||
      this.characteristicsDelta.weaponProficiencies.points !== 0
    );
  }

  get isChangeValid(): boolean {
    return (
      this.characteristics.attributes.points + this.characteristicsDelta.attributes.points >= 0 &&
      this.characteristics.skills.points + this.characteristicsDelta.skills.points >= 0 &&
      this.characteristics.weaponProficiencies.points +
        this.characteristicsDelta.weaponProficiencies.points >=
        0 &&
      this.allCurrentSkillRequirementsSatisfied
    );
  }

  get allCurrentSkillRequirementsSatisfied(): boolean {
    return Object.keys(this.characteristics.skills)
      .filter(skillKey => skillKey !== 'points')
      .every(skillKey => this.currentSkillRequirementsSatisfied(skillKey as SkillKey));
  }

  created() {
    userModule.getCharacterCharacteristics(this.character.id);
    userModule.getCharacterStatistics(this.character.id);
  }

  createEmptycharacteristics(): CharacterCharacteristics {
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
        mountedArchery: 0,
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

  getKda(): string {
    const statistics = userModule.characterStatistics(this.character.id);
    if (statistics === null) {
      return '0/0/0';
    }

    const ratio =
      statistics.deaths === 0
        ? '∞'
        : Math.round((100 * (statistics.kills + statistics.assists)) / statistics.deaths) / 100;
    return `${statistics.kills}/${statistics.deaths}/${statistics.assists} (${ratio})`;
  }
  computeHealthPoints(): number {
    return computeHealthPoints(
      this.getInputProps('skills', 'ironFlesh').value,
      this.getInputProps('attributes', 'strength').value
    );
  }
  experienceTillNextLevel(): number {
    return computeHowMuchXPTillNextLevel(this.character.experience, this.character.level);
  }

  convertCharacteristics(conversion: CharacteristicConversion): Promise<CharacterCharacteristics> {
    return userModule.convertCharacterCharacteristics({
      characterId: this.character.id,
      conversion,
    });
  }

  getInputProps(
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey
  ): { value: number; min: number; max: number; controls: boolean } {
    const initialValue = (this.characteristics[characteristicSectionKey] as any)[characteristicKey];
    const deltaValue = (this.characteristicsDelta[characteristicSectionKey] as any)[
      characteristicKey
    ];
    const initialPoints = this.characteristics[characteristicSectionKey].points;
    const deltaPoints = this.characteristicsDelta[characteristicSectionKey].points;

    const value = initialValue + deltaValue;
    const points = initialPoints + deltaPoints;
    const costToIncrease =
      this.characteristicCost(characteristicSectionKey, characteristicKey, value + 1) -
      this.characteristicCost(characteristicSectionKey, characteristicKey, value);
    const requirementsSatisfied = this.characteristicRequirementsSatisfied(
      characteristicSectionKey,
      characteristicKey,
      value + 1
    );
    return {
      value,
      min: initialValue,
      max: value + (costToIncrease <= points && requirementsSatisfied ? 1 : 0),
      controls: initialPoints !== 0, // hide controls (+/-) if there is no points to give
    };
  }

  onInput(
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    newCharacteristicValue: number
  ): void {
    const characteristicInitialSection = this.characteristics![characteristicSectionKey] as any;
    const characteristicDeltaSection = this.characteristicsDelta[characteristicSectionKey] as any;

    const oldCharacteristicValue =
      characteristicInitialSection[characteristicKey] +
      characteristicDeltaSection[characteristicKey];
    characteristicDeltaSection.points +=
      this.characteristicCost(characteristicSectionKey, characteristicKey, oldCharacteristicValue) -
      this.characteristicCost(characteristicSectionKey, characteristicKey, newCharacteristicValue);
    characteristicDeltaSection[characteristicKey] =
      newCharacteristicValue - characteristicInitialSection[characteristicKey];

    if (characteristicKey === 'agility') {
      this.characteristicsDelta.weaponProficiencies.points +=
        this.wppForAgility(newCharacteristicValue) - this.wppForAgility(oldCharacteristicValue);
    } else if (characteristicKey === 'weaponMaster') {
      this.characteristicsDelta.weaponProficiencies.points +=
        this.wppForWeaponMaster(newCharacteristicValue) -
        this.wppForWeaponMaster(oldCharacteristicValue);
    }
  }

  wppForAgility(agility: number): number {
    return Math.floor(
      applyPolynomialFunction(agility, Constants.weaponProficiencyPointsForAgilityCoefs)
    );
  }

  wppForWeaponMaster(weaponMaster: number): number {
    return Math.floor(
      applyPolynomialFunction(weaponMaster, Constants.weaponProficiencyPointsForWeaponMasterCoefs)
    );
  }

  characteristicRequirementsSatisfied(
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    characteristic: number
  ): boolean {
    switch (characteristicSectionKey) {
      case 'skills':
        return this.skillRequirementsSatisfied(characteristicKey as SkillKey, characteristic);
      default:
        return true;
    }
  }

  currentSkillRequirementsSatisfied(skillKey: SkillKey): boolean {
    return this.skillRequirementsSatisfied(
      skillKey,
      this.characteristics.skills[skillKey] + this.characteristicsDelta.skills[skillKey]
    );
  }

  skillRequirementsSatisfied(skillKey: SkillKey, skill: number): boolean {
    switch (skillKey) {
      case 'ironFlesh':
      case 'powerStrike':
      case 'powerDraw':
      case 'powerThrow':
        return (
          skill <=
          Math.floor(
            (this.characteristics.attributes.strength +
              this.characteristicsDelta.attributes.strength) /
              3
          )
        );

      case 'athletics':
      case 'riding':
      case 'weaponMaster':
        return (
          skill <=
          Math.floor(
            (this.characteristics.attributes.agility +
              this.characteristicsDelta.attributes.agility) /
              3
          )
        );

      case 'mountedArchery':
      case 'shield':
        return (
          skill <=
          Math.floor(
            (this.characteristics.attributes.agility +
              this.characteristicsDelta.attributes.agility) /
              6
          )
        );

      default:
        return false;
    }
  }

  characteristicCost(
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    characteristic: number
  ): number {
    if (characteristicSectionKey === 'weaponProficiencies') {
      return Math.floor(
        applyPolynomialFunction(characteristic, Constants.weaponProficiencyCostCoefs)
      );
    }

    return characteristic;
  }

  reset(): void {
    this.characteristicsDelta = this.createEmptycharacteristics();
  }

  commit(): void {
    this.updatingCharacteristics = true;
    userModule
      .updateCharacterCharacteristics({
        characterId: this.character.id,
        characteristics: {
          attributes: {
            points:
              this.characteristics.attributes.points + this.characteristicsDelta.attributes.points,
            strength:
              this.characteristics.attributes.strength +
              this.characteristicsDelta.attributes.strength,
            agility:
              this.characteristics.attributes.agility +
              this.characteristicsDelta.attributes.agility,
          },
          skills: {
            points: this.characteristics.skills.points + this.characteristicsDelta.skills.points,
            ironFlesh:
              this.characteristics.skills.ironFlesh + this.characteristicsDelta.skills.ironFlesh,
            powerStrike:
              this.characteristics.skills.powerStrike +
              this.characteristicsDelta.skills.powerStrike,
            powerDraw:
              this.characteristics.skills.powerDraw + this.characteristicsDelta.skills.powerDraw,
            powerThrow:
              this.characteristics.skills.powerThrow + this.characteristicsDelta.skills.powerThrow,
            athletics:
              this.characteristics.skills.athletics + this.characteristicsDelta.skills.athletics,
            riding: this.characteristics.skills.riding + this.characteristicsDelta.skills.riding,
            weaponMaster:
              this.characteristics.skills.weaponMaster +
              this.characteristicsDelta.skills.weaponMaster,
            mountedArchery:
              this.characteristics.skills.mountedArchery +
              this.characteristicsDelta.skills.mountedArchery,
            shield: this.characteristics.skills.shield + this.characteristicsDelta.skills.shield,
          },
          weaponProficiencies: {
            points:
              this.characteristics.weaponProficiencies.points +
              this.characteristicsDelta.weaponProficiencies.points,
            oneHanded:
              this.characteristics.weaponProficiencies.oneHanded +
              this.characteristicsDelta.weaponProficiencies.oneHanded,
            twoHanded:
              this.characteristics.weaponProficiencies.twoHanded +
              this.characteristicsDelta.weaponProficiencies.twoHanded,
            polearm:
              this.characteristics.weaponProficiencies.polearm +
              this.characteristicsDelta.weaponProficiencies.polearm,
            bow:
              this.characteristics.weaponProficiencies.bow +
              this.characteristicsDelta.weaponProficiencies.bow,
            throwing:
              this.characteristics.weaponProficiencies.throwing +
              this.characteristicsDelta.weaponProficiencies.throwing,
            crossbow:
              this.characteristics.weaponProficiencies.crossbow +
              this.characteristicsDelta.weaponProficiencies.crossbow,
          },
        },
      })
      .then(() => {
        this.updatingCharacteristics = false;
        notify('Character characteristics updated');
      });
    this.reset();
  }

  characterUpdateFromCharacter(character: Character): CharacterUpdate {
    return { name: character.name };
  }

  openCharacterUpdateModal(): void {
    this.characterUpdate = this.characterUpdateFromCharacter(this.character);
    this.isCharacterUpdateModalActive = true;
  }

  closeCharacterUpdateModal(): void {
    this.isCharacterUpdateModalActive = false;
  }

  onCharacterUpdateSubmit(): void {
    userModule
      .updateCharacter({
        characterId: this.character.id,
        characterUpdate: this.characterUpdate,
      })
      .then(() => notify('Character updated!'));
    this.closeCharacterUpdateModal();
  }

  @Watch('character')
  onCharacterChange(): void {
    this.characteristicsDelta = this.createEmptycharacteristics();
  }
}
</script>

<style lang="scss">
.characteristic-field {
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

.characteristic-section {
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
