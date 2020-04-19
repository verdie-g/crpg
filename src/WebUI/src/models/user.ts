import Role from './role';

export default class User {
  public id: number;

  public steamId: number;

  public userName: string;

  public gold: number;

  public role: Role;

  public avatarSmall: string;

  public avatarMedium: string;

  public avatarFull: string;
}
