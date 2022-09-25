import Region from './region';
export default interface Clan {
  id: number;
  tag: string;
  primaryColor: string;
  secondaryColor: string;
  name: string;
  bannerKey: string;
  region: Region;
}
