import { Position } from 'geojson';
import { LatLng } from 'leaflet';

export const positionToLatLng = (p: Position) => new LatLng(p[1], p[0]);
