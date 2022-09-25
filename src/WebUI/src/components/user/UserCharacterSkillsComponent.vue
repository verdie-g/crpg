<template>
  <div>
    <div class="characteristic-section">
      <b-field horizontal label="Generation" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="character.generation"
          :controls="false"
        />
      </b-field>
      <b-field horizontal label="Level" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          controls-position="compact"
          :value="character.level"
          :controls="false"
        />
      </b-field>
      <b-field horizontal label="KDA" class="characteristic-field">
        <b-input size="is-small" :value="getKda()" readonly />
      </b-field>
    </div>

    <div class="characteristic-section" v-if="characteristics !== null">
      <h2 class="title is-4 mt-3">Attributes ({{ characteristics.attributes.points }})</h2>

      <b-field horizontal label="Strength" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.attributes.strength"
        />
      </b-field>

      <b-field horizontal label="Agility" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.attributes.agility"
        />
      </b-field>
    </div>

    <div class="characteristic-section" v-if="characteristics !== null">
      <h2 class="title is-4 mt-3">Skills ({{ characteristics.skills.points }})</h2>

      <b-field horizontal class="characteristic-field" label="Iron Flesh">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.ironFlesh"
        />
      </b-field>

      <b-field horizontal label="Power Strike" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.powerStrike"
        />
      </b-field>

      <b-field horizontal label="Power Draw" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.powerDraw"
        />
      </b-field>

      <b-field horizontal label="Power Throw" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.powerThrow"
        />
      </b-field>

      <b-field horizontal label="Athletics" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.athletics"
        />
      </b-field>

      <b-field horizontal label="Riding" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.riding"
        />
      </b-field>

      <b-field horizontal label="Weapon Master" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.weaponMaster"
        />
      </b-field>

      <b-field horizontal label="Mounted Archery" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.mountedArchery"
        />
      </b-field>

      <b-field horizontal label="Shield" class="characteristic-field">
        <b-input
          size="is-small"
          :editable="false"
          :controls="false"
          :value="characteristics.skills.shield"
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
export default class UserCharacterSkillsComponent extends Vue {
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
        points: 22,
        strength: 3,
        agility: 4,
      },
      skills: {
        points: 5,
        ironFlesh: 6,
        powerStrike: 7,
        powerDraw: 8,
        powerThrow: 2,
        athletics: 3,
        riding: 4,
        weaponMaster: 5,
        mountedArchery: 1,
        shield: 2,
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
}
</script>

<style lang="scss">
.characteristic-field {
  width: 50em;
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
