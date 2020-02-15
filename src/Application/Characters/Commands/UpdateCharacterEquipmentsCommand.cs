using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Characters.Commands
{
    public class UpdateCharacterEquipmentsCommand : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }
        public int? HeadEquipmentId { get; set; }
        public int? BodyEquipmentId { get; set; }
        public int? LegsEquipmentId { get; set; }
        public int? GlovesEquipmentId { get; set; }
        public int? Weapon1EquipmentId { get; set; }
        public int? Weapon2EquipmentId { get; set; }
        public int? Weapon3EquipmentId { get; set; }
        public int? Weapon4EquipmentId { get; set; }

        public class Handler : IRequestHandler<UpdateCharacterEquipmentsCommand, CharacterViewModel>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(UpdateCharacterEquipmentsCommand request,
                CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .Include(c => c.HeadEquipment)
                    .Include(c => c.BodyEquipment)
                    .Include(c => c.LegsEquipment)
                    .Include(c => c.GlovesEquipment)
                    .Include(c => c.Weapon1Equipment)
                    .Include(c => c.Weapon2Equipment)
                    .Include(c => c.Weapon3Equipment)
                    .Include(c => c.Weapon4Equipment)
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);

                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId, request.UserId);
                }

                await UpdateCharacterEquipments(request, character);
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<CharacterViewModel>(character);
            }

            private async Task UpdateCharacterEquipments(UpdateCharacterEquipmentsCommand request, Character character)
            {
                var ids = BuildEquipmentIdCollection(request);
                var equipmentsById = await _db.UserEquipments
                    .Include(ue => ue.Equipment)
                    .Where(ue => ids.Contains(ue.EquipmentId) && ue.UserId == request.UserId)
                    .ToDictionaryAsync(ue => ue.EquipmentId, ue => ue.Equipment);

                character.HeadEquipment = GetEquipmentWithChecks(request.HeadEquipmentId, EquipmentType.Head, equipmentsById);
                character.BodyEquipment = GetEquipmentWithChecks(request.BodyEquipmentId, EquipmentType.Body, equipmentsById);
                character.LegsEquipment = GetEquipmentWithChecks(request.LegsEquipmentId, EquipmentType.Legs, equipmentsById);
                character.GlovesEquipment = GetEquipmentWithChecks(request.GlovesEquipmentId, EquipmentType.Gloves, equipmentsById);
                character.Weapon1Equipment = GetEquipmentWithChecks(request.Weapon1EquipmentId, EquipmentType.Weapon, equipmentsById);
                character.Weapon2Equipment = GetEquipmentWithChecks(request.Weapon2EquipmentId, EquipmentType.Weapon, equipmentsById);
                character.Weapon3Equipment = GetEquipmentWithChecks(request.Weapon3EquipmentId, EquipmentType.Weapon, equipmentsById);
                character.Weapon4Equipment = GetEquipmentWithChecks(request.Weapon4EquipmentId, EquipmentType.Weapon, equipmentsById);
            }

            private Equipment GetEquipmentWithChecks(int? id, EquipmentType expectedType,
                Dictionary<int, Equipment> equipmentsById)
            {
                if (id == null)
                {
                    return null;
                }

                if (!equipmentsById.TryGetValue(id.Value, out var equipment) || equipment.Type != expectedType)
                {
                    throw new BadRequestException($"Unexpected equipment for {expectedType}");
                }

                return equipment;
            }

            private IEnumerable<int> BuildEquipmentIdCollection(UpdateCharacterEquipmentsCommand request)
            {
                var ids = new List<int>();
                if (request.HeadEquipmentId != null)
                    ids.Add(request.HeadEquipmentId.Value);
                if (request.BodyEquipmentId != null)
                    ids.Add(request.BodyEquipmentId.Value);
                if (request.LegsEquipmentId != null)
                    ids.Add(request.LegsEquipmentId.Value);
                if (request.GlovesEquipmentId != null)
                    ids.Add(request.GlovesEquipmentId.Value);
                if (request.Weapon1EquipmentId != null)
                    ids.Add(request.Weapon1EquipmentId.Value);
                if (request.Weapon2EquipmentId != null)
                    ids.Add(request.Weapon2EquipmentId.Value);
                if (request.Weapon3EquipmentId != null)
                    ids.Add(request.Weapon3EquipmentId.Value);
                if (request.Weapon4EquipmentId != null)
                    ids.Add(request.Weapon4EquipmentId.Value);

                return ids;
            }
        }
    }
}