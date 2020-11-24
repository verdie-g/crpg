using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Common.Helpers
{
    internal static class UserHelper
    {
        private const int DefaultGold = 300;
        private const Role DefaultRole = Role.User;
        private const int DefaultHeirloomPoints = 0;

        public static void SetDefaultValuesForUser(User user)
        {
            user.Gold = DefaultGold;
            user.Role = DefaultRole;
            user.HeirloomPoints = DefaultHeirloomPoints;
        }
    }
}