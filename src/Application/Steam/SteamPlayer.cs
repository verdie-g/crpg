using System;

namespace Crpg.Application.Steam
{
    public record SteamPlayer
    {
        public string SteamId { get; init; } = string.Empty;
        public CommunityVisibilityState CommunityVisibilityState { get; init; }
        public int ProfileState { get; init; }
        public string PersonaName { get; init; } = string.Empty;
        public long LastLogoff { get; init; }
        public int CommentPermission { get; init; }
        public string ProfileUrl { get; init; } = string.Empty;
        public Uri Avatar { get; init; } = default!;
        public Uri AvatarMedium { get; init; } = default!;
        public Uri AvatarFull { get; init; } = default!;
        public OnlineStatus PersonaState { get; init; }
        public string RealName { get; init; } = string.Empty;
        public string PrimaryClanId { get; init; } = string.Empty;
        public long TimeCreated { get; init; }
        public int PersonAStateFlags { get; init; }
        public string LocCountryCode { get; init; } = string.Empty;
        public string LocStateCode { get; init; } = string.Empty;
        public long LocCityId { get; init; }
    }

    public enum CommunityVisibilityState
    {
        Unknown = -1,
        Invisible = 0,
        Visible = 3,
    }

    public enum OnlineStatus
    {
        Unknown = -1,
        Offline = 0,
        Online = 1,
        Busy = 2,
        Away = 3,
        Snooze = 4,
        LookingToTrade = 5,
        LookingToPlay = 6,
    }
}
