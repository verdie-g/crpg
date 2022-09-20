import Region from './region';

export default interface ClanCreation {
  tag: string;
  primaryColor: number;
  secondaryColor: number;
  name: string;
  bannerKey: string;
  region: Region;
}
