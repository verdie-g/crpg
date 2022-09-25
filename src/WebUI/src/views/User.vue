<template>
  <section v-if="user != null" class="section">
    <div class="columns">
      <div class="column is-one-fifth is-narrow" style="width: 300px">
        <b-menu v-if="characters.length && !isPrivateProfile">
          <b-menu-list>
            <b-menu-item
              v-for="character in characters"
              v-bind:key="character.id"
              :active="selectedCharacter && character.id === selectedCharacter.id"
              :label="`${character.name} (lvl ${character.level})`"
              @click="selectedCharacter = character"
            />
          </b-menu-list>
        </b-menu>
      </div>
      <div class="column">
        <div class="columns is-vcentered">
          <img v-bind:src="user.avatarMedium" alt="avatar" />
          <h1 class="column is-size-2">{{ user.name }}</h1>
        </div>
        <h1 class="column is-size-5">{{ profileText }}</h1>
        <h1 class="column is-size-5" v-if="isPrivateProfile">This profile is private.</h1>

        <b-tabs v-model="activeTab" v-if="this.characters.length !== 0">
          <b-tab-item v-if="privacyShowSkills" label="Attributes">
            <user-character-skills-component :character="selectedCharacter" />
          </b-tab-item>
          <b-tab-item v-if="privacyShowItems" label="Items">
            <user-character-items-component :character="selectedCharacter" />
          </b-tab-item>
          <b-tab-item v-if="privacyShowWeaponProficiencies" label="Weapon Proficiencies">
            <user-character-weapon-proficiencies-component :character="selectedCharacter" />
          </b-tab-item>
        </b-tabs>
      </div>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import User from '@/models/user';
import * as userService from '@/services/users-service';
import UserCharacterSkillsComponent from '@/components/user/UserCharacterSkillsComponent.vue';
import UserCharacterItemsComponent from '@/components/user/UserCharacterItemsComponent.vue';
import UserCharacterWeaponProficienciesComponent from '@/components/user/UserCharacterWeaponProficienciesComponent.vue';
import Character from '@/models/character';

@Component({
  components: {
    UserCharacterSkillsComponent,
    UserCharacterItemsComponent,
    UserCharacterWeaponProficienciesComponent,
  },
})
export default class UserComponent extends Vue {
  user: User | null = null;
  characters: Character[] = [];

  async beforeMount() {
    const tab = parseInt(this.$route.query.tab as string);
    if (!tab || Number.isNaN(tab)) {
      this.activeTab = 0;
    }

    const userId = parseInt(this.$route.params.id as string);
    if (Number.isNaN(userId)) {
      this.$router.push({ name: 'not-found' });
      return;
    }

    const user = await userService.getUserById(userId);
    // TODO remove
    user.userProfile = {
      id: 123112,
      privacyShowSkills: true,
      privacyShowItems: true,
      privacyShowWeaponProficiencies: true,
      text: 'Frank junge',
    };
    this.user = user;

    // TODO
    // this.characters = await userService.getCharactersByUserId(user.id);
    this.characters = [];
    for (let index = 0; index < 15; index++) {
      this.characters.push({
        id: index,
        name: 'unrated-' + index,
        generation: 50 + index,
        level: 20 + index,
        experience: 500 + index,
        autoRepair: false,
      });
    }
  }

  get activeTab(): number {
    const tab = parseInt(this.$route.query.tab as string);
    if (tab && Number.isNaN(tab)) {
      return 0;
    }
    return tab;
  }

  set activeTab(tab: number) {
    this.$router.replace({ name: 'user', query: { ...this.$route.query, tab: tab.toString() } });
  }

  get selectedCharacter(): Character | null {
    const characterId = parseInt(this.$route.query.charId as string);
    if (characterId && Number.isNaN(characterId)) {
      return null;
    }

    if (characterId) {
      return this.characters.find(char => char.id === characterId) || null;
    } else {
      if (this.characters.length > 0) return this.characters[0];
    }
    return null;
  }

  set selectedCharacter(selectedCharacter: Character | null) {
    if (!selectedCharacter) return;
    if (this.selectedCharacter?.id === selectedCharacter.id) return;
    this.$router.replace({
      name: 'user',
      query: { ...this.$route.query, charId: selectedCharacter.id.toString() },
    });
  }

  get privacyShowSkills() {
    if (!this.user) return true;
    return this.user.userProfile.privacyShowSkills;
  }

  get privacyShowItems() {
    if (!this.user) return true;
    return this.user.userProfile.privacyShowItems;
  }

  get privacyShowWeaponProficiencies() {
    if (!this.user) return true;
    return this.user.userProfile.privacyShowWeaponProficiencies;
  }

  get profileText() {
    if (!this.user) return true;
    return this.user.userProfile.text;
  }

  get isPrivateProfile() {
    if (!this.user) return true;
    return (
      !this.user.userProfile.privacyShowSkills &&
      !this.user.userProfile.privacyShowItems &&
      !this.user.userProfile.privacyShowWeaponProficiencies
    );
  }
}
</script>

<style scoped lang="scss"></style>
