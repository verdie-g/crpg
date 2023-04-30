using System.Net;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record UpdateUserRegionCommand : IMediatorRequest<UserViewModel>
{
    public int UserId { get; init; }
    public IPAddress IpAddress { get; init; } = default!;

    internal class Handler : IMediatorRequestHandler<UpdateUserRegionCommand, UserViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateUserRegionCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IGeoIpService _geoIpService;

        public Handler(ICrpgDbContext db, IMapper mapper, IGeoIpService geoIpService)
        {
            _db = db;
            _mapper = mapper;
            _geoIpService = geoIpService;
        }

        public async Task<Result<UserViewModel>> Handle(UpdateUserRegionCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            Region? region = _geoIpService.ResolveRegionFromIp(req.IpAddress);
            if (region != null && region != user.Region)
            {
                Logger.LogInformation("User '{0}' changed region from '{1}' to '{2}'", user.Id, user.Region, region);
                user.Region = region;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return new(_mapper.Map<UserViewModel>(user));
        }
    }
}
