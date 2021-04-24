import SettlementType from '@/models/settlement-type';
import { Point } from 'geojson';

export default interface SettlementPublic {
  id: number;
  name: string;
  type: SettlementType;
  culture: string;
  position: Point;
  scene: string;
}
