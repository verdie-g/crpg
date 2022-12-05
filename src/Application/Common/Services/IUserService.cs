using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;

namespace Crpg.Application.Common.Services;

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
    private readonly IDateTime _dateTime;
    private readonly Constants _constants;

    public UserService(IDateTime dateTime, Constants constants)
    {
        _dateTime = dateTime;
        _constants = constants;
    }

    public void SetDefaultValuesForUser(User user)
    {
        user.Gold = user.CreatedAt == default || user.CreatedAt + TimeSpan.FromDays(30) < _dateTime.UtcNow
            ? _constants.DefaultGold
            : Math.Min(_constants.DefaultGold, user.Gold);
        user.Role = _constants.DefaultRole;
        user.HeirloomPoints = _constants.DefaultHeirloomPoints;
    }
}
