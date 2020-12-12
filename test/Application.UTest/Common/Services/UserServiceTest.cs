using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class UserServiceTest
    {
        private static readonly Constants Constants = new Constants
        {
            DefaultGold = 300,
            DefaultRole = Role.User,
            DefaultHeirloomPoints = 0,
        };

        [Test]
        public void SetDefaultValuesShouldSetDefaultValues()
        {
            var userService = new UserService(Constants);
            var user = new User();
            userService.SetDefaultValuesForUser(user);

            Assert.AreEqual(Constants.DefaultGold, user.Gold);
            Assert.AreEqual(Constants.DefaultRole, user.Role);
            Assert.AreEqual(Constants.DefaultHeirloomPoints, user.HeirloomPoints);
        }
    }
}
