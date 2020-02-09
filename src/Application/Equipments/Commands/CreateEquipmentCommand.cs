using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Equipments.Commands
{
    public class CreateEquipmentCommand : IRequest<EquipmentViewModel>
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public EquipmentType Type { get; set; }

        public class Validator : AbstractValidator<CreateEquipmentCommand>
        {
            public Validator()
            {
                RuleFor(e => e.Name).NotEmpty();
                RuleFor(e => e.Price).GreaterThan(0);
                RuleFor(e => e.Type).IsInEnum();
            }
        }

        public class Handler : IRequestHandler<CreateEquipmentCommand, EquipmentViewModel>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<EquipmentViewModel> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
            {
                if (await _db.Equipments.AnyAsync(e => e.Name == request.Name, cancellationToken))
                {
                    throw new BadRequestException($"Equipment \"{request.Name}\" already exists");
                }

                var equipment = _db.Equipments.Add(new Equipment
                {
                    Name = request.Name,
                    Price = request.Price,
                    Type = request.Type,
                });
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<EquipmentViewModel>(equipment.Entity);
            }
        }
    }
}