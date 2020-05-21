using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using MediatR;

namespace Crpg.Application.Items.Commands
{
    public class CreateItemsCommand : IRequest
    {
        public IList<ItemCreation> Items { get; set; } = Array.Empty<ItemCreation>();

        public class Handler : IRequestHandler<CreateItemsCommand>
        {
            private readonly ICrpgDbContext _db;

            public Handler(ICrpgDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(CreateItemsCommand request, CancellationToken cancellationToken)
            {
                _db.Items.AddRange(request.Items.Select(ItemHelper.ToItem));
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}