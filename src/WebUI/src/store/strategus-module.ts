import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import SettlementPublic from '@/models/settlement-public';
import * as strategusService from '@/services/strategus-service';
import { arrayMergeBy } from '@/utils/array';
import Party from '@/models/party';
import Region from '@/models/region';
import PartyVisible from '@/models/party-visible';
import StrategusUpdate from '@/models/strategus-update';
import { Result } from '@/models/result';
import PartyStatusUpdateRequest from '@/models/party-status-update-request';

@Module({ store, dynamic: true, name: 'strategus' })
class StrategusModule extends VuexModule {
  party: Party | null = null;
  settlements: SettlementPublic[] = [];
  visibleParties: PartyVisible[] = [];

  currentDialog: string | null = null;

  @Mutation
  setParty(party: Party) {
    this.party = party;
  }

  @Mutation
  setSettlements(settlements: SettlementPublic[]) {
    this.settlements = settlements;
  }

  @Mutation
  setVisibleParties(parties: PartyVisible[]) {
    this.visibleParties = parties;
  }

  @Mutation
  pushDialog(dialogComponent: string) {
    this.currentDialog = dialogComponent;
  }

  @Mutation
  popDialog() {
    this.currentDialog = null;
  }

  @Action({ commit: 'setSettlements' })
  getSettlements(): Promise<SettlementPublic> {
    return strategusService.getSettlements();
  }

  @Action({ commit: 'setParty' })
  registerUser(region: Region): Promise<Party> {
    return strategusService.registerUser(region);
  }

  @Action
  async getUpdate(): Promise<Result<StrategusUpdate>> {
    const res = await strategusService.getUpdate();
    if (res.errors !== null) {
      return res;
    }

    const update = res.data!;
    this.setParty(update.party);
    this.setSettlements(arrayMergeBy(this.settlements, update.visibleSettlements, s => s.id));
    this.setVisibleParties(update.visibleParties);

    return res;
  }

  @Action
  async updatePartyStatus(update: PartyStatusUpdateRequest) {
    const party = await strategusService.updatePartyStatus(update);
    this.setParty(party);
  }
}

export default getModule(StrategusModule);
