using Crpg.Domain.Entities;

namespace Crpg.Application.Common.Helpers
{
    internal static class UserHelper
    {
        private const int DefaultGold = 300;
        private const Role DefaultRole = Role.User;

        public static void SetDefaultValuesForNewUser(User user)
        {
            user.Gold = DefaultGold;
            user.Role = DefaultRole;
        }
    }
}