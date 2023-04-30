using Crpg.Application.Common.Services;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class UpsertUserCommandTest : TestBase
{
    [Test]
    public async Task TestWhenUserDoesntExist()
    {
        Mock<IUserService> userServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        UpsertUserCommand.Handler handler = new(ActDb, Mapper, userServiceMock.Object, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpsertUserCommand
        {
            Platform = Platform.EpicGames,
            PlatformUserId = "123",
            Name = "def",
            Avatar = new Uri("http://ghi.klm"),
        }, CancellationToken.None);

        var user = result.Data!;
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()));
        var dbUser = await AssertDb.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(dbUser, Is.Not.Null);
        Assert.That(dbUser!.DeletedAt, Is.Null);
    }

    [Test]
    public async Task TestWhenUserAlreadyExist()
    {
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "13948192759205810",
            Name = "def",
            Gold = 1000,
            Role = Role.Admin,
            Avatar = new Uri("http://ghi.klm"),
            DeletedAt = null,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IUserService> userServiceMock = new();
        UpsertUserCommand.Handler handler = new(ActDb, Mapper, userServiceMock.Object, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new UpsertUserCommand
        {
            Platform = Platform.Steam,
            PlatformUserId = "13948192759205810",
            Name = "def",
            Avatar = new Uri("http://gh.klm"),
        }, CancellationToken.None);

        var createdUser = result.Data!;
        Assert.That(createdUser.Id, Is.EqualTo(user.Id));
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()), Times.Never);

        var dbUser = await AssertDb.Users.FindAsync(user.Id);
        Assert.That(createdUser.Id, Is.EqualTo(dbUser!.Id));
        Assert.That(createdUser.PlatformUserId, Is.EqualTo(dbUser.PlatformUserId));
        Assert.That(createdUser.Gold, Is.EqualTo(dbUser.Gold));
        Assert.That(createdUser.Name, Is.EqualTo(dbUser.Name));
        Assert.That(createdUser.Role, Is.EqualTo(dbUser.Role));
        Assert.That(createdUser.Avatar, Is.EqualTo(new Uri("http://gh.klm")));
        Assert.That(dbUser.DeletedAt, Is.Null);
    }

    [Test]
    public async Task TestRecreatingUserAfterItWasDeleted()
    {
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "13948192759205810",
            DeletedAt = DateTime.Now, // Deleted user are just marked with a non-null DeletedAt
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IUserService> userServiceMock = new();
        UpsertUserCommand.Handler handler = new(ActDb, Mapper, userServiceMock.Object, Mock.Of<IActivityLogService>());
        await handler.Handle(new UpsertUserCommand
        {
            Platform = Platform.Steam,
            PlatformUserId = "13948192759205810",
        }, CancellationToken.None);

        var dbUser = await AssertDb.Users
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(dbUser.DeletedAt, Is.Null);
    }

    [Test]
    public void TestValidationValidCommand()
    {
        UpsertUserCommand.Validator validator = new();
        var res = validator.Validate(new UpsertUserCommand
        {
            Platform = Platform.Steam,
            PlatformUserId = "28320184920184918",
            Name = "toto",
            Avatar = new Uri("http://gh.klm"),
        });

        Assert.That(res.Errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestValidationInvalidCommand()
    {
        UpsertUserCommand.Validator validator = new();
        var res = validator.Validate(new UpsertUserCommand
        {
            Platform = Platform.Steam,
            PlatformUserId = "123",
            Name = string.Empty,
        });

        Assert.That(res.Errors.Count, Is.EqualTo(2));
    }
}
