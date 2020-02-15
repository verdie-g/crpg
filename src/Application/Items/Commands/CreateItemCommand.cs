using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Items.Commands
{
    public class CreateItemCommand : IRequest<ItemViewModel>
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public ItemType Type { get; set; }

        public class Validator : AbstractValidator<CreateItemCommand>
        {
            public Validator()
            {
                RuleFor(i => i.Name).NotEmpty();
                RuleFor(i => i.Price).GreaterThan(0);
                RuleFor(i => i.Type).IsInEnum();
            }
        }

        public class Handler : IRequestHandler<CreateItemCommand, ItemViewModel>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ItemViewModel> Handle(CreateItemCommand request, CancellationToken cancellationToken)
            {
                if (await _db.Items.AnyAsync(i => i.Name == request.Name, cancellationToken))
                {
                    throw new BadRequestException($"Item \"{request.Name}\" already exists");
                }

                var item = _db.Items.Add(new Item
                {
                    Name = request.Name,
                    Price = request.Price,
                    Type = request.Type,
                });
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<ItemViewModel>(item.Entity);
            }
        }
    }
}