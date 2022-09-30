import { Vue } from 'vue-property-decorator';
import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as userService from '@/services/users-service';
import * as itemService from '@/services/item-service';
import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';
import ItemSlot from '@/models/item-slot';
import CharacterCharacteristics from '@/models/character-characteristics';
import CharacterStatistics from '@/models/character-statistics';
import CharacteristicConversion from '@/models/characteristic-conversion';
import Restriction from '@/models/restriction';
import Role from '@/models/role';
import CharacterUpdate from '@/models/character-update';
import EquippedItem from '@/models/equipped-item';
import Clan from '@/models/clan';
import UserItem from '@/models/user-item';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  user: User | null = null;
  userLoading = false;
  userItems: UserItem[] = [];
  clan: Clan | null = null;
  userRestrictions: Restriction[] = [];

  characters: Character[] = [];
  equippedItemsByCharacterId: { [id: number]: EquippedItem[] } = {};
  characteristicsByCharacterId: { [id: number]: CharacterCharacteristics } = {};
  statisticsByCharacterId: { [id: number]: CharacterStatistics } = {};

  get isSignedIn(): boolean {
    return this.user !== null;
  }

  get isModeratorOrAdmin(): boolean {
    return this.user!.role === Role.Moderator || this.user!.role === Role.Admin;
  }

  get characterCharacteristics() {
    return (id: number) => {
      const characteristics = this.characteristicsByCharacterId[id];
      return characteristics === undefined ? null : characteristics;
    };
  }

  get characterStatistics() {
    return (id: number) => {
      const statistics = this.statisticsByCharacterId[id];
      return statistics === undefined ? null : statistics;
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
    this.userItems = [];
    this.characters = [];
  }

  @Mutation
  substractGold(loss: number) {
    this.user!.gold -= loss;
  }

  @Mutation
  addGold(gain: number) {
    this.user!.gold += gain;
  }

  @Mutation
  addHeirloomPoints(points: number) {
    this.user!.heirloomPoints += points;
  }

  @Mutation
  setUserItems(userItems: UserItem[]) {
    this.userItems = userItems;
  }

  @Mutation
  addUserItem(userItem: UserItem) {
    this.userItems.push(userItem);
  }

  @Mutation
  removeUserItem(userItem: UserItem) {
    const itemIdx = this.userItems.findIndex(ui => ui.id === userItem.id);
    if (itemIdx !== -1) {
      this.userItems.splice(itemIdx, 1);
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
    userItem,
  }: {
    characterId: number;
    slot: ItemSlot;
    userItem: UserItem | null;
  }) {
    const characterEquippedItems = this.equippedItemsByCharacterId[characterId];
    const equippedItemIdx = characterEquippedItems.findIndex(ei => ei.slot === slot);
    if (equippedItemIdx === -1) {
      if (userItem !== null) {
        characterEquippedItems.push({ slot, userItem });
      }
    } else if (userItem !== null) {
      characterEquippedItems[equippedItemIdx].userItem = userItem;
    } else {
      characterEquippedItems.splice(equippedItemIdx, 1);
    }
  }

  @Mutation
  replaceCharactersItem({
    toReplace,
    replaceWith,
  }: {
    toReplace: UserItem;
    replaceWith: UserItem;
  }) {
    Object.values(this.equippedItemsByCharacterId).forEach(characterEquippedItems => {
      characterEquippedItems.forEach(equippedItem => {
        if (equippedItem.userItem.id === toReplace.id) {
          equippedItem.userItem = replaceWith;
        }
      });
    });
  }

  @Mutation
  removeCharactersItem(toRemove: UserItem) {
    Object.values(this.equippedItemsByCharacterId).forEach(characterEquippedItems => {
      const equippedItemsToRemove = characterEquippedItems.filter(
        equippedItem => equippedItem.userItem.id === toRemove.id
      );
      equippedItemsToRemove.forEach(item => {
        const index = characterEquippedItems.indexOf(item);
        if (index !== -1) characterEquippedItems.splice(index, 1);
      });
    });
  }

  @Mutation
  setCharacterAutoRepair({ character, autoRepair }: { character: Character; autoRepair: boolean }) {
    character.autoRepair = autoRepair;
  }

  @Mutation
  setCharacterCharacteristics({
    characterId,
    characteristics,
  }: {
    characterId: number;
    characteristics: CharacterCharacteristics;
  }) {
    Vue.set(this.characteristicsByCharacterId, characterId, characteristics);
  }

  @Mutation
  setCharacterStatistics({
    characterId,
    statistics,
  }: {
    characterId: number;
    statistics: CharacterStatistics;
  }) {
    Vue.set(this.statisticsByCharacterId, characterId, statistics);
  }

  @Mutation
  convertAttributeToSkills(characterId: number) {
    const characteristics = this.characteristicsByCharacterId[characterId];
    characteristics.attributes.points -= 1;
    characteristics.skills.points += 2;
  }

  @Mutation
  convertSkillsToAttribute(characterId: number) {
    const characteristics = this.characteristicsByCharacterId[characterId];
    characteristics.attributes.points += 1;
    characteristics.skills.points -= 2;
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
  setUserRestrictions(restrictions: Restriction[]) {
    this.userRestrictions = restrictions;
  }

  @Action({ commit: 'setUser' })
  getUser() {
    return userService.getUser();
  }

  @Action({ commit: 'setUserItems' })
  getUserItems() {
    return userService.getUserItems();
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
    userItem,
  }: {
    character: Character;
    slot: ItemSlot;
    userItem: UserItem | null;
  }): Promise<EquippedItem[]> {
    this.setCharacterItem({ characterId: character.id, slot, userItem });
    return userService.updateCharacterItems(character.id, [
      { userItemId: userItem === null ? null : userItem.id, slot },
    ]);
  }

  @Action
  switchAutoRepair({ character, autoRepair }: { character: Character; autoRepair: boolean }) {
    this.setCharacterAutoRepair({ character, autoRepair });
    return userService.switchCharacterAutoRepair(character.id, autoRepair);
  }

  @Action
  async buyItem(item: Item) {
    const userItem = await userService.buyItem(item.id);
    this.addUserItem(userItem);
    this.substractGold(item.price);
  }

  @Action
  async upgradeUserItem(userItem: UserItem) {
    const upgradedUserItem = await userService.upgradeUserItem(userItem.id);
    this.addUserItem(upgradedUserItem);
    this.addHeirloomPoints(-1);
    this.replaceCharactersItem({ toReplace: userItem, replaceWith: upgradedUserItem });
    this.removeUserItem(userItem);
  }

  @Action
  async sellUserItem(userItem: UserItem): Promise<number> {
    await userService.sellUserItem(userItem.id);
    this.removeCharactersItem(userItem);
    this.removeUserItem(userItem);
    const salePrice = itemService.computeSalePrice(userItem);
    this.addGold(salePrice);
    return salePrice;
  }

  @Action({ commit: 'setCharacters' })
  getCharacters(): Promise<Character[]> {
    return userService.getCharacters();
  }

  @Action
  async getCharacterCharacteristics(characterId: number): Promise<void> {
    const characteristics = await userService.getCharacterCharacteristics(characterId);
    this.setCharacterCharacteristics({ characterId, characteristics });
  }

  @Action
  async getCharacterStatistics(characterId: number): Promise<void> {
    const statistics = await userService.getCharacterStatistics(characterId);
    this.setCharacterStatistics({ characterId, statistics });
  }

  @Action
  async getCharacterItems(characterId: number): Promise<void> {
    const items = await userService.getCharacterItems(characterId);
    this.setCharacterEquippedItems({ characterId, items });
  }

  @Action
  updateCharacterCharacteristics({
    characterId,
    characteristics,
  }: {
    characterId: number;
    characteristics: CharacterCharacteristics;
  }): Promise<CharacterCharacteristics> {
    this.setCharacterCharacteristics({ characterId, characteristics: characteristics });
    return userService.updateCharacterCharacteristics(characterId, characteristics);
  }

  @Action
  convertCharacterCharacteristics({
    characterId,
    conversion,
  }: {
    characterId: number;
    conversion: CharacteristicConversion;
  }): Promise<CharacterCharacteristics> {
    if (conversion === CharacteristicConversion.AttributesToSkills) {
      this.convertAttributeToSkills(characterId);
    } else if (conversion === CharacteristicConversion.SkillsToAttributes) {
      this.convertSkillsToAttribute(characterId);
    }

    return userService.convertCharacterCharacteristics(characterId, conversion);
  }

  @Action
  async retireCharacter(character: Character): Promise<void> {
    this.addHeirloomPoints(1);
    character = await userService.retireCharacter(character.id);
    this.replaceCharacter(character);
    this.setCharacterEquippedItems({ characterId: character.id, items: [] });
    const characteristics = await userService.getCharacterCharacteristics(character.id);
    this.setCharacterCharacteristics({ characterId: character.id, characteristics });
  }

  @Action
  async respecializeCharacter(character: Character): Promise<void> {
    character = await userService.respecializeCharacter(character.id);
    this.replaceCharacter(character);
    this.setCharacterEquippedItems({ characterId: character.id, items: [] });
    const characteristics = await userService.getCharacterCharacteristics(character.id);
    this.setCharacterCharacteristics({ characterId: character.id, characteristics });
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

  @Action({ commit: 'setUserRestrictions' })
  getUserRestrictions(): Promise<Restriction[]> {
    return userService.getUserRestrictions();
  }

  @Action
  deleteUser(): Promise<void> {
    this.resetUser();
    return userService.deleteUser();
  }
}

export default getModule(UserModule);
