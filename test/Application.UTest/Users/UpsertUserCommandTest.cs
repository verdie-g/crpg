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
        UpsertUserCommand.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        var result = await handler.Handle(new UpsertUserCommand
        {
            PlatformUserId = "123",
            Name = "def",
            Avatar = new Uri("http://ghi.klm"),
            AvatarMedium = new Uri("http://mno.pqr"),
            AvatarFull = new Uri("http://stu.vwx"),
        }, CancellationToken.None);

        var user = result.Data!;
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()));
        var dbUser = await AssertDb.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.IsNotNull(dbUser);
        Assert.IsNull(dbUser!.DeletedAt);
    }

    [Test]
    public async Task TestWhenUserAlreadyExist()
    {
        User user = new()
        {
            PlatformUserId = "13948192759205810",
            Name = "def",
            Gold = 1000,
            Role = Role.Admin,
            AvatarSmall = new Uri("http://ghi.klm"),
            AvatarMedium = new Uri("http://mno.pqr"),
            AvatarFull = new Uri("http://stu.vwx"),
            DeletedAt = null,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IUserService> userServiceMock = new();
        UpsertUserCommand.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        var result = await handler.Handle(new UpsertUserCommand
        {
            PlatformUserId = "13948192759205810",
            Name = "def",
            Avatar = new Uri("http://gh.klm"),
            AvatarMedium = new Uri("http://mn.pqr"),
            AvatarFull = new Uri("http://st.vwx"),
        }, CancellationToken.None);

        var createdUser = result.Data!;
        Assert.AreEqual(user.Id, createdUser.Id);
        userServiceMock.Verify(us => us.SetDefaultValuesForUser(It.IsAny<User>()), Times.Never);

        var dbUser = await AssertDb.Users.FindAsync(user.Id);
        Assert.AreEqual(dbUser!.Id, createdUser.Id);
        Assert.AreEqual(dbUser.PlatformUserId, createdUser.PlatformUserId);
        Assert.AreEqual(dbUser.Gold, createdUser.Gold);
        Assert.AreEqual(dbUser.Name, createdUser.Name);
        Assert.AreEqual(dbUser.Role, createdUser.Role);
        Assert.AreEqual(new Uri("http://gh.klm"), createdUser.AvatarSmall);
        Assert.AreEqual(new Uri("http://mn.pqr"), createdUser.AvatarMedium);
        Assert.AreEqual(new Uri("http://st.vwx"), createdUser.AvatarFull);
        Assert.IsNull(dbUser.DeletedAt);
    }

    [Test]
    public async Task TestRecreatingUserAfterItWasDeleted()
    {
        User user = new()
        {
            PlatformUserId = "13948192759205810",
            DeletedAt = DateTime.Now, // Deleted user are just marked with a non-null DeletedAt
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IUserService> userServiceMock = new();
        UpsertUserCommand.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        await handler.Handle(new UpsertUserCommand
        {
            PlatformUserId = "13948192759205810",
        }, CancellationToken.None);

        var dbUser = await AssertDb.Users.FindAsync(user.Id);
        Assert.IsNull(dbUser!.DeletedAt);
    }

    [Test]
    public void TestValidationValidCommand()
    {
        var validator = new UpsertUserCommand.Validator();
        var res = validator.Validate(new UpsertUserCommand
        {
            PlatformUserId = "28320184920184918",
            Name = "toto",
            Avatar = new Uri("http://gh.klm"),
            AvatarMedium = new Uri("http://mn.pqr"),
            AvatarFull = new Uri("http://st.vwx"),
        });

        Assert.AreEqual(0, res.Errors.Count);
    }

    [Test]
    public void TestValidationInvalidCommand()
    {
        UpsertUserCommand.Validator validator = new();
        var res = validator.Validate(new UpsertUserCommand
        {
            PlatformUserId = "123",
            Name = string.Empty,
        });

        Assert.AreEqual(4, res.Errors.Count);
    }
}
