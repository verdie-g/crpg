import Platform from './platform';
import Region from './region';

export default class HeroPublic {
  public id: number;
  public platform: Platform;
  public platformUserId: string;
  public name: string;
  public region: Region;
}
