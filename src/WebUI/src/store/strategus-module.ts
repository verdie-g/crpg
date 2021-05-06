import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import SettlementPublic from '@/models/settlement-public';
import BattlePublic from '@/models/battle-detailed';
import * as strategusService from '@/services/strategus-service';
import { arrayMergeBy } from '@/utils/array';
import Hero from '@/models/hero';
import Region from '@/models/region';
import Phase from '@/models/phase';
import HeroVisible from '@/models/hero-visible';
import StrategusUpdate from '@/models/strategus-update';
import { Result } from '@/models/result';
import HeroStatusUpdateRequest from '@/models/hero-status-update-request';

@Module({ store, dynamic: true, name: 'strategus' })
class StrategusModule extends VuexModule {
  hero: Hero | null = null;
  settlements: SettlementPublic[] = [];
  battles: BattlePublic[] = [];
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
  setBattles(battles: BattlePublic[]) {
    this.battles = battles;
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

  @Action({ commit: 'setBattles' })
  getBattles({ region, phases }: { region: Region, phases: Phase[] }): Promise<BattlePublic[]> {
    return strategusService.getBattles(region, phases);
  }

  @Action({ commit: 'setHero' })
  registerUser(region: Region): Promise<Hero> {
    return strategusService.registerUser(region);
  }

  @Action
  async getUpdate(): Promise<Result<StrategusUpdate>> {
    const res = await strategusService.getUpdate();
    if (res.errors !== null) {
      return res;
    }

    const update = res.data!;
    this.setHero(update.hero);
    this.setSettlements(arrayMergeBy(this.settlements, update.visibleSettlements, s => s.id));
    this.setVisibleHeroes(update.visibleHeroes);

    return res;
  }

  @Action
  async updateHeroStatus(update: HeroStatusUpdateRequest) {
    const hero = await strategusService.updateHeroStatus(update);
    this.setHero(hero);
  }
}

export default getModule(StrategusModule);
