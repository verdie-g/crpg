import Hero from "@/models/hero";
import Settlement from "@/models/settlement-public";
import BattleSide from "@/models/battle-side";

export default interface Fighters {
    id: number;
    settlement: Settlement;
    side: BattleSide;
    hero: Hero;
  }
