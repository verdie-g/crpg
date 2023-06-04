import { type User, type UserItem } from '@/models/user';
import { type Character } from '@/models/character';
import { type ClanMemberRole, type Clan } from '@/models/clan';

import { getUser, getUserItems, buyUserItem, getUserClan } from '@/services/users-service';
import { getCharacters } from '@/services/characters-service';

interface State {
  user: User | null;
  characters: Character[];
  userItems: UserItem[];
  clan: Clan | null;
  clanMemberRole: ClanMemberRole | null;
}

export const useUserStore = defineStore('user', {
  state: (): State => ({
    user: null,
    characters: [],
    userItems: [],
    clan: null,
    clanMemberRole: null,
  }),

  getters: {
    activeCharacterId: state => state.user?.activeCharacterId || state.characters?.[0]?.id || null,
  },

  actions: {
    validateCharacter(id: number) {
      return this.characters.some(c => c.id === id);
    },

    replaceCharacter(character: Character) {
      this.characters.splice(
        this.characters.findIndex(c => c.id === character.id),
        1,
        character
      );
    },

    async fetchUser() {
      this.user = await getUser();
    },

    async fetchCharacters() {
      this.characters = await getCharacters();
    },

    async fetchUserItems() {
      this.userItems = await getUserItems();
    },

    addUserItem(userItem: UserItem) {
      this.userItems.push(userItem);
    },

    subtractGold(loss: number) {
      this.user!.gold -= loss;
    },

    async buyItem(itemId: string) {
      const userItem = await buyUserItem(itemId);
      this.addUserItem(userItem);
      this.subtractGold(userItem.item.price);
    },

    async getUserClanAndRole() {
      const userClanAndRole = await getUserClan();

      if (userClanAndRole === null) {
        this.clan = null;
        this.clanMemberRole = null;
        return;
      }

      this.clan = userClanAndRole.clan;
      this.clanMemberRole = userClanAndRole.role;
    },
  },
});
