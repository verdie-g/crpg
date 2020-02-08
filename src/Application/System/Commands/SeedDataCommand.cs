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
                    });

                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}