import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as clanService from '@/services/clan-service';
import Clan from '@/models/clan';

@Module({ store, dynamic: true, name: 'clan' })
class ClanModule extends VuexModule {
  clans: Clan[] = [];

  @Mutation
  setClans(clans: Clan[]) {
    this.clans = clans;
  }

  @Action({ commit: 'setClans' })
  getClans() {
    return clanService.getClans();
  }
}

export default getModule(ClanModule);
