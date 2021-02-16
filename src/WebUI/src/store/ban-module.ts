import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as banService from '@/services/ban-service';
import Ban from '@/models/ban';

@Module({ store, dynamic: true, name: 'ban' })
class BanModule extends VuexModule {
  bans: Ban[] = [];

  @Mutation
  setBans(bans: Ban[]) {
    this.bans = bans;
  }

  @Action({ commit: 'setBans' })
  getBans() {
    return banService.getBans();
  }
}

export default getModule(BanModule);
