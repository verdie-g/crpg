import { MultiPoint } from 'geojson';
import PartyStatus from './party-status';

export default interface PartyStatusUpdateRequest {
  status: PartyStatus;
  waypoints: MultiPoint;
  targetedPartyId: number;
  targetedSettlementId: number;
}
