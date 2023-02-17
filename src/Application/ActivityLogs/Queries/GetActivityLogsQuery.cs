using AutoMapper;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.ActivityLogs.Queries;

public record GetActivityLogsQuery : IMediatorRequest<IList<ActivityLogViewModel>>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public int[] UserIds { get; init; } = Array.Empty<int>();

    public class Validator : AbstractValidator<GetActivityLogsQuery>
    {
        public Validator()
        {
            RuleFor(l => l.From).LessThan(l => l.To);
        }
    }

    internal class Handler : IMediatorRequestHandler<GetActivityLogsQuery, IList<ActivityLogViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<ActivityLogViewModel>>> Handle(GetActivityLogsQuery req,
            CancellationToken cancellationToken)
        {
            var activityLogs = await _db.ActivityLogs
                .Include(l => l.Metadata)
                .Where(l => l.CreatedAt >= req.From && l.CreatedAt <= req.To && (req.UserIds.Length == 0 || req.UserIds.Contains(l.UserId)))
                .OrderByDescending(l => l.CreatedAt)
                .Take(1000)
                .ToArrayAsync(cancellationToken);
            return new(_mapper.Map<IList<ActivityLogViewModel>>(activityLogs));
        }
    }
}
