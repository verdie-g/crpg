import UserPublic from '@/models/user-public';
import RestrictionType from '@/models/restriction-type';

export default interface Restriction {
  id: number;
  restrictedUser: UserPublic | null;
  duration: number;
  type: RestrictionType;
  reason: string;
  restrictedByUser: UserPublic;
  createdAt: Date;
}

export interface RestrictionWithActive extends Restriction {
  active: boolean;
}
