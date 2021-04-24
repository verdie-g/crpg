import UserPublic from '@/models/user-public';

export default interface Ban {
  id: number;
  bannedUser: UserPublic | null;
  duration: number;
  reason: string;
  bannedByUser: UserPublic;
  createdAt: Date;
}
