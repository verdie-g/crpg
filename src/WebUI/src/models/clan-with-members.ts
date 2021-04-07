import ClanMember from './clan-member';

export default class ClanWithMembers {
  public id: number;
  public tag: string;
  public name: string;
  public members: ClanMember[];
}
