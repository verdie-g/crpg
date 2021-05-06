import Region from '@/models/region';
import { LatLng } from 'leaflet';
import BattlePhase from '@/models/battle-phase';

export default interface BattleDetailed {
  id: number;
  createdAt: Date;
  region: Region;
  position: LatLng;
  phase: BattlePhase;
}
