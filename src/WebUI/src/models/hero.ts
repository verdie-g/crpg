import HeroPublic from './hero-public';
import HeroStatus from './hero-status';
import Point from './point';
import Region from './region';
import SettlementPublic from './settlement-public';

export default class Hero {
  public id: number;
  public name: string;
  public region: Region;
  public gold: number;
  public troops: number;
  public position: Point;
  public status: HeroStatus;
  public targetedHero: HeroPublic;
  public targetSettlement: SettlementPublic;
}
