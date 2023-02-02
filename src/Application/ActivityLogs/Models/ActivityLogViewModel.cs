using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.ActivityLogs;

namespace Crpg.Application.ActivityLogs.Models;

public record ActivityLogViewModel : IMapFrom<ActivityLog>
{
    public int Id { get; init; }
    public ActivityLogType Type { get; init; }
    public int UserId { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    public DateTime CreatedAt { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ActivityLog, ActivityLogViewModel>()
            .ForMember(l => l.Metadata, opt => opt.MapFrom(l =>
                l.Metadata.ToDictionary(m => new ActivityLogMetadata(m.Key, m.Value))));
    }
}
