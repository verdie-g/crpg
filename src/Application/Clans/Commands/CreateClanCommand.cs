using System.Text.RegularExpressions;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands;

public record CreateClanCommand : IMediatorRequest<ClanViewModel>
{
    public int UserId { get; init; }
    public string Tag { get; init; } = string.Empty;
    public uint PrimaryColor { get; init; }
    public uint SecondaryColor { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BannerKey { get; init; } = string.Empty;

    public class Validator : AbstractValidator<CreateClanCommand>
    {
        public Validator(Constants constants)
        {
            RuleFor(c => c.Tag)
                .MinimumLength(constants.ClanTagMinLength)
                .MaximumLength(constants.ClanTagMaxLength)
                .Matches(new Regex(constants.ClanTagRegex, RegexOptions.Compiled));

            RuleFor(c => c.Name)
                .MinimumLength(constants.ClanNameMinLength)
                .MaximumLength(constants.ClanNameMaxLength);

            RuleFor(c => c.BannerKey)
                .MinimumLength(0)
                .MaximumLength(constants.ClanBannerKeyMaxLength)
                .Matches(new Regex(constants.ClanBannerKeyRegex, RegexOptions.Compiled));
        }
    }

    internal class Handler : IMediatorRequestHandler<CreateClanCommand, ClanViewModel>
    {
        /// <summary>Mask to get alpha from ARGB to avoid having invisible colors.</summary>
        private const uint AlphaMask = 0xFF000000;

        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateClanCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<ClanViewModel>> Handle(CreateClanCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.ClanMembership != null)
            {
                return new(CommonErrors.UserAlreadyInAClan(req.UserId));
            }

            var clan = await _db.Clans
                .FirstOrDefaultAsync(c => c.Tag == req.Tag || c.Name == req.Name, cancellationToken);
            if (clan != null)
            {
                return clan.Tag == req.Tag
                    ? new(CommonErrors.ClanTagAlreadyUsed(clan.Tag))
                    : new(CommonErrors.ClanNameAlreadyUsed(clan.Name));
            }

            clan = new Clan
            {
                Tag = req.Tag,
                PrimaryColor = req.PrimaryColor | AlphaMask,
                SecondaryColor = req.SecondaryColor | AlphaMask,
                Name = req.Name,
                BannerKey = req.BannerKey,
                Members =
                {
                    new ClanMember
                    {
                        User = user,
                        Role = ClanMemberRole.Leader,
                    },
                },
            };

            _db.Clans.Add(clan);
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' created clan '[{1}] {2}' ({3})", req.UserId, req.Tag, req.Name, clan.Id);
            return new(_mapper.Map<ClanViewModel>(clan));
        }
    }
}
