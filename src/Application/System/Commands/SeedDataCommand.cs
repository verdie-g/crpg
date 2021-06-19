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
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.System.Commands
{
    public record SeedDataCommand : IMediatorRequest
    {
        internal class Handler : IMediatorRequestHandler<SeedDataCommand>
        {
            private static readonly int[] ItemRanks = { -3, -2, -1, 1, 2, 3 };

            private static readonly Dictionary<SettlementType, int> StrategusSettlementDefaultTroops = new()
            {
                [SettlementType.Village] = 1000,
                [SettlementType.Castle] = 4000,
                [SettlementType.Town] = 8000,
            };

            private readonly ICrpgDbContext _db;
            private readonly IItemsSource _itemsSource;
            private readonly IApplicationEnvironment _appEnv;
            private readonly ICharacterService _characterService;
            private readonly IExperienceTable _experienceTable;
            private readonly IStrategusMap _strategusMap;
            private readonly ISettlementsSource _settlementsSource;
            private readonly ItemValueModel _itemValueModel;
            private readonly ItemModifierService _itemModifierService;

            public Handler(ICrpgDbContext db, IItemsSource itemsSource, IApplicationEnvironment appEnv,
                ICharacterService characterService, IExperienceTable experienceTable, IStrategusMap strategusMap,
                ISettlementsSource settlementsSource, ItemValueModel itemValueModel,
                ItemModifierService itemModifierService)
            {
                _db = db;
                _itemsSource = itemsSource;
                _appEnv = appEnv;
                _characterService = characterService;
                _experienceTable = experienceTable;
                _strategusMap = strategusMap;
                _settlementsSource = settlementsSource;
                _itemValueModel = itemValueModel;
                _itemModifierService = itemModifierService;
            }

            public async Task<Result> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                await CreateOrUpdateItems(cancellationToken);
                await CreateOrUpdateSettlements(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                if (_appEnv.Environment == HostingEnvironment.Development)
                {
                    await AddDevelopmentData();
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return Result.NoErrors;
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
                    PlatformUserId = "76561197979977620",
                    Name = "Sellka",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63_full.jpg"),
                };
                User leanir = new()
                {
                    PlatformUserId = "76561198018585047",
                    Name = "Laenir",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d_full.jpg"),
                };
                User opset = new()
                {
                    PlatformUserId = "76561198009970770",
                    Name = "Opset_the_Grey",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81_full.jpg"),
                };
                User falcom = new()
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
                User manik = new()
                {
                    PlatformUserId = "76561198068833541",
                    Name = "Manik",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edf5af17958c09a5bbcb12e352d8fa9560c22aac.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edf5af17958c09a5bbcb12e352d8fa9560c22aac_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edf5af17958c09a5bbcb12e352d8fa9560c22aac_full.jpg"),
                };
                User ajroselle = new()
                {
                    PlatformUserId = "76561199043634047",
                    Name = "ajroselle",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
                };
                User skrael = new()
                {
                    PlatformUserId = "76561197996473259",
                    Name = "Skrael",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/95/950f9f3147d4c8530a5072825d01c34ee3f1afa1.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/95/950f9f3147d4c8530a5072825d01c34ee3f1afa1_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/95/950f9f3147d4c8530a5072825d01c34ee3f1afa1_full.jpg"),
                };
                User bedo = new()
                {
                    PlatformUserId = "76561198068806579",
                    Name = "bedo",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce19953febd356e443567298449acd7284050a83.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce19953febd356e443567298449acd7284050a83_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce19953febd356e443567298449acd7284050a83_full.jpg"),
                };
                User lambic = new()
                {
                    PlatformUserId = "76561198065010536",
                    Name = "Lambic",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af03d6342998e9f6887ac12883279c78edec7272.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af03d6342998e9f6887ac12883279c78edec7272_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af03d6342998e9f6887ac12883279c78edec7272_full.jpg"),
                };
                User sanasar = new()
                {
                    PlatformUserId = "76561198038834052",
                    Name = "Sanasar",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/38/38b27ecb2cfd536bf553790e425ccd0a4ac9add7.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/38/38b27ecb2cfd536bf553790e425ccd0a4ac9add7_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/38/38b27ecb2cfd536bf553790e425ccd0a4ac9add7_full.jpg"),
                };
                User vlad007 = new()
                {
                    PlatformUserId = "76561198007345621",
                    Name = "Vlad007",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
                };
                User canp0g = new()
                {
                    PlatformUserId = "76561198099388699",
                    Name = "CaNp0G",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b2dc0e2223189a9ba64377e3be43d0d99442432f.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b2dc0e2223189a9ba64377e3be43d0d99442432f_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b2dc0e2223189a9ba64377e3be43d0d99442432f_full.jpg"),
                };
                User shark = new()
                {
                    PlatformUserId = "76561198035838802",
                    Name = "Shark",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edd897e10a88795339e102f3ff88730afd684dd9.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edd897e10a88795339e102f3ff88730afd684dd9_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edd897e10a88795339e102f3ff88730afd684dd9_full.jpg"),
                };
                User noobAmphetamine = new()
                {
                    PlatformUserId = "76561198140492451",
                    Name = "Noobamphetamine",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
                };
                User mundete = new()
                {
                    PlatformUserId = "76561198298979454",
                    Name = "Mundete",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/99/994d037cb361b375cf7f34d510664dca959e27d2.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/99/994d037cb361b375cf7f34d510664dca959e27d2_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/99/994d037cb361b375cf7f34d510664dca959e27d2_full.jpg"),
                };
                User aroyFalconer = new()
                {
                    PlatformUserId = "76561198055090640",
                    Name = "aroyfalconer",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
                };
                User insanitoid = new()
                {
                    PlatformUserId = "76561198073114187",
                    Name = "Insanitoid",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/23/23ca1018e64e454b05b558cbf9cc7d55d1e57fc5.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/23/23ca1018e64e454b05b558cbf9cc7d55d1e57fc5_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/23/23ca1018e64e454b05b558cbf9cc7d55d1e57fc5_full.jpg"),
                };
                User scarface = new()
                {
                    PlatformUserId = "76561198279433049",
                    Name = "Scarface",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7b237d0943aa81b7f0637e46baff7eff9afa48ae.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7b237d0943aa81b7f0637e46baff7eff9afa48ae_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7b237d0943aa81b7f0637e46baff7eff9afa48ae_full.jpg"),
                };
                User xDem = new()
                {
                    PlatformUserId = "76561197998420060",
                    Name = "XDem",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a15730cb6852a7b3b8109ff70a8ab506ed221ea1.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a15730cb6852a7b3b8109ff70a8ab506ed221ea1_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a15730cb6852a7b3b8109ff70a8ab506ed221ea1_full.jpg"),
                };
                User disorot = new()
                {
                    PlatformUserId = "76561198117963151",
                    Name = "Disorot",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7bab1c0d1a1716a7648afdfd987c44bfb58367a8.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7bab1c0d1a1716a7648afdfd987c44bfb58367a8_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7bab1c0d1a1716a7648afdfd987c44bfb58367a8_full.jpg"),
                };
                User ace = new()
                {
                    PlatformUserId = "76561198069571271",
                    Name = "Ace",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ac/ac7445b35f7e18eebe0d2a728aaad139b0dca3c5.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ac/ac7445b35f7e18eebe0d2a728aaad139b0dca3c5_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ac/ac7445b35f7e18eebe0d2a728aaad139b0dca3c5_full.jpg"),
                };
                User sagar = new()
                {
                    PlatformUserId = "76561198049628859",
                    Name = "Sagar",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/01/0190fa213e030bcffdde532705df318f348e8d30.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/01/0190fa213e030bcffdde532705df318f348e8d30_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/01/0190fa213e030bcffdde532705df318f348e8d30_full.jpg"),
                };
                User greenShadow = new()
                {
                    PlatformUserId = "76561198239298650",
                    Name = "GreenShadow",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7f74b4cea3ce894e22890705466741276667e91.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7f74b4cea3ce894e22890705466741276667e91_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7f74b4cea3ce894e22890705466741276667e91_full.jpg"),
                };
                User hannibaru = new()
                {
                    PlatformUserId = "76561198120421508",
                    Name = "Hannibaru",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af69a66c19d409449586fdd863a70ffca5a3924c.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af69a66c19d409449586fdd863a70ffca5a3924c_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af69a66c19d409449586fdd863a70ffca5a3924c_full.jpg"),
                };
                User drexx = new()
                {
                    PlatformUserId = "76561198010855139",
                    Name = "Drexx",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ee/ee56a301d3ec686b77c6d06c7517fbb57065b36b.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ee/ee56a301d3ec686b77c6d06c7517fbb57065b36b_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ee/ee56a301d3ec686b77c6d06c7517fbb57065b36b_full.jpg"),
                };
                User xarosh = new()
                {
                    PlatformUserId = "76561198089566223",
                    Name = "Xarosh",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bc/bcc1c53ab76da0813e6456264ee6b588b30de7af.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bc/bcc1c53ab76da0813e6456264ee6b588b30de7af_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bc/bcc1c53ab76da0813e6456264ee6b588b30de7af_full.jpg"),
                };
                User tipsyToby = new()
                {
                    PlatformUserId = "76561198084047374",
                    Name = "TipsyToby1969",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/1c/1caacc14b003b71ddf09c56675c9462440dcb534.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/1c/1caacc14b003b71ddf09c56675c9462440dcb534_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/1c/1caacc14b003b71ddf09c56675c9462440dcb534_full.jpg"),
                };
                User localAlpha = new()
                {
                    PlatformUserId = "76561198204128229",
                    Name = "LocalAlpha",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5b58ff641803804038c3cb3529904b14bc22b2c.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5b58ff641803804038c3cb3529904b14bc22b2c_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5b58ff641803804038c3cb3529904b14bc22b2c_full.jpg"),
                };
                User alex = new()
                {
                    PlatformUserId = "76561198049945204",
                    Name = "Alex",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c3/c300efbbcfae57c59095547ad9362c81c9001f07.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c3/c300efbbcfae57c59095547ad9362c81c9001f07_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c3/c300efbbcfae57c59095547ad9362c81c9001f07_full.jpg"),
                };
                User kedrynFuel = new()
                {
                    PlatformUserId = "76561198124895605",
                    Name = "KedrynFuel",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d9/d94b47877d0f0a0e50f66d80a1de34bfbf94a56f.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d9/d94b47877d0f0a0e50f66d80a1de34bfbf94a56f_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d9/d94b47877d0f0a0e50f66d80a1de34bfbf94a56f_full.jpg"),
                };
                User luqero = new()
                {
                    PlatformUserId = "76561197990543288",
                    Name = "LuQeRo",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ad/adf81333c999516c251df9ca281553d487825f1c.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ad/adf81333c999516c251df9ca281553d487825f1c_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ad/adf81333c999516c251df9ca281553d487825f1c_full.jpg"),
                };
                User ilya = new()
                {
                    PlatformUserId = "76561198116180462",
                    Name = "ilya2106",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f4/f4b04c6590153ebb1a43c9192627beb07bb613f3.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f4/f4b04c6590153ebb1a43c9192627beb07bb613f3_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f4/f4b04c6590153ebb1a43c9192627beb07bb613f3_full.jpg"),
                };
                User eztli = new()
                {
                    PlatformUserId = "76561197995328883",
                    Name = "Eztli",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/97/971a781269e5cd82b76d0cacc138f180bbfbb8d2.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/97/971a781269e5cd82b76d0cacc138f180bbfbb8d2_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/97/971a781269e5cd82b76d0cacc138f180bbfbb8d2_full.jpg"),
                };
                User telesto = new()
                {
                    PlatformUserId = "76561198021932355",
                    Name = "Telesto",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
                };
                User kypak = new()
                {
                    PlatformUserId = "76561198133571210",
                    Name = "Kypak",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/df/df6e263fe8cd9ec2d1a2a7d61da59d47f23a52cd.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/df/df6e263fe8cd9ec2d1a2a7d61da59d47f23a52cd_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/df/df6e263fe8cd9ec2d1a2a7d61da59d47f23a52cd_full.jpg"),
                };
                User devoidDragon = new()
                {
                    PlatformUserId = "76561198018668459",
                    Name = "DevoidDragon",
                    AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/79/79a8119bd2a027755f93872d0d09b959909a0405.jpg"),
                    AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/79/79a8119bd2a027755f93872d0d09b959909a0405_medium.jpg"),
                    AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/79/79a8119bd2a027755f93872d0d09b959909a0405_full.jpg"),
                };

                User[] newUsers =
                {
                    takeo, baronCyborg, magnuclean, knitler, tjens, lerch, buddha, lancelot, bakhrat, distance,
                    victorhh888, schumetzq, bryggan, ikarooz, kiwi, brainfart, falcom, opset, leanir, sellka, firebat,
                    ecko, neostralie, zorguy, azuma, elmaryk, namidaka, laHire, manik, ajroselle, skrael, bedo, lambic,
                    sanasar, vlad007, canp0g, shark, noobAmphetamine, mundete, aroyFalconer, insanitoid, scarface,
                    xDem, disorot, ace, sagar, greenShadow, hannibaru, drexx, xarosh, tipsyToby, localAlpha, alex,
                    kedrynFuel, luqero, ilya, eztli, telesto, kypak, devoidDragon,
                };

                var existingUsers = await _db.Users.ToDictionaryAsync(u => (u.Platform, u.PlatformUserId));
                foreach (var newUser in newUsers)
                {
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

                Character takeoCharacter0 = new()
                {
                    User = takeo,
                    Name = takeo.Name,
                    Generation = 2,
                    Level = 23,
                    Experience = _experienceTable.GetExperienceForLevel(23),
                };
                Character takeoCharacter1 = new()
                {
                    User = takeo,
                    Name = "totoalala",
                    Level = 12,
                    Experience = _experienceTable.GetExperienceForLevel(12),
                };
                Character takeoCharacter2 = new()
                {
                    User = takeo,
                    Name = "Retire me",
                    Level = 31,
                    Experience = _experienceTable.GetExperienceForLevel(31) + 100,
                };
                Character falcomCharacter0 = new()
                {
                    User = falcom,
                    Name = falcom.Name,
                };
                Character victorhh888Character0 = new()
                {
                    User = victorhh888,
                    Name = victorhh888.Name,
                };
                Character sellkaCharacter0 = new()
                {
                    User = sellka,
                    Name = sellka.Name,
                };

                Character[] newCharacters =
                {
                    takeoCharacter0, takeoCharacter1, takeoCharacter2, falcomCharacter0, victorhh888Character0,
                    sellkaCharacter0,
                };

                var existingCharacters = await _db.Characters.ToDictionaryAsync(c => c.Name);
                foreach (var newCharacter in newCharacters)
                {
                    _characterService.ResetCharacterStats(newCharacter, respecialization: true);

                    if (existingCharacters.TryGetValue(newCharacter.Name, out var existingCharacter))
                    {
                        _db.Entry(existingCharacter).State = EntityState.Detached;

                        newCharacter.Id = existingCharacter.Id;
                        _db.Characters.Update(newCharacter);
                    }
                    else
                    {
                        _db.Characters.Add(newCharacter);
                    }
                }

                Clan pecores = new()
                {
                    Tag = "PEC",
                    Color = "#3273DC",
                    Name = "Pecores",
                };
                Clan ats = new()
                {
                    Tag = "ATS",
                    Color = "#FF3860",
                    Name = "Among The Shadows",
                };
                Clan legio = new()
                {
                    Tag = "LEG",
                    Color = "#FFDD57",
                    Name = "Legio",
                };
                Clan theGrey = new()
                {
                    Tag = "GREY",
                    Color = "#7A7A7A",
                    Name = "The Grey",
                };
                Clan ode = new()
                {
                    Tag = "OdE",
                    Color = "#00D1B2",
                    Name = "Ordre de l'étoile",
                };
                Clan virginDefenders = new()
                {
                    Tag = "VD",
                    Color = "#FF7D97",
                    Name = "Virgin Defenders",
                };
                Clan randomClan = new()
                {
                    Tag = "RC",
                    Color = "#F5F5F5",
                    Name = "Random Clan",
                };
                Clan abcClan = new()
                {
                    Tag = "ABC",
                    Color = "#5D3C43",
                    Name = "ABC",
                };
                Clan defClan = new()
                {
                    Tag = "DEF",
                    Color = "#65B1A6",
                    Name = "DEF",
                };
                Clan ghiClan = new()
                {
                    Tag = "GHI",
                    Color = "#1A544C",
                    Name = "GHI",
                };
                Clan jklClan = new()
                {
                    Tag = "JKL",
                    Color = "#10044F",
                    Name = "JKL",
                };
                Clan mnoClan = new()
                {
                    Tag = "MNO",
                    Color = "#5A541C",
                    Name = "MNO",
                };
                Clan pqrClan = new()
                {
                    Tag = "PQR",
                    Color = "#123456",
                    Name = "Plan QR",
                };
                Clan[] newClans =
                {
                    pecores, ats, legio, theGrey, ode, virginDefenders, randomClan, abcClan, defClan, ghiClan, jklClan,
                    mnoClan, pqrClan,
                };

                var existingClans = await _db.Clans.ToDictionaryAsync(c => c.Name);
                foreach (var newClan in newClans)
                {
                    if (!existingClans.ContainsKey(newClan.Name))
                    {
                        _db.Clans.Add(newClan);
                    }
                }

                ClanMember namidakaMember = new() { User = namidaka, Clan = pecores, Role = ClanMemberRole.Leader };
                ClanMember neostralieMember = new() { User = neostralie, Clan = pecores, Role = ClanMemberRole.Officer };
                ClanMember elmarykMember = new() { User = elmaryk, Clan = pecores, Role = ClanMemberRole.Officer };
                ClanMember laHireMember = new() { User = laHire, Clan = pecores, Role = ClanMemberRole.Member };
                ClanMember azumaMember = new() { User = azuma, Clan = pecores, Role = ClanMemberRole.Member };
                ClanMember zorguyMember = new() { User = zorguy, Clan = pecores, Role = ClanMemberRole.Member };
                ClanMember eckoMember = new() { User = ecko, Clan = ats, Role = ClanMemberRole.Leader };
                ClanMember firebatMember = new() { User = firebat, Clan = ats, Role = ClanMemberRole.Officer };
                ClanMember sellkaMember = new() { User = sellka, Clan = ats, Role = ClanMemberRole.Member };
                ClanMember leanirMember = new() { User = leanir, Clan = legio, Role = ClanMemberRole.Leader, };
                ClanMember opsetMember = new() { User = opset, Clan = theGrey, Role = ClanMemberRole.Leader, };
                ClanMember falcomMember = new() { User = falcom, Clan = ode, Role = ClanMemberRole.Leader, };
                ClanMember brainfartMember = new() { User = brainfart, Clan = virginDefenders, Role = ClanMemberRole.Leader };
                ClanMember kiwiMember = new() { User = kiwi, Clan = virginDefenders, Role = ClanMemberRole.Officer };
                ClanMember ikaroozMember = new() { User = ikarooz, Clan = virginDefenders, Role = ClanMemberRole.Member };
                ClanMember brygganMember = new() { User = bryggan, Clan = virginDefenders, Role = ClanMemberRole.Member };
                ClanMember schumetzqMember = new() { User = schumetzq, Clan = virginDefenders, Role = ClanMemberRole.Member };
                ClanMember victorhh888Member = new() { User = victorhh888, Clan = randomClan, Role = ClanMemberRole.Leader };
                ClanMember distanceMember = new() { User = distance, Clan = randomClan, Role = ClanMemberRole.Officer };
                ClanMember bakhratMember = new() { User = bakhrat, Clan = randomClan, Role = ClanMemberRole.Member };
                ClanMember lancelotMember = new() { User = lancelot, Clan = abcClan, Role = ClanMemberRole.Leader };
                ClanMember buddhaMember = new() { User = buddha, Clan = abcClan, Role = ClanMemberRole.Member };
                ClanMember lerchMember = new() { User = lerch, Clan = defClan, Role = ClanMemberRole.Leader };
                ClanMember tjensMember = new() { User = tjens, Clan = ghiClan, Role = ClanMemberRole.Leader };
                ClanMember knitlerMember = new() { User = knitler, Clan = jklClan, Role = ClanMemberRole.Leader };
                ClanMember magnucleanMember = new() { User = magnuclean, Clan = mnoClan, Role = ClanMemberRole.Leader };
                ClanMember baronCyborgMember = new() { User = baronCyborg, Clan = pqrClan, Role = ClanMemberRole.Leader, };

                ClanMember[] newClanMembers =
                {
                    namidakaMember, neostralieMember, elmarykMember, laHireMember, azumaMember, zorguyMember,
                    eckoMember, firebatMember, sellkaMember, leanirMember, opsetMember,
                    falcomMember, brainfartMember, kiwiMember, ikaroozMember, brygganMember, schumetzqMember,
                    victorhh888Member, distanceMember, bakhratMember, lancelotMember,
                    buddhaMember, lerchMember, tjensMember, knitlerMember, magnucleanMember, baronCyborgMember,
                };
                var existingClanMembers = await _db.ClanMembers.ToDictionaryAsync(cm => cm.UserId);
                foreach (var newClanMember in newClanMembers)
                {
                    if (!existingClanMembers.ContainsKey(newClanMember.User!.Id))
                    {
                        _db.ClanMembers.Add(newClanMember);
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
                    await _db.ClanInvitations.ToDictionaryAsync(i => (i.InviteeId, i.InviterId));
                foreach (var newClanInvitation in newClanInvitations)
                {
                    if (!existingClanInvitations.ContainsKey((newClanInvitation.Invitee!.Id, newClanInvitation.Inviter!.Id)))
                    {
                        _db.ClanInvitations.Add(newClanInvitation);
                    }
                }

                Task<Settlement> GetSettlementByName(string name) =>
                    _db.Settlements.FirstAsync(s => s.Name == name && s.Region == Region.Europe);
                var epicrotea = await GetSettlementByName("Epicrotea");
                var mecalovea = await GetSettlementByName("Mecalovea");
                var marathea = await GetSettlementByName("Marathea");
                var stathymos = await GetSettlementByName("Stathymos");
                var gersegosCastle = await GetSettlementByName("Gersegos Castle");
                var dyopalis = await GetSettlementByName("Dyopalis");
                var rhesosCastle = await GetSettlementByName("Rhesos Castle");
                var potamis = await GetSettlementByName("Potamis");
                var carphenion = await GetSettlementByName("Carphenion");
                var ataconiaCastle = await GetSettlementByName("Ataconia Castle");
                var ataconia = await GetSettlementByName("Ataconia");
                var elipa = await GetSettlementByName("Elipa");
                var rhotae = await GetSettlementByName("Rhotae");
                var hertogeaCastle = await GetSettlementByName("Hertogea Castle");
                var hertogea = await GetSettlementByName("Hertogea");
                var nideon = await GetSettlementByName("Nideon");
                var leblenion = await GetSettlementByName("Leblenion");
                var rhemtoil = await GetSettlementByName("Rhemtoil");

                Hero brainfartHero = new()
                {
                    Region = Region.Europe,
                    User = brainfart,
                    Troops = 1,
                    Position = new Point(112, -88),
                    Status = HeroStatus.Idle,
                };
                Hero kiwiHero = new()
                {
                    Region = Region.Europe,
                    User = kiwi,
                    Troops = 1,
                    Position = new Point(142, -90),
                    Status = HeroStatus.Idle,
                };
                Hero ikaroozHero = new()
                {
                    Region = Region.Europe,
                    User = ikarooz,
                    Troops = 20,
                    Position = new Point(130, -102),
                    Status = HeroStatus.Idle,
                };
                Hero laHireHero = new()
                {
                    Region = Region.Europe,
                    User = laHire,
                    Troops = 20,
                    Position = new Point(135, -97),
                    Status = HeroStatus.Idle,
                };
                Hero brygganHero = new()
                {
                    Region = Region.Europe,
                    User = bryggan,
                    Troops = 1,
                    Position = new Point(131, -102),
                    Status = HeroStatus.Idle,
                };
                Hero elmarykHero = new()
                {
                    Region = Region.Europe,
                    User = elmaryk,
                    Troops = 6,
                    Position = new Point(108, -98),
                    Status = HeroStatus.Idle,
                };
                Hero schumetzqHero = new()
                {
                    Region = Region.Europe,
                    User = schumetzq,
                    Troops = 7,
                    Position = new Point(119, -105),
                    Status = HeroStatus.Idle,
                };
                Hero azumaHero = new()
                {
                    Region = Region.Europe,
                    User = azuma,
                    Troops = 121,
                    Position = new Point(106, -112),
                    Status = HeroStatus.Idle,
                };
                Hero zorguyHero = new()
                {
                    Region = Region.Europe,
                    User = zorguy,
                    Troops = 98,
                    Position = new Point(114, -114),
                    Status = HeroStatus.Idle,
                };
                Hero eckoHero = new()
                {
                    Region = Region.Europe,
                    User = ecko,
                    Troops = 55,
                    Position = new Point(117, -112),
                    Status = HeroStatus.Idle,
                };
                Hero firebatHero = new()
                {
                    Region = Region.Europe,
                    User = firebat,
                    Troops = 29,
                    Position = new Point(105, -111),
                    Status = HeroStatus.Idle,
                };
                Hero laenirHero = new()
                {
                    Region = Region.Europe,
                    User = leanir,
                    Troops = 1,
                    Position = new Point(103, -102),
                    Status = HeroStatus.Idle,
                };
                Hero opsetHero = new()
                {
                    Region = Region.Europe,
                    User = opset,
                    Troops = 1,
                    Position = new Point(113, -112),
                    Status = HeroStatus.Idle,
                };
                Hero falcomHero = new()
                {
                    Region = Region.Europe,
                    User = falcom,
                    Troops = 4,
                    Position = epicrotea.Position,
                    Status = HeroStatus.IdleInSettlement,
                    TargetedSettlement = epicrotea,
                };
                Hero victorhh888Hero = new()
                {
                    Region = Region.Europe,
                    User = victorhh888,
                    Troops = 9,
                    Position = epicrotea.Position,
                    Status = HeroStatus.RecruitingInSettlement,
                };
                Hero sellkaHero = new()
                {
                    Region = Region.Europe,
                    User = sellka,
                    Troops = 3,
                    Position = dyopalis.Position,
                    Status = HeroStatus.RecruitingInSettlement,
                    TargetedSettlement = dyopalis,
                };
                Hero distanceHero = new()
                {
                    Region = Region.Europe,
                    User = distance,
                    Troops = 1,
                    Position = rhotae.Position,
                    Status = HeroStatus.RecruitingInSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero bakhratHero = new()
                {
                    Region = Region.Europe,
                    User = bakhrat,
                    Troops = 120,
                    Position = rhotae.Position,
                    Status = HeroStatus.RecruitingInSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero lancelotHero = new()
                {
                    Region = Region.Europe,
                    User = lancelot,
                    Troops = 243,
                    Position = rhotae.Position,
                    Status = HeroStatus.Idle,
                    TargetedSettlement = rhotae,
                };
                Hero buddhaHero = new()
                {
                    Region = Region.Europe,
                    User = buddha,
                    Troops = 49,
                    Position = nideon.Position,
                    Status = HeroStatus.IdleInSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero lerchHero = new()
                {
                    Region = Region.Europe,
                    User = lerch,
                    Troops = 10,
                    Position = new Point(107, -102),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero tjensHero = new()
                {
                    Region = Region.Europe,
                    User = tjens,
                    Troops = 20,
                    Position = new Point(112, -93),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero knitlerHero = new()
                {
                    Region = Region.Europe,
                    User = knitler,
                    Troops = 3,
                    Position = new Point(124, -102),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero magnucleanHero = new()
                {
                    Region = Region.Europe,
                    User = magnuclean,
                    Troops = 9,
                    Position = new Point(120, -88),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = rhemtoil,
                };
                Hero baronCyborgHero = new()
                {
                    Region = Region.Europe,
                    User = baronCyborg,
                    Troops = 9,
                    Position = new Point(120, -88),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = mecalovea,
                };
                Hero scarfaceHero = new()
                {
                    Region = Region.Europe,
                    User = scarface,
                    Troops = 25,
                    Position = new Point(119, -105),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = hertogeaCastle,
                };
                Hero neostralieHero = new()
                {
                    Region = Region.Europe,
                    User = neostralie,
                    Troops = 1,
                    Position = new Point(128, -97),
                    Status = HeroStatus.MovingToSettlement,
                    TargetedSettlement = potamis,
                };
                Hero manikHero = new()
                {
                    Region = Region.Europe,
                    User = manik,
                    Troops = 1,
                    Position = new Point(129, -102),
                    Status = HeroStatus.MovingToAttackHero,
                    TargetedHero = neostralieHero,
                };
                Hero ajroselleHero = new()
                {
                    Region = Region.Europe,
                    User = ajroselle,
                    Troops = 1,
                    Position = new Point(130, -107),
                    Status = HeroStatus.MovingToAttackHero,
                    TargetedHero = manikHero,
                };
                Hero skraelHero = new()
                {
                    Region = Region.Europe,
                    User = skrael,
                    Troops = 1,
                    Position = new Point(126, -93),
                    Status = HeroStatus.MovingToAttackHero,
                    TargetedHero = neostralieHero,
                };
                Hero bedoHero = new()
                {
                    Region = Region.Europe,
                    User = bedo,
                    Troops = 300,
                    Position = new Point(114, -101),
                    Status = HeroStatus.MovingToAttackSettlement,
                    TargetedSettlement = gersegosCastle,
                };
                Hero lambicHero = new()
                {
                    Region = Region.Europe,
                    User = lambic,
                    Troops = 87,
                    Position = new Point(113, -98),
                    Status = HeroStatus.MovingToAttackSettlement,
                    TargetedSettlement = gersegosCastle,
                };
                Hero sanasarHero = new()
                {
                    Region = Region.Europe,
                    User = sanasar,
                    Troops = 21,
                    Position = new Point(119, -101),
                    Status = HeroStatus.MovingToAttackSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero vlad007Hero = new()
                {
                    Region = Region.Europe,
                    User = vlad007,
                    Troops = 21,
                    Position = new Point(119, -101),
                    Status = HeroStatus.MovingToAttackSettlement,
                    TargetedSettlement = rhotae,
                };
                Hero canp0GHero = new()
                {
                    Region = Region.Europe,
                    User = canp0g,
                    Troops = 1,
                    Position = rhesosCastle.Position,
                    Status = HeroStatus.MovingToPoint,
                    Waypoints = new MultiPoint(new[] { new Point(125, -97) }),
                };
                Hero sharkHero = new()
                {
                    Region = Region.Europe,
                    User = shark,
                    Troops = 1,
                    Position = new Point(105, -107),
                    Status = HeroStatus.MovingToPoint,
                    Waypoints = new MultiPoint(new[] { new Point(121, -99) }),
                };
                Hero noobAmphetamineHero = new()
                {
                    Region = Region.Europe,
                    User = noobAmphetamine,
                    Troops = 1,
                    Position = new Point(107, -100),
                    Status = HeroStatus.MovingToPoint,
                    Waypoints = new MultiPoint(new[] { new Point(112, -88) }),
                };
                Hero mundeteHero = new()
                {
                    Region = Region.Europe,
                    User = mundete,
                    Troops = 1,
                    Position = new Point(112, -99),
                    Status = HeroStatus.FollowingHero,
                    TargetedHero = sharkHero,
                };
                Hero aroyFalconerHero = new()
                {
                    Region = Region.Europe,
                    User = aroyFalconer,
                    Troops = 1,
                    Position = new Point(123, -88),
                    Status = HeroStatus.MovingToPoint,
                    Waypoints = new MultiPoint(new[] { new Point(135, -98) }),
                };
                Hero insanitoidHero = new()
                {
                    Region = Region.Europe,
                    User = insanitoid,
                    Troops = 1,
                    Position = new Point(135, -98),
                    Status = HeroStatus.MovingToPoint,
                    Waypoints = new MultiPoint(new[] { new Point(123, -88) }),
                };
                Hero namidakaHero = new()
                {
                    Region = Region.Europe,
                    User = namidaka,
                    Troops = 11,
                    Position = new Point(135, -99),
                    Status = HeroStatus.FollowingHero,
                    TargetedHero = insanitoidHero,
                };
                Hero xDemHero = new()
                {
                    Region = Region.Europe,
                    User = xDem,
                    Troops = 250,
                    Position = new Point(nideon.Position.X - 0.2, nideon.Position.Y - 0.2),
                    Status = HeroStatus.InBattle,
                    TargetedSettlement = nideon,
                };
                Hero disorotHero = new()
                {
                    Region = Region.Europe,
                    User = disorot,
                    Troops = 89,
                    Position = new Point(nideon.Position.X + 0.2, nideon.Position.Y + 0.2),
                    Status = HeroStatus.InBattle,
                };
                Hero aceHero = new()
                {
                    Region = Region.Europe,
                    User = ace,
                    Troops = 104,
                    Position = new Point(nideon.Position.X - 0.2, nideon.Position.Y + 0.2),
                    Status = HeroStatus.InBattle,
                };
                Hero sagarHero = new()
                {
                    Region = Region.Europe,
                    User = sagar,
                    Troops = 300,
                    Position = new Point(nideon.Position.X + 0.2, nideon.Position.Y - 0.2),
                    Status = HeroStatus.Idle,
                };
                Hero greenShadowHero = new()
                {
                    Region = Region.Europe,
                    User = greenShadow,
                    Troops = 31,
                    Position = new Point(106.986, -110.171),
                    Status = HeroStatus.InBattle,
                };
                Hero hannibaruHero = new()
                {
                    Region = Region.Europe,
                    User = hannibaru,
                    Troops = 42,
                    Position = new Point(107.109, -110.328),
                    Status = HeroStatus.InBattle,
                };
                Hero drexxHero = new()
                {
                    Region = Region.Europe,
                    User = drexx,
                    Troops = 53,
                    Position = new Point(107.304, -110.203),
                    Status = HeroStatus.InBattle,
                };
                Hero xaroshHero = new()
                {
                    Region = Region.Europe,
                    User = xarosh,
                    Troops = 64,
                    Position = new Point(107.210, -110.062),
                    Status = HeroStatus.InBattle,
                };
                Hero tipsyTobyHero = new()
                {
                    Region = Region.Europe,
                    User = tipsyToby,
                    Troops = 75,
                    Position = new Point(107.304, -110.046),
                    Status = HeroStatus.Idle,
                };
                Hero localAlphaHero = new()
                {
                    Region = Region.Europe,
                    User = localAlpha,
                    Troops = 75,
                    Position = new Point(107.304, -110.046),
                    Status = HeroStatus.Idle,
                };
                Hero alexHero = new()
                {
                    Region = Region.Europe,
                    User = alex,
                    Troops = 86,
                    Position = new Point(107, -106),
                    Status = HeroStatus.InBattle,
                    TargetedSettlement = hertogea,
                };
                Hero kedrynFuelHero = new()
                {
                    Region = Region.Europe,
                    User = kedrynFuel,
                    Troops = 97,
                    Position = new Point(107, -106.2),
                    Status = HeroStatus.FollowingHero,
                    TargetedHero = alexHero,
                };
                Hero luqeroHero = new()
                {
                    Region = Region.Europe,
                    User = luqero,
                    Troops = 108,
                    Position = hertogea.Position,
                    Status = HeroStatus.IdleInSettlement,
                    TargetedSettlement = hertogea,
                };
                Hero ilyaHero = new()
                {
                    Region = Region.Europe,
                    User = ilya,
                    Troops = 119,
                    Position = hertogea.Position,
                    Status = HeroStatus.IdleInSettlement,
                    TargetedSettlement = hertogea,
                };
                Hero eztliHero = new()
                {
                    Region = Region.Europe,
                    User = eztli,
                    Troops = 86,
                    Position = leblenion.Position,
                    Status = HeroStatus.InBattle,
                    TargetedSettlement = leblenion,
                };

                // Users with no hero: telesto, kypak, devoidDragon.

                var newHeroes = new[]
                {
                    brainfartHero, kiwiHero, ikaroozHero, laHireHero, brygganHero, elmarykHero, schumetzqHero, azumaHero,
                    zorguyHero, eckoHero, firebatHero, laenirHero, opsetHero, falcomHero, victorhh888Hero, sellkaHero,
                    distanceHero, bakhratHero, lancelotHero, buddhaHero, lerchHero, tjensHero, knitlerHero, magnucleanHero,
                    baronCyborgHero, scarfaceHero, neostralieHero, manikHero, ajroselleHero, skraelHero, bedoHero,
                    lambicHero, sanasarHero, vlad007Hero, canp0GHero, sharkHero, noobAmphetamineHero, mundeteHero,
                    aroyFalconerHero, insanitoidHero, namidakaHero, xDemHero, disorotHero, aceHero, sagarHero,
                    greenShadowHero, hannibaruHero, drexxHero, xaroshHero, tipsyTobyHero, localAlphaHero, eztliHero,
                };

                var existingHeroes = (await _db.Heroes.ToArrayAsync())
                    .Select(u => u.Id)
                    .ToHashSet();
                foreach (var newHero in newHeroes)
                {
                    if (!existingHeroes.Contains(newHero.User!.Id))
                    {
                        _db.Heroes.Add(newHero);
                    }
                }

                Battle nideonBattle = new()
                {
                    Phase = BattlePhase.Preparation,
                    Region = Region.Europe,
                    Position = nideon.Position,
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Hero = xDemHero,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Hero = disorotHero,
                            Side = BattleSide.Attacker,
                            Commander = false,
                        },
                        new BattleFighter
                        {
                            Hero = null,
                            Settlement = nideon,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Hero = aceHero,
                            Side = BattleSide.Defender,
                            Commander = false,
                        },
                    },
                    FighterApplications =
                    {
                        new BattleFighterApplication
                        {
                            Hero = sagarHero,
                            Side = BattleSide.Defender,
                            Status = BattleFighterApplicationStatus.Pending,
                        },
                    },
                };
                Battle plainBattle = new()
                {
                    Phase = BattlePhase.Preparation,
                    Region = Region.Europe,
                    Position = new Point(107.187, -110.164),
                    Fighters =
                    {
                        new BattleFighter { Hero = xaroshHero, Side = BattleSide.Attacker, Commander = true },
                        new BattleFighter { Hero = greenShadowHero, Side = BattleSide.Attacker, Commander = false },
                        new BattleFighter { Hero = drexxHero, Side = BattleSide.Defender, Commander = true },
                        new BattleFighter { Hero = hannibaruHero, Side = BattleSide.Defender, Commander = false },
                    },
                    FighterApplications =
                    {
                        new BattleFighterApplication
                        {
                            Hero = tipsyTobyHero,
                            Side = BattleSide.Attacker,
                            Status = BattleFighterApplicationStatus.Pending,
                        },
                        new BattleFighterApplication
                        {
                            Hero = localAlphaHero,
                            Side = BattleSide.Defender,
                            Status = BattleFighterApplicationStatus.Pending,
                        },
                    },
                    CreatedAt = DateTimeOffset.Now,
                };
                Battle hertogeaBattle = new()
                {
                    Phase = BattlePhase.Hiring,
                    Region = Region.Europe,
                    Position = hertogea.Position,
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Hero = alexHero,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Hero = null,
                            Settlement = hertogea,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                    FighterApplications =
                    {
                        new BattleFighterApplication
                        {
                            Hero = kedrynFuelHero,
                            Side = BattleSide.Attacker,
                            Status = BattleFighterApplicationStatus.Pending,
                        },
                        new BattleFighterApplication
                        {
                            Hero = luqeroHero,
                            Side = BattleSide.Defender,
                            Status = BattleFighterApplicationStatus.Pending,
                        },
                        new BattleFighterApplication
                        {
                            Hero = ilyaHero,
                            Side = BattleSide.Defender,
                            Status = BattleFighterApplicationStatus.Pending,
                        },
                    },
                    CreatedAt = DateTimeOffset.Now.AddHours(-2),
                };
                Battle leblenionBattle = new()
                {
                    Phase = BattlePhase.Hiring,
                    Region = Region.Europe,
                    Position = leblenion.Position,
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Hero = eztliHero,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Hero = null,
                            Settlement = leblenion,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                    MercenaryApplications =
                    {
                        new BattleMercenaryApplication
                        {
                            Character = falcomCharacter0,
                            Side = BattleSide.Attacker,
                            Status = BattleMercenaryApplicationStatus.Pending,
                        },
                        new BattleMercenaryApplication
                        {
                            Character = victorhh888Character0,
                            Side = BattleSide.Defender,
                            Status = BattleMercenaryApplicationStatus.Pending,
                        },
                        new BattleMercenaryApplication
                        {
                            Character = sellkaCharacter0,
                            Side = BattleSide.Defender,
                            Status = BattleMercenaryApplicationStatus.Pending,
                        },
                    },
                    CreatedAt = DateTimeOffset.Now.AddHours(-4),
                };

                Battle[] newBattles = { nideonBattle, plainBattle, hertogeaBattle, leblenionBattle };
                if (!(await _db.Battles.AnyAsync()))
                {
                    _db.Battles.AddRange(newBattles);
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

                    var userItems = await _db.UserItems
                        .Include(oi => oi.User)
                        .Include(oi => oi.Item)
                        .Where(oi => oi.Item!.BaseItemId == dbItem.BaseItemId)
                        .ToArrayAsync(cancellationToken);
                    foreach (var userItem in userItems)
                    {
                        userItem.User!.Gold += userItem.Item!.Value;
                        if (userItem.Item.Rank > 0)
                        {
                            userItem.User.HeirloomPoints += userItem.Item.Rank;
                        }

                        _db.UserItems.Remove(userItem);
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

            private async Task CreateOrUpdateSettlements(CancellationToken cancellationToken)
            {
                var settlementsByName = (await _settlementsSource.LoadStrategusSettlements())
                    .ToDictionary(i => i.Name);
                var dbSettlementsByNameRegion = await _db.Settlements
                    .ToDictionaryAsync(di => (di.Name, di.Region), cancellationToken);

                foreach (var settlementCreation in settlementsByName.Values)
                {
                    foreach (var region in GetRegions())
                    {
                        var settlement = new Settlement
                        {
                            Name = settlementCreation.Name,
                            Type = settlementCreation.Type,
                            Culture = settlementCreation.Culture,
                            Region = region,
                            Position = _strategusMap.TranslatePositionForRegion(settlementCreation.Position, Region.Europe, region),
                            Scene = settlementCreation.Scene,
                            Troops = StrategusSettlementDefaultTroops[settlementCreation.Type],
                            OwnerId = null,
                        };

                        if (dbSettlementsByNameRegion.TryGetValue((settlement.Name, settlement.Region), out Settlement? dbSettlement))
                        {
                            _db.Entry(dbSettlement).State = EntityState.Detached;

                            settlement.Id = dbSettlement.Id;
                            _db.Settlements.Update(settlement);
                        }
                        else
                        {
                            _db.Settlements.Add(settlement);
                        }
                    }
                }

                foreach (var dbSettlement in dbSettlementsByNameRegion.Values)
                {
                    if (!settlementsByName.ContainsKey(dbSettlement.Name))
                    {
                        _db.Settlements.Remove(dbSettlement);
                    }
                }
            }

            private IEnumerable<Region> GetRegions() => Enum.GetValues(typeof(Region)).Cast<Region>();
        }
    }
}
