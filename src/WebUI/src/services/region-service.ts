import Region, { TranslatedRegion } from '@/models/region';

export function getTranslatedRegions(): TranslatedRegion[] {
  return Object.values(Region)
    .map(r => {
      return { region: r, translation: r } as TranslatedRegion;
    })
    .sort((r1, r2) => (r1.translation > r2.translation ? 1 : -1));
}
