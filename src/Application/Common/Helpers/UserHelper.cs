using Crpg.Domain.Entities;

namespace Crpg.Application.Common.Helpers
{
    internal static class UserHelper
    {
        private const int DefaultGold = 300;
        private const Role DefaultRole = Role.User;
        private const int DefaultLoomPoints = 0;

        public static void SetDefaultValuesForUser(User user)
        {
            user.Gold = DefaultGold;
            user.Role = DefaultRole;
            user.LoomPoints = 0;
        }
    }
}