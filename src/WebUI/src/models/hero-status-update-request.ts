import { MultiPoint } from 'geojson';
import HeroStatus from './hero-status';

export default interface HeroStatusUpdateRequest {
  status: HeroStatus;
  waypoints: MultiPoint;
  targetedHeroId: number;
  targetedSettlementId: number;
}
