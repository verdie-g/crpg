import {
  Action, getModule, Module, Mutation, VuexModule,
} from 'vuex-module-decorators';
import store from '@/store';
import * as userService from '@/services/users-service';
import User from '@/models/user';
import Character from '@/models/character';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  user: User | null = null;

  characters: Character[] = [];

  @Mutation
  setUser(user: User) {
    this.user = user;
  }

  @Mutation
  resetUser() {
    this.user = null;
  }

  @Mutation
  setCharacters(characters: Character[]) {
    this.characters = characters;
  }

  @Action({ commit: 'setUser' })
  getUser() {
    return userService.getUser();
  }

  @Action({ commit: 'setCharacters' })
  getCharacters() {
    return userService.getCharacters();
  }
}

export default getModule(UserModule);
