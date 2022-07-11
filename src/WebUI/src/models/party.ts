import { MultiPoint, Point } from 'geojson';
import PartyStatus from './party-status';
import PartyVisible from './party-visible';
import Region from './region';
import SettlementPublic from './settlement-public';

export default interface Party {
  id: number;
  name: string;
  region: Region;
  gold: number;
  troops: number;
  position: Point;
  status: PartyStatus;
  waypoints: MultiPoint;
  targetedParty: PartyVisible;
  targetedSettlement: SettlementPublic;
}
