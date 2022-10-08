import Region from './region';

export default interface ClanDTO {
  id: number;
  tag: string;
  primaryColor: number;
  secondaryColor: number;
  name: string;
  bannerKey: string;
  region: Region;
}
