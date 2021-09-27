namespace Crpg.GameMod.Api.Models.Strategus
{
    internal class CrpgSettlementCreation
    {
        public string Name { get; set; } = default!;
        public CrpgSettlementType Type { get; set; }
        public CrpgCulture Culture { get; set; }
        public Point Position { get; set; } = default!;
        public string Scene { get; set; } = default!;
    }

    // Copy of Crpg.Domain.Entities.Settlements.SettlementType.
    internal enum CrpgSettlementType
    {
        Village,
        Castle,
        Town,
    }
}
