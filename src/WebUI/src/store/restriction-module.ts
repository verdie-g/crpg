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
}

export default getModule(RestrictionModules);
