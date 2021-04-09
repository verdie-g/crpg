import Platform from './platform';
import Region from './region';

export default interface HeroPublic {
  id: number;
  platform: Platform;
  platformUserId: string;
  name: string;
  region: Region;
}
