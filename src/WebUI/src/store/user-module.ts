import {
  Action, getModule, Module, Mutation, VuexModule,
} from 'vuex-module-decorators';
import store from '@/store';
import * as userService from '@/services/users-service';
import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';
import ItemSlot from '@/models/item-slot';
import { setCharacterItem } from '@/services/characters-service';
import CharacterItems from '@/models/character-items';
import CharacterStatistics from '@/models/character-statistics';
import StatisticConversion from '@/models/statistic-conversion';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  isSignedIn = false;
  user: User | null = null;
  ownedItems: Item[] = [];
  characters: Character[] = [];

  @Mutation
  signIn() {
    this.isSignedIn = true;
  }

  @Mutation
  signOut() {
    this.isSignedIn = false;
    this.user = null;
    this.ownedItems = [];
    this.characters = [];
  }

  @Mutation
  setUser(user: User) {
    this.user = user;
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
  setCharacterItem({ characterItems, slot, item }: { characterItems: CharacterItems; slot: ItemSlot; item: Item }) {
    setCharacterItem(characterItems, slot, item);
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

  @Action({ commit: 'setUser' })
  getUser() {
    return userService.getUser();
  }

  @Action({ commit: 'setOwnedItems' })
  getOwnedItems() {
    return userService.getOwnedItems();
  }

  @Action
  renameCharacter({ character, newName }: { character: Character; newName: string }) {
    this.replaceCharacter({
      ...character,
      name: newName,
    });
    return userService.updateCharacter(character.id, { name: newName });
  }

  @Action
  replaceItem({ character, slot, item }: { character: Character; slot: ItemSlot; item: Item }) {
    const { items } = character;
    this.setCharacterItem({ characterItems: items, slot, item });
    return userService.updateCharacterItems(character.id, {
      headItemId: items.headItem?.id ?? null,
      capeItemId: items.capeItem?.id ?? null,
      bodyItemId: items.bodyItem?.id ?? null,
      handItemId: items.handItem?.id ?? null,
      legItemId: items.legItem?.id ?? null,
      horseHarnessItemId: items.horseHarnessItem?.id ?? null,
      horseItemId: items.horseItem?.id ?? null,
      weapon1ItemId: items.weapon1Item?.id ?? null,
      weapon2ItemId: items.weapon2Item?.id ?? null,
      weapon3ItemId: items.weapon3Item?.id ?? null,
      weapon4ItemId: items.weapon4Item?.id ?? null,
    });
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
  convertCharacterStats({ characterId, conversion }: { characterId: number; conversion: StatisticConversion }): Promise<CharacterStatistics> {
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
}

export default getModule(UserModule);
