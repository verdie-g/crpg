import { MultiPoint, Point } from 'geojson';
import HeroStatus from './hero-status';
import HeroVisible from './hero-visible';
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
  waypoints: MultiPoint;
  targetedHero: HeroVisible;
  targetedSettlement: SettlementPublic;
}
