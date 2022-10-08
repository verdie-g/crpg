import Region from './region';

export default interface ClanEdition {
  id: number;
  tag: string;
  primaryColor: number;
  secondaryColor: number;
  name: string;
  bannerKey: string;
  region: Region;
}
