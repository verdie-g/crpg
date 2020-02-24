using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Commands
{
    public class CreateItemsCommand : IRequest
    {
        public IReadOnlyList<ItemCreation> Items { get; set; }

        public class Handler : IRequestHandler<CreateItemsCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(CreateItemsCommand request, CancellationToken cancellationToken)
            {
                _db.Items.AddRange(request.Items.Select(i => new Item
                {
                    MbId = i.MbId,
                    Name = i.Name,
                    Image = i.Image,
                    Type = i.Type,
                    Value = i.Value,
                    Weight = i.Weight,
                    HeadArmor = i.HeadArmor,
                    BodyArmor = i.BodyArmor,
                    ArmArmor = i.ArmArmor,
                    LegArmor = i.LegArmor,
                    BodyLength = i.BodyLength,
                    ChargeDamage = i.ChargeDamage,
                    Maneuver = i.Maneuver,
                    Speed = i.Speed,
                    HitPoints = i.HitPoints,
                    ThrustDamageType = i.ThrustDamageType,
                    SwingDamageType = i.SwingDamageType,
                    Accuracy = i.Accuracy,
                    MissileSpeed = i.MissileSpeed,
                    StackAmount = i.StackAmount,
                    WeaponLength = i.WeaponLength,
                    PrimaryThrustDamage = i.PrimaryThrustDamage,
                    PrimaryThrustSpeed = i.PrimaryThrustSpeed,
                    PrimarySwingDamage = i.PrimarySwingDamage,
                    PrimarySwingSpeed = i.PrimarySwingSpeed,
                    PrimaryWeaponFlags = i.PrimaryWeaponFlags,
                    SecondaryThrustDamage = i.SecondaryThrustDamage,
                    SecondaryThrustSpeed = i.SecondaryThrustSpeed,
                    SecondarySwingDamage = i.SecondarySwingDamage,
                    SecondarySwingSpeed = i.SecondarySwingSpeed,
                    SecondaryWeaponFlags = i.SecondaryWeaponFlags,
                }));
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}