using System.Text.RegularExpressions;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands;

public record UpdateClanCommand : IMediatorRequest<ClanViewModel>
{
    public int UserId { get; init; }
    public int ClanId { get; init; }
    public string Tag { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public uint PrimaryColor { get; init; }
    public uint SecondaryColor { get; init; }
    public string BannerKey { get; init; } = string.Empty;
    public Region Region { get; init; }

    public class Validator : AbstractValidator<UpdateClanCommand>
    {
        public Validator(Constants constants)
        {
            RuleFor(c => c.Tag)
                .MinimumLength(constants.ClanTagMinLength)
                .MaximumLength(constants.ClanTagMaxLength)
                .Matches(new Regex(constants.ClanTagRegex, RegexOptions.Compiled));

            RuleFor(c => c.PrimaryColor)
                .GreaterThan(constants.ClanColorMinValue);

            RuleFor(c => c.SecondaryColor)
                .GreaterThan(constants.ClanColorMinValue);

            RuleFor(c => c.Name)
                .MinimumLength(constants.ClanNameMinLength)
                .MaximumLength(constants.ClanNameMaxLength);

            RuleFor(c => c.BannerKey)
                .MinimumLength(0)
                .MaximumLength(constants.ClanBannerKeyMaxLength)
                .Matches(new Regex(constants.ClanBannerKeyRegex, RegexOptions.Compiled));

            RuleFor(cmd => cmd.Region).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<UpdateClanCommand, ClanViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateClanCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IClanService _clanService;

        public Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService)
        {
            _db = db;
            _mapper = mapper;
            _clanService = clanService;
        }

        public async Task<Result<ClanViewModel>> Handle(UpdateClanCommand req, CancellationToken cancellationToken)
        {
            var clan = await _db.Clans
                .Where(c => c.Id == req.ClanId)
                .FirstOrDefaultAsync(cancellationToken);

            if (clan == null)
            {
                return new(CommonErrors.ClanNotFound(req.ClanId));
            }

            var userRes = await _clanService.GetClanMember(_db, req.UserId, req.ClanId, cancellationToken);
            if (userRes.Errors != null)
            {
                return new(userRes.Errors);
            }

            User user = userRes.Data!;
            if (user.ClanMembership!.Role != ClanMemberRole.Leader)
            {
                return new(CommonErrors.ClanMemberRoleNotMet(req.UserId, ClanMemberRole.Leader,
                    user.ClanMembership.Role));
            }

            var conflictingClan = await _db.Clans
                .FirstOrDefaultAsync(c => c.Tag == req.Tag || c.Name == req.Name, cancellationToken);
            if (conflictingClan != null)
            {
                return conflictingClan.Tag == req.Tag
                    ? new(CommonErrors.ClanTagAlreadyUsed(conflictingClan.Tag))
                    : new(CommonErrors.ClanNameAlreadyUsed(conflictingClan.Name));
            }

            clan.Tag = req.Tag;
            clan.PrimaryColor = req.PrimaryColor;
            clan.SecondaryColor = req.SecondaryColor;
            clan.Name = req.Name;
            clan.BannerKey = req.BannerKey;
            clan.Region = req.Region;

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' updated clan '{1}'", req.UserId, req.ClanId);
            return new(_mapper.Map<ClanViewModel>(clan));
        }
    }
}
