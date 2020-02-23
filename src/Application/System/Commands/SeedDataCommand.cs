using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;

namespace Crpg.Application.System.Commands
{
    public class SeedDataCommand : IRequest
    {
        public class Handler : IRequestHandler<SeedDataCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                _db.Items.AddRange(new Item
                {
                    Name = "Good Sword",
                    Value = 200,
                    Type = ItemType.TwoHandedWeapon
                }, new Item
                {
                    Name = "Bad Sword",
                    Value = 100,
                    Type = ItemType.OneHandedWeapon
                }, new Item
                {
                    Name = "Ok helmet",
                    Value = 150,
                    Type = ItemType.HeadArmor
                });

                _db.Users.AddRange(
                    new User
                    {
                        SteamId = 76561197987525637,
                        UserName = "takeoshigeru",
                        Golds = 300,
                        Role = Role.SuperAdmin,
                        AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
                        AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
                        AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_medium.jpg"),
                        Characters = new List<Character>
                        {
                            new Character
                            {
                                Name = "takeoshigeru",
                                Level = 23,
                                Experience = 2529284,
                            },
                            new Character
                            {
                                Name = "totoalala",
                                Level = 12,
                                Experience = 13493,
                            },
                            new Character
                            {
                                Name = "jackie",
                                Level = 1,
                                Experience = 200,
                            },
                        }
                    });

                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}