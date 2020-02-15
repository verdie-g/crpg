using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.System.Commands
{
    public class SeedDataCommand : IRequest
    {
        public class Handler : IRequestHandler<SeedDataCommand>
        {
            private readonly ITrpgDbContext _db;

            public Handler(ITrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                _db.Equipments.AddRange(new Equipment
                {
                    Name = "Good Sword",
                    Price = 200,
                    Type = EquipmentType.Weapon
                }, new Equipment
                {
                    Name = "Bad Sword",
                    Price = 100,
                    Type = EquipmentType.Weapon
                }, new Equipment
                {
                    Name = "Ok helmet",
                    Price = 150,
                    Type = EquipmentType.Head
                });

                _db.Users.AddRange(
                    new User
                    {
                        SteamId = 76561197987525637,
                        UserName = "takeoshigeru",
                        Money = 300,
                        Role = Role.SuperAdmin,
                        Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
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