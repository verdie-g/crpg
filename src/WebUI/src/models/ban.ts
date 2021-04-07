import UserPublic from '@/models/user-public';

export default class Ban {
  public id: number;
  public bannedUser: UserPublic | null;
  public duration: number;
  public reason: string;
  public bannedByUser: UserPublic;
  public createdAt: Date;
}
