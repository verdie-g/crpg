import Hero from './hero';
import HeroPublic from './hero-public';
import SettlementPublic from './settlement-public';

export default class StrategusUpdate {
  public hero: Hero;
  public visibleHeroes: HeroPublic[];
  public visibleSettlements: SettlementPublic[];
}
