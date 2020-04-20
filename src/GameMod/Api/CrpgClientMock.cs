using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Requests;
using Crpg.GameMod.Api.Responses;

namespace Crpg.GameMod.Api
{
    public class CrpgClientMock : ICrpgClient
    {
        public Task<GetUserResponse> GetOrCreateUser(GetUserRequest req, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new GetUserResponse
            {
                Id = 1,
                Character = new GameCharacter
                {
                    Id = 1,
                    Name = "toto",
                    Level = 1,
                    Experience = 100,
                    NextLevelExperience = 300,
                    /*HeadItemMbId = "mp_wrapped_desert_cap",
                    BodyItemMbId = "mp_aserai_civil_e",
                    LegItemMbId = "mp_strapped_shoes",
                    Weapon1ItemMbId = "mp_aserai_axe",
                    Weapon2ItemMbId = "mp_throwing_stone",*/
                }
            });
        }

        public Task<TickResponse> Tick(TickRequest req, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TickResponse { Users = Array.Empty<TickUserResponse>() });
        }
    }
}