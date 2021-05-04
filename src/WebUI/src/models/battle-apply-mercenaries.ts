import Side from '@/models/side';

export default interface BattleApplyMercenaries {
  battleId: number;
  characterId: number;
  side: Side;
}
