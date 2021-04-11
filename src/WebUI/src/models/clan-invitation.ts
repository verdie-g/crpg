import ClanInvitationStatus from './clan-invitation-status';
import ClanInvitationType from './clan-invitation-type';
import UserPublic from './user-public';

export default interface ClanInvitation {
  id: number;
  invitee: UserPublic;
  inviter: UserPublic;
  type: ClanInvitationType;
  status: ClanInvitationStatus;
}
