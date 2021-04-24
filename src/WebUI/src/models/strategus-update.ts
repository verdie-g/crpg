import Hero from './hero';
import HeroVisible from './hero-visible';
import SettlementPublic from './settlement-public';

export default interface StrategusUpdate {
  hero: Hero;
  visibleHeroes: HeroVisible[];
  visibleSettlements: SettlementPublic[];
}
