import Platform from './platform';
import Role from './role';

export default interface User {
  id: number;
  platform: Platform;
  platformUserId: number;
  name: string;
  gold: number;
  heirloomPoints: number;
  role: Role;
  isDonor: boolean;
  avatarSmall: string;
  avatarMedium: string;
  avatarFull: string;
  activeCharacterId: number | null;
}
