import Hero from "@/models/hero";
import Settlement from "@/models/settlement-public";
import Side from "@/models/side";

export default interface Fighters {
    id: number;
    settlement: Settlement;
    side: Side;
    hero: Hero;
  }
  