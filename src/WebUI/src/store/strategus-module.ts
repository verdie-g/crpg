import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import SettlementPublic from '@/models/settlement-public';
import * as strategusService from '@/services/strategus-service';
import { arrayMergeBy } from '@/utils/array';
import Hero from '@/models/hero';
import Region from '@/models/region';
import HeroVisible from '@/models/hero-visible';

@Module({ store, dynamic: true, name: 'strategus' })
class StrategusModule extends VuexModule {
  hero: Hero | null = null;
  settlements: SettlementPublic[] = [];
  visibleHeroes: HeroVisible[] = [];

  currentDialog: string | null = null;

  @Mutation
  setHero(hero: Hero) {
    this.hero = hero;
  }

  @Mutation
  setSettlements(settlements: SettlementPublic[]) {
    this.settlements = settlements;
  }

  @Mutation
  setVisibleHeroes(heroes: HeroVisible[]) {
    this.visibleHeroes = heroes;
  }

  @Mutation
  pushDialog(dialogComponent: string) {
    this.currentDialog = dialogComponent;
  }

  @Mutation
  popDialog() {
    this.currentDialog = null;
  }

  @Action({ commit: 'setSettlements' })
  getSettlements(): Promise<SettlementPublic> {
    return strategusService.getSettlements();
  }

  @Action({ commit: 'setHero' })
  registerUser(region: Region): Promise<Hero> {
    return strategusService.registerUser(region);
  }

  @Action
  async getUpdate(): Promise<void> {
    const res = await strategusService.getUpdate();
    if (res.errors !== null && res.errors[0].code === 'HeroNotFound') {
      this.pushDialog('RegistrationDialog');
      return;
    }

    const update = res.data!;
    this.setHero(update.hero);
    this.setSettlements(arrayMergeBy(this.settlements, update.visibleSettlements, s => s.id));
    this.setVisibleHeroes(update.visibleHeroes);
  }
}

export default getModule(StrategusModule);
