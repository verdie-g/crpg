using Crpg.Domain.Entities;

namespace Crpg.Application.Common
{
    public class Constants
    {
        public const int StartingGold = 300;
        public const Role DefaultRole = Role.User;
        public const float SellItemRatio = 0.66f;
        public const int MinimumCharacterNameLength = 2;
        public const int MaximumCharacterNameLength = 32;
        public const int MinimumRetiringLevel = 31;
        public const float DefaultExperienceMultiplier = 1.0f;
        public const float ExperienceMultiplierIncrease = 0.05f;
    }
}