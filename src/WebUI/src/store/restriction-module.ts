import RestrictionType from '@/models/restriction-type'
import { restrictUser } from '@/services/restriction-service'
import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as restrictionService from '@/services/restriction-service';
import Restriction from '@/models/restriction';

@Module({ store, dynamic: true, name: 'restriction' })
class RestrictionModules extends VuexModule {
  restrictions: Restriction[] = [];

  @Mutation
  setRestrictions(restrictions: Restriction[]) {
    this.restrictions = restrictions;
  }

  @Action({ commit: 'setRestrictions' })
  getRestrictions() {
    return restrictionService.getRestrictions();
  }

  @Action
  createRestriction({
    restrictedUserId,
    type,
    reason,
    duration,
  }: {
    restrictedUserId: number;
    type: RestrictionType;
    reason: string;
    duration?: number;
  }) {
    return restrictionService.restrictUser(restrictedUserId, type, reason, duration);
  }
}

export default getModule(RestrictionModules);
