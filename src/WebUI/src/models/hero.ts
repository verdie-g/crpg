import { Point } from 'geojson';
import HeroPublic from './hero-public';
import HeroStatus from './hero-status';
import Region from './region';
import SettlementPublic from './settlement-public';

export default interface Hero {
  id: number;
  name: string;
  region: Region;
  gold: number;
  troops: number;
  position: Point;
  status: HeroStatus;
  targetedHero: HeroPublic;
  targetSettlement: SettlementPublic;
}
