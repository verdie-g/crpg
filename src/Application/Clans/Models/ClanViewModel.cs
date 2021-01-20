using System.Collections.Generic;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models
{
    public class ClanViewModel : IMapFrom<Clan>
    {
        public int Id { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IList<ClanMemberViewModel> Members { get; set; } = new List<ClanMemberViewModel>();
    }
}
