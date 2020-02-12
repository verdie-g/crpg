import {
  Module, VuexModule, Mutation, Action, getModule,
} from 'vuex-module-decorators';
import User from '@/models/user';
import * as userService from '@/services/users-service';
import store from './index';

@Module({ store, dynamic: true, name: 'user' })
export class UserModule extends VuexModule {
  user: User;

  @Mutation
  setUser(user: User) {
    this.user = user;
  }

  @Action({ commit: 'setUser' })
  async getUser() {
    return await userService.getUser();
  }
}

export default getModule(UserModule);
