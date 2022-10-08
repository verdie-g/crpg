import Region from '@/models/region';

export const regionToStr: Record<Region, string> = {
  [Region.Europe]: 'Europe',
  [Region.NorthAmerica]: 'North America',
  [Region.Asia]: 'Asia',
};

export const regionIcons: Record<Region, string> = {
  [Region.Europe]: 'globe-europe',
  [Region.NorthAmerica]: 'globe-americas',
  [Region.Asia]: 'globe-asia',
};
