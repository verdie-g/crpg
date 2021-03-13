using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public class UpdateStrategusUserMovementCommand : IMediatorRequest<StrategusUserViewModel>
    {
        public int UserId { get; set; }
        public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;

        internal class Handler : IMediatorRequestHandler<UpdateStrategusUserMovementCommand, StrategusUserViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusUserMovementCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<StrategusUserViewModel>> Handle(UpdateStrategusUserMovementCommand req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.StrategusUser)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserNotFound(req.UserId));
                }

                if (user.StrategusUser == null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserNotRegisteredToStrategus(req.UserId));
                }

                user.StrategusUser.Waypoints = req.Waypoints;

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' updated their movement", req.UserId);
                return new Result<StrategusUserViewModel>(_mapper.Map<StrategusUserViewModel>(user.StrategusUser));
            }
        }
    }
}
