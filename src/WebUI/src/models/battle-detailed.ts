import Region from '@/models/region';
import { LatLng } from 'leaflet';
import Phase from '@/models/phase';

export default interface BattleDetailed {
  id: number;
  createdAt: Date;
  region: Region;
  position: LatLng;
  phase: Phase;
}
