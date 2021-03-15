import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import Settlement from '@/models/settlement';
import * as strategusService from '@/services/strategus-service';

@Module({ store, dynamic: true, name: 'strategus' })
class strategusModule extends VuexModule {
  settlements: Settlement[] = [];

  @Mutation
  setSettlements(settlements: Settlement[]) {
    this.settlements = settlements;
  }

  @Action({ commit: 'setSettlements' })
  getSettlements() {
    return strategusService.getSettlements();
  }
}

export default getModule(strategusModule);
