// TODO: SPEC
import { Region } from '@/models/region';

export const getRegions = () => Object.keys(Region) as Array<keyof Region>;
