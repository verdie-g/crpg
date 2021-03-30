import Platform from './platform';
import Region from './region';
import Point from '@/models/point';

export default class HeroVisible {
  public id: number;
  public platform: Platform;
  public platformUserId: string;
  public name: string;
  public region: Region;
  public troops: number;
  public position: Point;
}
