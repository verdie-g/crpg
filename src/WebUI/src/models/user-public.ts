import Platform from './platform';

export default interface UserPublic {
  id: number;
  platform: Platform;
  platformUserId: number;
  name: string;
}
