import { type Party, type PartyVisible } from '@/models/strategus/party';
import { type SettlementPublic } from '@/models/strategus/settlement';

export interface StrategusUpdate {
  party: Party;
  visibleParties: PartyVisible[];
  visibleSettlements: SettlementPublic[];
}

export enum MovementType {
  Move = 'Move',
  Follow = 'Follow',
  Attack = 'Attack',
}

export enum MovementTargetType {
  Party = 'Party',
  Settlement = 'Settlement',
}
