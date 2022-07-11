import Platform from './platform';
import Region from './region';

export default interface PartyPublic {
  id: number;
  platform: Platform;
  platformUserId: string;
  name: string;
  region: Region;
}
