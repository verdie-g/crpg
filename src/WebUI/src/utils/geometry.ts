import Point from '@/models/point';
import { LatLng } from 'leaflet';

export function pointToLatLng(p: Point): LatLng {
  return new LatLng(p.coordinates[1], p.coordinates[0]);
}
