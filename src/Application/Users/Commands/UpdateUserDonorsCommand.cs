using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record UpdateUserDonorsCommand : IMediatorRequest
{
    public IList<string> PlatformUserIds { get; set; } = Array.Empty<string>();

    internal class Handler : IMediatorRequestHandler<UpdateUserDonorsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateUserDonorsCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(UpdateUserDonorsCommand request, CancellationToken cancellationToken)
        {
            var donorPlatformUserIds = request.PlatformUserIds.ToHashSet(StringComparer.Ordinal);
            var users = await _db.Users
                .Where(u => donorPlatformUserIds.Contains(u.PlatformUserId) || u.IsDonor)
                .ToArrayAsync(cancellationToken);

            List<int> donorsAdded = new();
            List<int> donorsRemoved = new();
            foreach (var user in users)
            {
                if (donorPlatformUserIds.Contains(user.PlatformUserId))
                {
                    if (!user.IsDonor)
                    {
                        user.IsDonor = true;
                        donorsAdded.Add(user.Id);
                    }
                }
                else if (user.IsDonor)
                {
                    user.IsDonor = false;
                    donorsRemoved.Add(user.Id);
                }
            }

            if (donorsRemoved.Count > 0)
            {
                var bannerUserItems = await _db.UserItems
                    .Where(ui => donorsRemoved.Contains(ui.UserId) && ui.BaseItem!.Type == ItemType.Banner)
                    .ToArrayAsync(cancellationToken);
                _db.UserItems.RemoveRange(bannerUserItems);
            }

            await _db.SaveChangesAsync(cancellationToken);

            if (donorsAdded.Count > 0)
            {
                Logger.LogInformation("Added users [{0}] as donors", string.Join(", ", donorsAdded));
            }

            if (donorsRemoved.Count > 0)
            {
                Logger.LogInformation("Removed users [{0}] as donors", string.Join(", ", donorsRemoved));
            }

            return Result.NoErrors;
        }
    }
}
