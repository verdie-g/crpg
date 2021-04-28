import { Vue } from 'vue-property-decorator';
import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as userService from '@/services/users-service';
import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';
import ItemSlot from '@/models/item-slot';
import CharacterStatistics from '@/models/character-statistics';
import StatisticConversion from '@/models/statistic-conversion';
import Ban from '@/models/ban';
import Role from '@/models/role';
import CharacterUpdate from '@/models/character-update';
import EquippedItem from '@/models/equipped-item';
import Clan from '@/models/clan';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  user: User | null = null;
  userLoading = false;
  ownedItems: Item[] = [];
  clan: Clan | null = null;
  userBans: Ban[] = [];

  characters: Character[] = [];
  equippedItemsByCharacterId: { [id: number]: EquippedItem[] } = {};
  statisticsByCharacterId: { [id: number]: CharacterStatistics } = {};

  get isSignedIn(): boolean {
    return this.user !== null;
  }

  get isModeratorOrAdmin(): boolean {
    return this.user!.role === Role.Moderator || this.user!.role === Role.Admin;
  }

  get characterStatistics() {
    return (id: number) => {
      const stats = this.statisticsByCharacterId[id];
      return stats === undefined ? null : stats;
    };
  }

  get characterEquippedItems() {
    return (id: number) => {
      const equippedItems = this.equippedItemsByCharacterId[id];
      return equippedItems === undefined ? null : equippedItems;
    };
  }

  @Mutation
  setUser(user: User) {
    this.user = user;
  }

  @Mutation
  setUserLoading(userLoading: boolean) {
    this.userLoading = userLoading;
  }

  @Mutation
  resetUser() {
    this.user = null;
    this.ownedItems = [];
    this.characters = [];
  }

  @Mutation
  substractGold(loss: number) {
    this.user!.gold -= loss;
  }

  @Mutation
  addHeirloomPoints(points: number) {
    this.user!.heirloomPoints += points;
  }

  @Mutation
  setOwnedItems(ownedItems: Item[]) {
    this.ownedItems = ownedItems;
  }

  @Mutation
  addOwnedItem(item: Item) {
    this.ownedItems.push(item);
  }

  @Mutation
  removeOwnedItem(item: Item) {
    const itemIdx = this.ownedItems.findIndex(i => i.id === item.id);
    if (itemIdx !== -1) {
      this.ownedItems.splice(itemIdx, 1);
    }
  }

  @Mutation
  setCharacters(characters: Character[]) {
    this.characters = characters;
  }

  @Mutation
  setCharacterEquippedItems({
    characterId,
    items,
  }: {
    characterId: number;
    items: EquippedItem[];
  }) {
    Vue.set(this.equippedItemsByCharacterId, characterId, items);
  }

  @Mutation
  setCharacterItem({
    characterId,
    slot,
    item,
  }: {
    characterId: number;
    slot: ItemSlot;
    item: Item | null;
  }) {
    const characterEquippedItems = this.equippedItemsByCharacterId[characterId];
    const equippedItemIdx = characterEquippedItems.findIndex(ei => ei.slot === slot);
    if (equippedItemIdx === -1) {
      if (item !== null) {
        characterEquippedItems.push({ slot, item });
      }
    } else if (item !== null) {
      characterEquippedItems[equippedItemIdx].item = item;
    } else {
      characterEquippedItems.splice(equippedItemIdx, 1);
    }
  }

  @Mutation
  replaceCharactersItem({ toReplace, replaceWith }: { toReplace: Item; replaceWith: Item }) {
    Object.values(this.equippedItemsByCharacterId).forEach(characterEquippedItems => {
      characterEquippedItems.forEach(equippedItem => {
        if (equippedItem.item.id === toReplace.id) {
          equippedItem.item = replaceWith;
        }
      });
    });
  }

  @Mutation
  setCharacterAutoRepair({ character, autoRepair }: { character: Character; autoRepair: boolean }) {
    character.autoRepair = autoRepair;
  }

  @Mutation
  setCharacterStatistics({
    characterId,
    stats,
  }: {
    characterId: number;
    stats: CharacterStatistics;
  }) {
    Vue.set(this.statisticsByCharacterId, characterId, stats);
  }

  @Mutation
  convertAttributeToSkills(characterId: number) {
    const stats = this.statisticsByCharacterId[characterId];
    stats.attributes.points -= 1;
    stats.skills.points += 2;
  }

  @Mutation
  convertSkillsToAttribute(characterId: number) {
    const stats = this.statisticsByCharacterId[characterId];
    stats.attributes.points += 1;
    stats.skills.points -= 2;
  }

  @Mutation
  replaceCharacter(character: Character) {
    const idx = this.characters.findIndex(c => c.id === character.id);
    this.characters.splice(idx, 1, character);
  }

  @Mutation
  removeCharacter(character: Character) {
    const idx = this.characters.findIndex(c => c.id === character.id);
    this.characters.splice(idx, 1);
  }

  @Mutation
  setUserClan(clan: Clan) {
    this.clan = clan;
  }

  @Mutation
  setUserBans(bans: Ban[]) {
    this.userBans = bans;
  }

  @Action({ commit: 'setUser' })
  getUser() {
    return userService.getUser();
  }

  @Action({ commit: 'setOwnedItems' })
  getOwnedItems() {
    return userService.getOwnedItems();
  }

  @Action
  updateCharacter({
    characterId,
    characterUpdate,
  }: {
    characterId: number;
    characterUpdate: CharacterUpdate;
  }) {
    const character = this.characters.find(c => c.id === characterId)!;
    this.replaceCharacter({
      ...character,
      name: characterUpdate.name,
    });
    return userService.updateCharacter(character.id, characterUpdate);
  }

  @Action
  replaceItem({
    character,
    slot,
    item,
  }: {
    character: Character;
    slot: ItemSlot;
    item: Item | null;
  }): Promise<EquippedItem[]> {
    this.setCharacterItem({ characterId: character.id, slot, item });
    return userService.updateCharacterItems(character.id, [
      { itemId: item === null ? null : item.id, slot },
    ]);
  }

  @Action
  switchAutoRepair({ character, autoRepair }: { character: Character; autoRepair: boolean }) {
    this.setCharacterAutoRepair({ character, autoRepair });
    return userService.switchCharacterAutoRepair(character.id, autoRepair);
  }

  @Action
  async buyItem(item: Item) {
    await userService.buyItem(item.id);
    this.addOwnedItem(item);
    this.substractGold(item.value);
  }

  @Action
  async upgradeItem(item: Item) {
    const upgradedItem = await userService.upgradeItem(item.id);
    this.addHeirloomPoints(-1);
    this.replaceCharactersItem({ toReplace: item, replaceWith: upgradedItem });
    this.removeOwnedItem(item);
  }

  @Action({ commit: 'setCharacters' })
  getCharacters(): Promise<Character[]> {
    return userService.getCharacters();
  }

  @Action
  async getCharacterStatistics(characterId: number): Promise<void> {
    const stats = await userService.getCharacterStatistics(characterId);
    this.setCharacterStatistics({ characterId, stats });
  }

  @Action
  async getCharacterItems(characterId: number): Promise<void> {
    const items = await userService.getCharacterItems(characterId);
    this.setCharacterEquippedItems({ characterId, items });
  }

  @Action
  updateCharacterStatistics({
    characterId,
    stats,
  }: {
    characterId: number;
    stats: CharacterStatistics;
  }): Promise<CharacterStatistics> {
    this.setCharacterStatistics({ characterId, stats });
    return userService.updateCharacterStatistics(characterId, stats);
  }

  @Action
  convertCharacterStatistics({
    characterId,
    conversion,
  }: {
    characterId: number;
    conversion: StatisticConversion;
  }): Promise<CharacterStatistics> {
    if (conversion === StatisticConversion.AttributesToSkills) {
      this.convertAttributeToSkills(characterId);
    } else if (conversion === StatisticConversion.SkillsToAttributes) {
      this.convertSkillsToAttribute(characterId);
    }

    return userService.convertCharacterStatistics(characterId, conversion);
  }

  @Action
  async retireCharacter(character: Character): Promise<void> {
    this.addHeirloomPoints(1);
    character = await userService.retireCharacter(character.id);
    this.replaceCharacter(character);
    this.setCharacterEquippedItems({ characterId: character.id, items: [] });
    const stats = await userService.getCharacterStatistics(character.id);
    this.setCharacterStatistics({ characterId: character.id, stats });
  }

  @Action
  async respecializeCharacter(character: Character): Promise<void> {
    character = await userService.respecializeCharacter(character.id);
    this.replaceCharacter(character);
    this.setCharacterEquippedItems({ characterId: character.id, items: [] });
    const stats = await userService.getCharacterStatistics(character.id);
    this.setCharacterStatistics({ characterId: character.id, stats });
  }

  @Action
  deleteCharacter(character: Character): Promise<void> {
    this.removeCharacter(character);
    return userService.deleteCharacter(character.id);
  }

  @Action({ commit: 'setUserClan' })
  getUserClan(): Promise<Clan | null> {
    return userService.getUserClan();
  }

  @Action({ commit: 'setUserBans' })
  getUserBans(): Promise<Ban[]> {
    return userService.getUserBans();
  }

  @Action
  deleteUser(): Promise<void> {
    this.resetUser();
    return userService.deleteUser();
  }
}

export default getModule(UserModule);
