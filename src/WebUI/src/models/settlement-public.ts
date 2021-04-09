import Point from '@/models/point';
import SettlementType from '@/models/settlement-type';

export default interface SettlementPublic {
  id: number;
  name: string;
  type: SettlementType;
  culture: string;
  position: Point;
  scene: string;
}
