import Party from './party';
import PartyVisible from './party-visible';
import SettlementPublic from './settlement-public';

export default interface StrategusUpdate {
  party: Party;
  visibleParties: PartyVisible[];
  visibleSettlements: SettlementPublic[];
}
