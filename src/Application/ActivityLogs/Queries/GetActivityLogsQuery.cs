using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.ActivityLogs.Queries;

public record GetActivityLogsQuery : IMediatorRequest<IList<ActivityLogViewModel>>
{
    public int? AfterId { get; init; }
    public int Count { get; init; }

    public class Validator : AbstractValidator<GetActivityLogsQuery>
    {
        public Validator()
        {
            RuleFor(q => q.Count).InclusiveBetween(1, 100);
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
            var query = req.AfterId is { } afterId
                ? _db.ActivityLogs.Where(l => l.Id > afterId)
                : _db.ActivityLogs;
            var activityLogs = await query
                .Include(l => l.Metadata)
                .OrderByDescending(l => l.CreatedAt)
                .Take(req.Count)
                .ToArrayAsync(cancellationToken);
            return new(_mapper.Map<IList<ActivityLogViewModel>>(activityLogs));
        }
    }
}
