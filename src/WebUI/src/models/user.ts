import Platform from './platform';
import Role from './role';

export default class User {
  public id: number;
  public platform: Platform;
  public platformUserId: number;
  public name: string;
  public gold: number;
  public role: Role;
  public avatarSmall: string;
  public avatarMedium: string;
  public avatarFull: string;
}
