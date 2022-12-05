using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class UserServiceTest
{
    private static readonly Constants Constants = new()
    {
        DefaultGold = 300,
        DefaultRole = Role.User,
        DefaultHeirloomPoints = 0,
    };

    [Test]
    public void SetDefaultValuesShouldSetDefaultValues()
    {
        UserService userService = new(Mock.Of<IDateTime>(), Constants);
        User user = new();
        userService.SetDefaultValuesForUser(user);

        Assert.AreEqual(Constants.DefaultGold, user.Gold);
        Assert.AreEqual(Constants.DefaultRole, user.Role);
        Assert.AreEqual(Constants.DefaultHeirloomPoints, user.HeirloomPoints);
    }

    [Test]
    public void SetDefaultValuesShouldGiveGoldIsUserIsBeingCreated()
    {
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(DateTime.UtcNow);

        UserService userService = new(dateTimeMock.Object, Constants);
        User user = new() { CreatedAt = default };
        userService.SetDefaultValuesForUser(user);
        Assert.AreEqual(Constants.DefaultGold, user.Gold);
    }

    [Test]
    public void SetDefaultValuesShouldGiveGoldIfTheUserWasCreatedSomeTimeAgo()
    {
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 10, 1));

        UserService userService = new(dateTimeMock.Object, Constants);
        User user = new() { CreatedAt = new DateTime(2000, 8, 1) };
        userService.SetDefaultValuesForUser(user);
        Assert.AreEqual(Constants.DefaultGold, user.Gold);
    }

    [TestCase(100, 100)]
    [TestCase(500, 300)]
    public void SetDefaultValuesShouldNotGiveGoldIfTheUserWasCreatedRecently(int currentGold, int expectedGold)
    {
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 10, 1));

        UserService userService = new(dateTimeMock.Object, Constants);
        User user = new() { Gold = currentGold, CreatedAt = new DateTime(2000, 10, 5) };
        userService.SetDefaultValuesForUser(user);
        Assert.AreEqual(expectedGold, user.Gold);
    }
}
