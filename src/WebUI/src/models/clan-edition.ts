import Region from './region';

export interface ClanEdition {
  id: number;
  tag: string;
  primaryColor: number;
  secondaryColor: number;
  name: string;
  bannerKey: string;
  region: Region;
}

export enum ClanEditionModes {
  Create = 'Create',
  Update = 'Update',
}

export type ClanEditionMode = `${ClanEditionModes}`;
