<template>
  <div>
    <div class="characteristic-section">
      <h2 class="title is-4">
        Weapon Proficiencies ({{ characteristics.weaponProficiencies.points }})
      </h2>
      <b-field horizontal label="One Handed" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :value="characteristics.weaponProficiencies.oneHanded"
        />
      </b-field>

      <b-field horizontal label="Two Handed" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :value="characteristics.weaponProficiencies.twoHanded"
        />
      </b-field>

      <b-field horizontal label="Polearm" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :value="characteristics.weaponProficiencies.polearm"
        />
      </b-field>

      <b-field horizontal label="Bow" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :value="characteristics.weaponProficiencies.bow"
        />
      </b-field>

      <b-field horizontal label="Crossbow" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :value="characteristics.weaponProficiencies.crossbow"
        />
      </b-field>

      <b-field horizontal label="Throwing" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :value="characteristics.weaponProficiencies.throwing"
        />
      </b-field>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import CharacterCharacteristics from '@/models/character-characteristics';
import Character from '@/models/character';
import userModule from '@/store/user-module';

@Component
export default class UserCharacterWeaponProficienciesComponent extends Vue {
  @Prop(Object) character: Character;

  get characteristics(): CharacterCharacteristics {
    return this.createEmptycharacteristics();
    // return userModule.characterCharacteristics(this.character.id)!;
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
        points: 222,
        oneHanded: 34,
        twoHanded: 23,
        polearm: 12,
        bow: 54,
        throwing: 23,
        crossbow: 45,
      },
    };
  }
}
</script>

<style lang="scss"></style>

<style scoped lang="scss">
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
