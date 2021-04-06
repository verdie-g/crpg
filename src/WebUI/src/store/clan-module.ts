import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import { get } from '@/services/crpg-client';
import ClanLite from '@/models/clan-lite';

@Module({ store, dynamic: true, name: 'clan' })
class ClanModule extends VuexModule {
  clans: ClanLite[] = [];

  @Mutation
  setClans(clans: ClanLite[]) {
    this.clans = clans;
  }

  @Action({ commit: 'setClans' })
  getClans() {
    return get('/clans');
  }
}

export default getModule(ClanModule);
