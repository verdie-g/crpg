import {
  Action, getModule, Module, Mutation, VuexModule,
} from 'vuex-module-decorators';
import store from '@/store';
import * as userService from '@/services/users-service';
import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';
import ItemSlot from '@/models/item-slot';
import { setCharacterItem, updateCharacterItems } from '@/services/characters-service';
import CharacterItems from '@/models/character-items';
import CharacterStatistics from '@/models/character-statistics';
import StatisticConversion from '@/models/statistic-conversion';
import Ban from '@/models/ban';
import Role from '@/models/role';
import CharacterUpdate from '@/models/character-update';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  user: User | null = null;
  ownedItems: Item[] = [];
  characters: Character[] = [];
  userBans: Ban[] = [];

  get isSignedIn(): boolean {
    return this.user !== null;
  }

  get isAdminOrSuperAdmin(): boolean {
    return this.user!.role === Role.Admin || this.user!.role === Role.SuperAdmin;
  }

  @Mutation
  setUser(user: User) {
    this.user = user;
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
  setOwnedItems(ownedItems: Item[]) {
    this.ownedItems = ownedItems;
  }

  @Mutation
  addOwnedItem(item: Item) {
    this.ownedItems.push(item);
  }

  @Mutation
  setCharacters(characters: Character[]) {
    this.characters = characters;
  }

  @Mutation
  setCharacterItem({ characterItems, slot, item }: { characterItems: CharacterItems; slot: ItemSlot; item: Item | null }) {
    setCharacterItem(characterItems, slot, item);
  }

  @Mutation
  setCharacterItemAutoRepair({ characterItems, autoRepair }: { characterItems: CharacterItems; autoRepair: boolean }) {
    characterItems.autoRepair = autoRepair;
  }

  @Mutation
  setCharacterStats({ characterId, stats }: { characterId: number; stats: CharacterStatistics }) {
    this.characters.find(c => c.id === characterId)!.statistics = stats;
  }

  @Mutation
  convertAttributeToSkills(characterId: number) {
    const character = this.characters.find(c => c.id === characterId)!;
    character.statistics.attributes.points -= 1;
    character.statistics.skills.points += 2;
  }

  @Mutation
  convertSkillsToAttribute(characterId: number) {
    const character = this.characters.find(c => c.id === characterId)!;
    character.statistics.attributes.points += 1;
    character.statistics.skills.points -= 2;
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
  updateCharacter({ characterId, characterUpdate }: { characterId: number; characterUpdate: CharacterUpdate }) {
    const character = this.characters.find(c => c.id === characterId)!;
    this.replaceCharacter({
      ...character,
      name: characterUpdate.name,
      bodyProperties: characterUpdate.bodyProperties,
      gender: characterUpdate.gender,
    });
    return userService.updateCharacter(character.id, characterUpdate);
  }

  @Action
  replaceItem({ character, slot, item }: { character: Character; slot: ItemSlot; item: Item | null }) {
    const { items } = character;
    this.setCharacterItem({ characterItems: items, slot, item });
    return updateCharacterItems(character.id, items);
  }

  @Action
  switchAutoRepair({ character, autoRepair }: { character: Character; autoRepair: boolean }) {
    this.setCharacterItemAutoRepair({ characterItems: character.items, autoRepair });
    return updateCharacterItems(character.id, character.items);
  }

  @Action
  async buyItem(item: Item) {
    await userService.buyItem(item.id);
    this.addOwnedItem(item);
    this.substractGold(item.value);
  }

  @Action({ commit: 'setCharacters' })
  getCharacters(): Promise<Character[]> {
    return userService.getCharacters();
  }

  @Action
  updateCharacterStats({ characterId, stats }: { characterId: number; stats: CharacterStatistics }): Promise<CharacterStatistics> {
    this.setCharacterStats({ characterId, stats });
    return userService.updateCharacterStats(characterId, stats);
  }

  @Action
  convertCharacterStats({ characterId, conversion }:
                          { characterId: number; conversion: StatisticConversion }):Promise<CharacterStatistics> {
    if (conversion === StatisticConversion.AttributesToSkills) {
      this.convertAttributeToSkills(characterId);
    } else if (conversion === StatisticConversion.SkillsToAttributes) {
      this.convertSkillsToAttribute(characterId);
    }

    return userService.convertCharacterStats(characterId, conversion);
  }

  @Action({ commit: 'replaceCharacter' })
  retireCharacter(character: Character): Promise<Character> {
    return userService.retireCharacter(character.id);
  }

  @Action({ commit: 'replaceCharacter' })
  respecializeCharacter(character: Character): Promise<Character> {
    return userService.respecializeCharacter(character.id);
  }

  @Action
  deleteCharacter(character: Character): Promise<void> {
    this.removeCharacter(character);
    return userService.deleteCharacter(character.id);
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
