using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.System.Commands
{
    public record SeedDataCommand : IMediatorRequest
    {
        internal class Handler : IMediatorRequestHandler<SeedDataCommand>
        {
            private static readonly int[] ItemRanks = { -3, -2, -1, 1, 2, 3 };
            private readonly ICrpgDbContext _db;
            private readonly IItemsSource _itemsSource;
            private readonly IApplicationEnvironment _appEnv;
            private readonly ICharacterService _characterService;
            private readonly IExperienceTable _experienceTable;
            private readonly ItemValueModel _itemValueModel;
            private readonly ItemModifierService _itemModifierService;

            public Handler(ICrpgDbContext db, IItemsSource itemsSource, IApplicationEnvironment appEnv,
                ICharacterService characterService, IExperienceTable experienceTable, ItemValueModel itemValueModel,
                ItemModifierService itemModifierService)
            {
                _db = db;
                _itemsSource = itemsSource;
                _appEnv = appEnv;
                _characterService = characterService;
                _experienceTable = experienceTable;
                _itemValueModel = itemValueModel;
                _itemModifierService = itemModifierService;
            }

            public async Task<Result> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                if (_appEnv.Environment == HostingEnvironment.Development)
                {
                    await AddDevelopmentData();
                }

                await CreateOrUpdateItems(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return new Result();
            }

            private async Task AddDevelopmentData()
            {
                User takeo = new()
                {
                    PlatformUserId = "76561197987525637",
                    Name = "takeoshigeru",
                    Gold = 30000,
                    HeirloomPoints = 2,
                    Role = Role.Admin,
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_medium.jpg"),
                    Characters = new List<Character>
                    {
                        new()
                        {
                            Name = "takeoshigeru",
                            Generation = 2,
                            Level = 23,
                            Experience = _experienceTable.GetExperienceForLevel(23),
                        },
                        new()
                        {
                            Name = "totoalala",
                            Level = 12,
                            Experience = _experienceTable.GetExperienceForLevel(12),
                        },
                        new()
                        {
                            Name = "Retire me",
                            Level = 31,
                            Experience = _experienceTable.GetExperienceForLevel(31) + 100,
                        },
                    },
                };
                User namidaka = new()
                {
                    PlatformUserId = "76561197979511363",
                    Name = "Namidaka",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/70/703178fb540263bd30d5b84562b1167985603273.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/70/703178fb540263bd30d5b84562b1167985603273_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/70/703178fb540263bd30d5b84562b1167985603273_full.jpg"),
                };
                User laHire = new()
                {
                    PlatformUserId = "76561198012340299",
                    Name = "LaHire",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/31/31f7c86313e48dd924c08844f1cb2dd76e542a46.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/31/31f7c86313e48dd924c08844f1cb2dd76e542a46_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/31/31f7c86313e48dd924c08844f1cb2dd76e542a46_full.jpg"),
                };
                User elmaryk = new()
                {
                    PlatformUserId = "76561197972800560",
                    Name = "Elmaryk",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/05/059f27b9bdf15392d8b0114d8d106bd430398cf2.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/05/059f27b9bdf15392d8b0114d8d106bd430398cf2_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/05/059f27b9bdf15392d8b0114d8d106bd430398cf2_full.jpg"),
                };
                User azuma = new()
                {
                    PlatformUserId = "76561198081821029",
                    Name = "Azuma",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/57/57eab4bf98145304377078d0a3d73dc05d540714.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/57/57eab4bf98145304377078d0a3d73dc05d540714_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/57/57eab4bf98145304377078d0a3d73dc05d540714_full.jpg"),
                };
                User zorguy = new()
                {
                    PlatformUserId = "76561197989897581",
                    Name = "Zorguy",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e1/e12361889a18f7e834447bd96b9389943200f693.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e1/e12361889a18f7e834447bd96b9389943200f693_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e1/e12361889a18f7e834447bd96b9389943200f693_full.jpg"),
                };
                User neostralie = new()
                {
                    PlatformUserId = "76561197992190847",
                    Name = "Neostralie",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/50/50696c5fc162251193044d50e84956a60b9b9750.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/50/50696c5fc162251193044d50e84956a60b9b9750_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/50/50696c5fc162251193044d50e84956a60b9b9750_full.jpg"),
                };
                User ecko = new()
                {
                    PlatformUserId = "76561198003849595",
                    Name = "Ecko",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b22b63e50e6148d446735f9d10b53be3dbe8114a.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b22b63e50e6148d446735f9d10b53be3dbe8114a_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b22b63e50e6148d446735f9d10b53be3dbe8114a_full.jpg"),
                };
                User firebat = new()
                {
                    PlatformUserId = "76561198034738782",
                    Name = "Firebat",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/80/80cfe380953ec4b9c8c09c36b22278263c47f506.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/80/80cfe380953ec4b9c8c09c36b22278263c47f506_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/80/80cfe380953ec4b9c8c09c36b22278263c47f506_full.jpg"),
                };
                User sellka = new()
                {
                    PlatformUserId = "76561197979977620", Name = "Sellka",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63_full.jpg"),
                };
                User leanir = new()
                {
                    PlatformUserId = "76561198018585047", Name = "Laenir",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d_full.jpg"),
                };
                User opset = new() // Grey
                {
                    PlatformUserId = "76561198009970770",
                    Name = "Opset_the_Grey",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81_full.jpg"),
                };
                User falcom = new() // OdE
                {
                    PlatformUserId = "76561197963438590",
                    Name = "[OdE]Falcom",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ff/ffbc4f2f33a16d764ce9aeb92495c05421738834.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ff/ffbc4f2f33a16d764ce9aeb92495c05421738834_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ff/ffbc4f2f33a16d764ce9aeb92495c05421738834_full.jpg"),
                };
                User brainfart = new()
                {
                    PlatformUserId = "76561198007258336", Name = "Brainfart",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/06/06be92280c028dbf83951ccaa7857d1b46f50401.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/06/06be92280c028dbf83951ccaa7857d1b46f50401_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/06/06be92280c028dbf83951ccaa7857d1b46f50401_full.jpg"),
                };
                User kiwi = new()
                {
                    PlatformUserId = "76561198050263436",
                    Name = "Kiwi",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b1/b1eeebf4b5eaf0d0fd255e7bfd88dddac53a79b7.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b1/b1eeebf4b5eaf0d0fd255e7bfd88dddac53a79b7_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b1/b1eeebf4b5eaf0d0fd255e7bfd88dddac53a79b7_full.jpg"),
                };
                User ikarooz = new()
                {
                    PlatformUserId = "76561198013940874",
                    Name = "Ikarooz",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fd9de1adbc5a2d7d9f6f43905663051d1f3ad6b.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fd9de1adbc5a2d7d9f6f43905663051d1f3ad6b_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fd9de1adbc5a2d7d9f6f43905663051d1f3ad6b_full.jpg"),
                };
                User bryggan = new()
                {
                    PlatformUserId = "76561198076068057",
                    Name = "Bryggan",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7b0ba5b51367b8e667bac7be347c4b194e46c42.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7b0ba5b51367b8e667bac7be347c4b194e46c42_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7b0ba5b51367b8e667bac7be347c4b194e46c42_full.jpg"),
                };
                User schumetzq = new()
                {
                    PlatformUserId = "76561198050714825",
                    Name = "Schumetzq",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/02/02fd365a5cd57ab2a09ada405546c7e1732e6e09.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/02/02fd365a5cd57ab2a09ada405546c7e1732e6e09_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/02/02fd365a5cd57ab2a09ada405546c7e1732e6e09_full.jpg"),
                };
                User victorhh888 = new()
                {
                    PlatformUserId = "76561197968139412",
                    Name = "victorhh888",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/90/90fb01f63a3b68a4a6f06208c84cc03250f4786e.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/90/90fb01f63a3b68a4a6f06208c84cc03250f4786e_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/90/90fb01f63a3b68a4a6f06208c84cc03250f4786e_full.jpg"),
                };
                User distance = new()
                {
                    PlatformUserId = "76561198874880658",
                    Name = "远方",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d1/d18e1efd0df9440d21a820e3f37ebfc57a2b9ed4.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d1/d18e1efd0df9440d21a820e3f37ebfc57a2b9ed4_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d1/d18e1efd0df9440d21a820e3f37ebfc57a2b9ed4_full.jpg"),
                };
                User bakhrat = new()
                {
                    PlatformUserId = "76561198051386592",
                    Name = "bakhrat 22hz",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f3/f3b2fbe95be2dfe6f3f2d5ceaca04d75a1a81966.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f3/f3b2fbe95be2dfe6f3f2d5ceaca04d75a1a81966_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f3/f3b2fbe95be2dfe6f3f2d5ceaca04d75a1a81966_full.jpg"),
                };
                User lancelot = new()
                {
                    PlatformUserId = "76561198015772903",
                    Name = "Lancelot",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e9/e9cb98a2cd5facedca0982a52eb47f37142c3555.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e9/e9cb98a2cd5facedca0982a52eb47f37142c3555_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e9/e9cb98a2cd5facedca0982a52eb47f37142c3555_full.jpg"),
                };
                User buddha = new()
                {
                    PlatformUserId = "76561198036356550",
                    Name = "Buddha.dll",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fab01b855c8e9704f0239fa716d182ad96e3ff8.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fab01b855c8e9704f0239fa716d182ad96e3ff8_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fab01b855c8e9704f0239fa716d182ad96e3ff8_full.jpg"),
                };
                User lerch = new()
                {
                    PlatformUserId = "76561197988504032",
                    Name = "Lerch_77",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c0/c0d5345e5592f47aeee066e73f27d884496e75e1.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c0/c0d5345e5592f47aeee066e73f27d884496e75e1_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c0/c0d5345e5592f47aeee066e73f27d884496e75e1_full.jpg"),
                };
                User tjens = new()
                {
                    PlatformUserId = "76561197997439945",
                    Name = "Tjens",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce5524c76a12dff71e0c02b3220907597ded1aca.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce5524c76a12dff71e0c02b3220907597ded1aca_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce5524c76a12dff71e0c02b3220907597ded1aca_full.jpg"),
                };
                User knitler = new()
                {
                    PlatformUserId = "76561198034120910",
                    Name = "Knitler",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a1174ff1fdc31ff8078511e16a73d9caeee4675b.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a1174ff1fdc31ff8078511e16a73d9caeee4675b_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a1174ff1fdc31ff8078511e16a73d9caeee4675b_full.jpg"),
                };
                User magnuclean = new()
                {
                    PlatformUserId = "76561198044343808",
                    Name = "Magnuclean",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/8a/8a7486e99e489a7e1f7ad356ab2dd4892e4e908e.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/8a/8a7486e99e489a7e1f7ad356ab2dd4892e4e908e_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/8a/8a7486e99e489a7e1f7ad356ab2dd4892e4e908e_full.jpg"),
                };
                User baronCyborg = new()
                {
                    PlatformUserId = "76561198026044780",
                    Name = "Baron Cyborg",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/58/5838cfcd99e280d82f63d92472d6d5aecebfb812.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/58/5838cfcd99e280d82f63d92472d6d5aecebfb812_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/58/5838cfcd99e280d82f63d92472d6d5aecebfb812_full.jpg"),
                };

                User[] newUsers =
                {
                    takeo, baronCyborg, magnuclean, knitler, tjens, lerch, buddha, lancelot, bakhrat, distance,
                    victorhh888, schumetzq, bryggan, ikarooz, kiwi, brainfart, falcom, opset, leanir, sellka, firebat,
                    ecko, neostralie, zorguy, azuma, elmaryk, namidaka, laHire,
                };

                var existingUsers = await _db.Users.ToDictionaryAsync(u => (u.Platform, u.PlatformUserId));
                foreach (var newUser in newUsers)
                {
                    foreach (var character in newUser.Characters)
                    {
                        _characterService.ResetCharacterStats(character, respecialization: true);
                    }

                    if (existingUsers.TryGetValue((newUser.Platform, newUser.PlatformUserId), out var existingUser))
                    {
                        _db.Entry(existingUser).State = EntityState.Detached;

                        newUser.Id = existingUser.Id;
                        _db.Users.Update(newUser);
                    }
                    else
                    {
                        _db.Users.Add(newUser);
                    }
                }

                Clan pecores = new()
                {
                    Tag = "PEC",
                    Color = "#3273DC",
                    Name = "Pecores",
                    Members =
                    {
                        new ClanMember { Role = ClanMemberRole.Leader, User = namidaka },
                        new ClanMember { Role = ClanMemberRole.Admin, User = neostralie },
                        new ClanMember { Role = ClanMemberRole.Admin, User = elmaryk },
                        new ClanMember { Role = ClanMemberRole.Member, User = laHire },
                        new ClanMember { Role = ClanMemberRole.Member, User = azuma },
                        new ClanMember { Role = ClanMemberRole.Member, User = zorguy },
                    },
                };
                Clan ats = new()
                {
                    Tag = "ATS",
                    Color = "#FF3860",
                    Name = "Among The Shadows",
                    Members =
                    {
                        new ClanMember { Role = ClanMemberRole.Leader, User = ecko },
                        new ClanMember { Role = ClanMemberRole.Admin, User = firebat },
                        new ClanMember { Role = ClanMemberRole.Member, User = sellka },
                    },
                };
                Clan legio = new()
                {
                    Tag = "LEG",
                    Color = "#FFDD57",
                    Name = "Legio",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = leanir } },
                };
                Clan theGrey = new()
                {
                    Tag = "GREY",
                    Color = "#7A7A7A",
                    Name = "The Grey",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = opset } },
                };
                Clan ode = new()
                {
                    Tag = "OdE",
                    Color = "#00D1B2",
                    Name = "Ordre de l'étoile",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = falcom } },
                };
                Clan virginDefenders = new()
                {
                    Tag = "VD",
                    Color = "#FF7D97",
                    Name = "Virgin Defenders",
                    Members =
                    {
                        new ClanMember { Role = ClanMemberRole.Leader, User = brainfart },
                        new ClanMember { Role = ClanMemberRole.Admin, User = kiwi },
                        new ClanMember { Role = ClanMemberRole.Member, User = ikarooz },
                        new ClanMember { Role = ClanMemberRole.Member, User = bryggan },
                        new ClanMember { Role = ClanMemberRole.Member, User = schumetzq },
                    },
                };
                Clan randomClan = new()
                {
                    Tag = "RC",
                    Color = "#F5F5F5",
                    Name = "Random Clan",
                    Members =
                    {
                        new ClanMember { Role = ClanMemberRole.Leader, User = victorhh888 },
                        new ClanMember { Role = ClanMemberRole.Admin, User = distance },
                        new ClanMember { Role = ClanMemberRole.Member, User = bakhrat },
                    },
                };
                Clan abcClan = new()
                {
                    Tag = "ABC",
                    Color = "#5D3C43",
                    Name = "ABC",
                    Members =
                    {
                        new ClanMember { Role = ClanMemberRole.Leader, User = lancelot },
                        new ClanMember { Role = ClanMemberRole.Member, User = buddha },
                    },
                };
                Clan defClan = new()
                {
                    Tag = "DEF",
                    Color = "#65B1A6",
                    Name = "DEF",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = lerch } },
                };
                Clan ghiClan = new()
                {
                    Tag = "GHI",
                    Color = "#1A544C",
                    Name = "GHI",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = tjens } },
                };
                Clan jklClan = new()
                {
                    Tag = "JKL",
                    Color = "#10044F",
                    Name = "JKL",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = knitler } },
                };
                Clan mnoClan = new()
                {
                    Tag = "MNO",
                    Color = "#5A541C",
                    Name = "MNO",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = magnuclean } },
                };
                Clan pqrClan = new()
                {
                    Tag = "PQR",
                    Color = "#123456",
                    Name = "Plan QR",
                    Members = { new ClanMember { Role = ClanMemberRole.Leader, User = baronCyborg } },
                };
                Clan[] newClans =
                {
                    pecores, ats, legio, theGrey, ode, virginDefenders, randomClan, abcClan, defClan, ghiClan, jklClan,
                    mnoClan, pqrClan,
                };

                var existingClans = await _db.Clans.ToDictionaryAsync(c => c.Name);
                foreach (var newClan in newClans)
                {
                    if (existingClans.TryGetValue(newClan.Name, out var existingClan))
                    {
                        _db.Entry(existingClan).State = EntityState.Detached;

                        newClan.Id = existingClan.Id;
                        _db.Clans.Update(newClan);
                    }
                    else
                    {
                        _db.Clans.Add(newClan);
                    }
                }

                ClanInvitation schumetzqRequestForPecores = new()
                {
                    Clan = pecores,
                    Invitee = schumetzq,
                    Inviter = schumetzq,
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Pending,
                };
                ClanInvitation neostralieOfferToBrygganForPecores = new()
                {
                    Clan = pecores,
                    Inviter = neostralie,
                    Invitee = bryggan,
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Pending,
                };
                ClanInvitation[] newClanInvitations = { schumetzqRequestForPecores, neostralieOfferToBrygganForPecores };
                var existingClanInvitations =
                    await _db.ClanInvitations.ToDictionaryAsync(i => (i.InviterId, i.InviteeId));
                foreach (var newClanInvitation in newClanInvitations)
                {
                    if (existingClanInvitations.TryGetValue((newClanInvitation.InviteeId, newClanInvitation.InviteeId),
                        out var existingClanInvitation))
                    {
                        _db.Entry(existingClanInvitation).State = EntityState.Detached;

                        newClanInvitation.Id = existingClanInvitation.Id;
                        _db.ClanInvitations.Update(newClanInvitation);
                    }
                    else
                    {
                        _db.ClanInvitations.Add(newClanInvitation);
                    }
                }
            }

            private async Task CreateOrUpdateItems(CancellationToken cancellationToken)
            {
                var itemsByMdId = (await _itemsSource.LoadItems())
                    .ToDictionary(i => i.TemplateMbId);
                var dbItemsByMbId = await _db.Items
                    .ToDictionaryAsync(di => (di.TemplateMbId, di.Rank), cancellationToken);

                var baseItems = new List<Item>();

                foreach (ItemCreation item in itemsByMdId.Values)
                {
                    Item baseItem = ItemCreationToItem(item);
                    baseItem.Value = _itemValueModel.ComputeItemValue(baseItem);
                    // EF Core doesn't support creating an entity referencing itself, which is needed for items with
                    // rank = 0. Workaround is to set BaseItemId to null and replace with the reference to the item
                    // once it was created. This is the only reason why BaseItemId is nullable.
                    baseItem.BaseItemId = null;
                    CreateOrUpdateItem(dbItemsByMbId, baseItem);
                    baseItems.Add(baseItem);

                    foreach (int rank in ItemRanks)
                    {
                        var modifiedItem = _itemModifierService.ModifyItem(baseItem, rank);
                        modifiedItem.BaseItem = baseItem;
                        CreateOrUpdateItem(dbItemsByMbId, modifiedItem);
                    }
                }

                // Remove items that were deleted from the item source
                foreach (Item dbItem in dbItemsByMbId.Values)
                {
                    if (dbItem.Rank != 0 || itemsByMdId.ContainsKey(dbItem.TemplateMbId))
                    {
                        continue;
                    }

                    var ownedItems = await _db.OwnedItems
                        .Include(oi => oi.User)
                        .Include(oi => oi.Item)
                        .Where(oi => oi.Item!.BaseItemId == dbItem.BaseItemId)
                        .ToArrayAsync(cancellationToken);
                    foreach (var ownedItem in ownedItems)
                    {
                        ownedItem.User!.Gold += ownedItem.Item!.Value;
                        if (ownedItem.Item.Rank > 0)
                        {
                            ownedItem.User.HeirloomPoints += ownedItem.Item.Rank;
                        }

                        _db.OwnedItems.Remove(ownedItem);
                    }

                    var itemsToDelete = dbItemsByMbId.Values.Where(i => i.BaseItemId == dbItem.BaseItemId).ToArray();
                    foreach (var i in itemsToDelete)
                    {
                        _db.Entry(i).State = EntityState.Deleted;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);

                // Fix BaseItem for items of rank = 0
                foreach (Item baseItem in baseItems)
                {
                    baseItem.BaseItem = baseItem;
                }
            }

            private void CreateOrUpdateItem(Dictionary<(string mbId, int rank), Item> dbItemsByMbId, Item item)
            {
                if (dbItemsByMbId.TryGetValue((item.TemplateMbId, item.Rank), out Item? dbItem))
                {
                    // replace item in context
                    _db.Entry(dbItem).State = EntityState.Detached;

                    item.Id = dbItem.Id;
                    _db.Items.Update(item);
                }
                else
                {
                    _db.Items.Add(item);
                }
            }

            private Item ItemCreationToItem(ItemCreation item)
            {
                var res = new Item
                {
                    TemplateMbId = item.TemplateMbId,
                    Name = item.Name,
                    Culture = item.Culture,
                    Type = item.Type,
                    Weight = item.Weight,
                    Rank = item.Rank,
                };

                if (item.Armor != null)
                {
                    res.Armor = new ItemArmorComponent
                    {
                        HeadArmor = item.Armor!.HeadArmor,
                        BodyArmor = item.Armor!.BodyArmor,
                        ArmArmor = item.Armor!.ArmArmor,
                        LegArmor = item.Armor!.LegArmor,
                    };
                }

                if (item.Mount != null)
                {
                    res.Mount = new ItemMountComponent
                    {
                        BodyLength = item.Mount!.BodyLength,
                        ChargeDamage = item.Mount!.ChargeDamage,
                        Maneuver = item.Mount!.Maneuver,
                        Speed = item.Mount!.Speed,
                        HitPoints = item.Mount!.HitPoints,
                    };
                }

                if (item.Weapons.Count > 0)
                {
                    res.PrimaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[0]);
                }

                if (item.Weapons.Count > 1)
                {
                    res.SecondaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[1]);
                }

                if (item.Weapons.Count > 2)
                {
                    res.TertiaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[2]);
                }

                return res;
            }

            private ItemWeaponComponent IteamWeaponComponentFromViewModel(ItemWeaponComponentViewModel weaponComponent)
            {
                return new()
                {
                    Class = weaponComponent.Class,
                    Accuracy = weaponComponent.Accuracy,
                    MissileSpeed = weaponComponent.MissileSpeed,
                    StackAmount = weaponComponent.StackAmount,
                    Length = weaponComponent.Length,
                    Balance = weaponComponent.Balance,
                    Handling = weaponComponent.Handling,
                    BodyArmor = weaponComponent.BodyArmor,
                    Flags = weaponComponent.Flags,
                    ThrustDamage = weaponComponent.ThrustDamage,
                    ThrustDamageType = weaponComponent.ThrustDamageType,
                    ThrustSpeed = weaponComponent.ThrustSpeed,
                    SwingDamage = weaponComponent.SwingDamage,
                    SwingDamageType = weaponComponent.SwingDamageType,
                    SwingSpeed = weaponComponent.SwingSpeed,
                };
            }
        }
    }
}
