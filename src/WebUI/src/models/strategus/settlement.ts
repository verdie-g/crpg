import { type Point } from 'geojson';

export enum SettlementType {
  Village = 'Village',
  Castle = 'Castle',
  Town = 'Town',
}

export interface SettlementPublic {
  id: number;
  name: string;
  type: SettlementType;
  culture: string;
  position: Point;
  scene: string;
}
