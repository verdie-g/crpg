import RestrictionType from '@/models/restriction-type';

export default interface RestrictionCreation {
  restrictedUserId: number;
  type: RestrictionType;
  reason: string;
  duration: number;
}
