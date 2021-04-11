import UserPublic from './user-public';
import ClanMemberRole from './clan-member-role';

export default interface ClanMember {
  user: UserPublic;
  role: ClanMemberRole;
}
