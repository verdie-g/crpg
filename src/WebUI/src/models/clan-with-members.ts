import ClanMember from './clan-member';

export default interface ClanWithMembers {
  id: number;
  tag: string;
  color: string;
  name: string;
  members: ClanMember[];
}
