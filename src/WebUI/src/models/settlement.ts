import Point from '@/models/point';
import SettlementType from '@/models/settlement-type';

export default class Settlement {
    name: string;
    type: SettlementType;
    culture: string;
    position: Point;
    scene: string
  }
