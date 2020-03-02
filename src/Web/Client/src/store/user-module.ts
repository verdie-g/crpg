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

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  isSignedIn: boolean = false;
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
  setCharacterItem({ character, slot, item } : { character: Character, slot: ItemSlot, item: Item }) {
    setCharacterItem(character, slot, item);
  }

  @Mutation
  replaceCharacter(character: Character) {
    const idx = this.characters.findIndex(c => c.id === character.id);
    this.characters.splice(idx, 1, character);
  }

  @Action({ commit: 'setUser' })
  getUser() {
    return userService.getUser();
  }

  @Action({ commit: 'setOwnedItems' })
  getOwnedItems() {
    return userService.getOwnedItems();
  }

  @Action({ commit: 'replaceCharacter' })
  replaceItem({ character, slot, item } : { character: Character, slot: ItemSlot, item: Item }) {
    this.setCharacterItem({ character, slot, item });
    return userService.setItems(character.id, {
      headItemId: character.headItem !== null ? character.headItem!.id : null,
      capeItemId: character.capeItem !== null ? character.capeItem!.id : null,
      bodyItemId: character.bodyItem !== null ? character.bodyItem!.id : null,
      handItemId: character.handItem !== null ? character.handItem!.id : null,
      legItemId: character.legItem !== null ? character.legItem!.id : null,
      horseHarnessItemId: character.horseHarnessItem !== null ? character.horseHarnessItem!.id : null,
      horseItemId: character.horseItem !== null ? character.horseItem!.id : null,
      weapon1ItemId: character.weapon1Item !== null ? character.weapon1Item!.id : null,
      weapon2ItemId: character.weapon2Item !== null ? character.weapon2Item!.id : null,
      weapon3ItemId: character.weapon3Item !== null ? character.weapon3Item!.id : null,
      weapon4ItemId: character.weapon4Item !== null ? character.weapon4Item!.id : null,
    });
  }

  @Action
  async buyItem(item: Item) {
    await userService.buyItem(item.id);
    this.addOwnedItem(item);
    this.substractGold(item.value);
  }

  @Action({ commit: 'setCharacters' })
  getCharacters() : Promise<Character[]> {
    return userService.getCharacters();
  }
}

export default getModule(UserModule);
