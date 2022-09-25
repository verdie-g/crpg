export enum Region {
  Europe = 'Europe',
  NorthAmerica = 'NorthAmerica',
  Asia = 'Asia',
}
export interface TranslatedRegion {
  region: Region;
  translation: string;
}

export default Region;
