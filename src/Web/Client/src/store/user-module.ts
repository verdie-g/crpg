import {
  Action, getModule, Module, Mutation, VuexModule,
} from 'vuex-module-decorators';
import store from '@/store';
import * as userService from '@/services/users-service';
import User from '@/models/user';
import Character from '@/models/character';
import Item from '@/models/item';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  user: User | null = null;
  ownedItems: Item[] = [];
  characters: Character[] = [];

  @Mutation
  setUser(user: User) {
    this.user = user;
  }

  @Mutation
  substractGold(loss: number) {
    this.user!.gold -= loss;
  }

  @Mutation
  resetUser() {
    this.user = null;
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

  @Action({ commit: 'setUser' })
  getUser() {
    return userService.getUser();
  }

  @Action({ commit: 'setOwnedItems' })
  getOwnedItems() {
    return userService.getOwnedItems();
  }

  @Action
  async buyItem(item: Item) {
    await userService.buyItem(item.id);
    this.addOwnedItem(item);
    this.substractGold(item.value);
  }

  @Action({ commit: 'setCharacters' })
  getCharacters() {
    return userService.getCharacters();
  }
}

export default getModule(UserModule);
