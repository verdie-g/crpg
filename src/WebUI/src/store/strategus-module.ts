import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import SettlementPublic from '@/models/settlement-public';
import BattleDetailed from '@/models/battle-detailed';
import * as strategusService from '@/services/strategus-service';
import { arrayMergeBy } from '@/utils/array';
import Hero from '@/models/hero';
import Region from '@/models/region';
import BattleSide from '@/models/battle-side';
import Mercenaries from '@/models/mercenaries';
import Fighters from '@/models/fighters';
import BattlePhase from '@/models/battle-phase';
import HeroVisible from '@/models/hero-visible';
import StrategusUpdate from '@/models/strategus-update';
import { Result } from '@/models/result';
import HeroStatusUpdateRequest from '@/models/hero-status-update-request';

@Module({ store, dynamic: true, name: 'strategus' })
class StrategusModule extends VuexModule {
  hero: Hero | null = null;
  settlements: SettlementPublic[] = [];
  battles: BattleDetailed[] = [];
  battle: BattleDetailed | null = null;
  mercenaries: Mercenaries[] = [];
  fighters: Fighters[] = [];
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
  setBattles(battles: BattleDetailed[]) {
    this.battles = battles;
  }

  @Mutation
  setBattle(battle: BattleDetailed) {
    this.battle = battle;
  }

  @Mutation
  setMercenaries(mercenaries: Mercenaries[]) {
    this.mercenaries = mercenaries;
  }

  @Mutation
  setFighters(fighters: Fighters[]) {
    this.fighters = fighters;
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
  getBattles({
    region,
    phases,
  }: {
    region: Region;
    phases: BattlePhase[];
  }): Promise<BattleDetailed[]> {
    return strategusService.getBattles(region, phases);
  }

  @Action({ commit: 'setBattle' })
  getBattle(id: String): Promise<BattleDetailed> {
    return strategusService.getBattle(id);
  }

  @Action({ commit: 'setFighters' })
  getFighters(battleId: String): Promise<Fighters[]> {
    return strategusService.getFighters(battleId);
  }

  @Action({ commit: 'setMercenaries' })
  getMercenaries(battleId: String): Promise<Mercenaries[]> {
    return strategusService.getMercenaries(battleId);
  }

  @Action({ commit: 'setHero' })
  registerUser(region: Region): Promise<Hero> {
    return strategusService.registerUser(region);
  }

  @Action
  applyToBattleAsMercenary({battleId ,characterId, side }:{battleId: number, characterId: number, side: BattleSide}): Promise<Mercenaries[]> {
    return strategusService.applyToBattleAsMercenary(battleId ,characterId, side);
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
