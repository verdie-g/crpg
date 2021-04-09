import Platform from './platform';
import Region from './region';
import Point from '@/models/point';

export default interface HeroVisible {
  id: number;
  platform: Platform;
  platformUserId: string;
  name: string;
  region: Region;
  troops: number;
  position: Point;
}
