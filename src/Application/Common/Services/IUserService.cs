using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Common logic for characters.
    /// </summary>
    internal interface IUserService
    {
        void SetDefaultValuesForUser(User user);
    }

    /// <inheritdoc />
    internal class UserService : IUserService
    {
        private readonly Constants _constants;

        public UserService(Constants constants)
        {
            _constants = constants;
        }

        public void SetDefaultValuesForUser(User user)
        {
            user.Gold = _constants.DefaultGold;
            user.Role = _constants.DefaultRole;
            user.HeirloomPoints = _constants.DefaultHeirloomPoints;
        }
    }
}
