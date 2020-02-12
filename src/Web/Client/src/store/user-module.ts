import { Module, VuexModule, Mutation, Action, getModule } from 'vuex-module-decorators';
import store from '@/store';
import User from '@/models/user';
import * as userService from '@/services/users-service';

@Module({ store, dynamic: true, name: 'user' })
class UserModule extends VuexModule {
  user: User | null = null;

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
