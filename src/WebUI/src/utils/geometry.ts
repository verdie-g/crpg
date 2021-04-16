import { Position } from 'geojson';
import { LatLng } from 'leaflet';

export function positionToLatLng(p: Position): LatLng {
  return new LatLng(p[1], p[0]);
}
