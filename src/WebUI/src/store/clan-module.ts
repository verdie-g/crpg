import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as clanService from '@/services/clan-service';
import Clan from '@/models/clan';
import ClanCreation from '@/models/clan-creation';
import ClanWithMemberCount from '@/models/clan-with-member-count';
import ClanMemberRole from '@/models/clan-member-role';
import Region from '@/models/region';
import ClanMember from '@/models/clan-member';

@Module({ store, dynamic: true, name: 'clan' })
class ClanModule extends VuexModule {
  clans: ClanWithMemberCount[] = [];

  @Mutation
  setClans(clans: ClanWithMemberCount[]) {
    this.clans = clans;
  }

  @Action({ commit: 'setClans' })
  getClans() {
    return clanService.getClans();
  }

  @Action
  createClan(clan: ClanCreation): Promise<Clan> {
    return clanService.createClan(clan);
  }

  @Action
  updateClan({ clanId, region }: { clanId: number; region: Region }): Promise<Clan> {
    return clanService.updateClan(clanId, region);
  }

  @Action
  kickClanMember({ clanId, userId }: { clanId: number; userId: number }): Promise<void> {
    return clanService.kickClanMember(clanId, userId);
  }

  @Action
  updateClanMember({
    clanId,
    memberId,
    role,
  }: {
    clanId: number;
    memberId: number;
    role: ClanMemberRole;
  }): Promise<ClanMember> {
    return clanService.updateClanMember(clanId, memberId, role);
  }
}

export default getModule(ClanModule);
