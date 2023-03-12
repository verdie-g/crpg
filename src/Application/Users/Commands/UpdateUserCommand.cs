using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record UpdateUserCommand : IMediatorRequest<UserViewModel>
{
    public int UserId { get; init; }

    public class Validator : AbstractValidator<UpdateUserCommand>
    {
        public Validator()
        {
        }
    }

    internal class Handler : IMediatorRequestHandler<UpdateUserCommand, UserViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateUserCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<UserViewModel>> Handle(UpdateUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            // This command doesn't do anything anymore but the code was not deleted because it can be useful later.

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' updated", req.UserId);

            return new Result<UserViewModel>(_mapper.Map<UserViewModel>(user));
        }
    }
}
