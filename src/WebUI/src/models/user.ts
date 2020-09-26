import Role from './role';

export default class User {
  public id: number;

  public platformId: number;

  public name: string;

  public gold: number;

  public role: Role;

  public avatarSmall: string;

  public avatarMedium: string;

  public avatarFull: string;
}
