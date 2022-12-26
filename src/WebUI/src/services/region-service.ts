import Region from '@/models/region';

export const regionToStr: Record<Region, string> = {
  [Region.Eu]: 'Europe',
  [Region.Na]: 'North America',
  [Region.As]: 'Asia',
};

export const regionIcons: Record<Region, string> = {
  [Region.Eu]: 'globe-europe',
  [Region.Na]: 'globe-americas',
  [Region.As]: 'globe-asia',
};
