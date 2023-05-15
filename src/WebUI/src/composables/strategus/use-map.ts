import { LMap } from '@vue-leaflet/vue-leaflet';
import { type Map, LatLngBounds, CRS, type PointExpression } from 'leaflet';
import { strategusMapHeight, strategusMapWidth } from '@root/data/constants.json';

export const useMap = () => {
  const mapOptions = {
    zoomSnap: 0.5,
    minZoom: 3,
    maxZoom: 7,
    crs: CRS.Simple,
    maxBoundsViscosity: 0.8,
    inertiaDeceleration: 2000,
    zoomControl: false,
  };

  const tileLayerOptions = {
    url: 'http://pecores.fr/gigamap/{z}/{y}/{x}.png',
    attribution:
      '<a target="_blank" href="https://www.taleworlds.com">TaleWorlds Entertainment</a>',
  };

  const center = ref<PointExpression>([-100, 125]);

  const mapBounds = ref<LatLngBounds | null>(null);

  const maxBounds = new LatLngBounds([
    [0, 0],
    [-strategusMapHeight, strategusMapWidth],
  ]);

  const onMapMoveEnd = () => {
    if (map.value === null) return;
    mapBounds.value = (map.value.leafletObject as Map).getBounds();
  };

  const zoom = ref<number>(mapOptions.minZoom);

  const map = ref<typeof LMap | null>(null);

  return {
    map,
    mapOptions,
    mapBounds,
    maxBounds,
    zoom,
    center,
    onMapMoveEnd,
    //
    tileLayerOptions,
    //
  };
};
