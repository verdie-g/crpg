using System;
using System.Collections.Generic;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal class Wave
    {
        public IList<WaveGroup> Groups { get; set; } = Array.Empty<WaveGroup>();
    }

    internal class WaveGroup
    {
        public string CharacterId { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}