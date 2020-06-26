using System;

namespace Crpg.Application.Steam
{
    public class SteamPlayer
    {
        public string SteamId { get; set; } = string.Empty;
        public CommunityVisibilityState CommunityVisibilityState { get; set; }
        public int ProfileState { get; set; }
        public string PersonaName { get; set; } = string.Empty;
        public long LastLogoff { get; set; }
        public int CommentPermission { get; set; }
        public string ProfileUrl { get; set; } = string.Empty;
        public Uri Avatar { get; set; } = default!;
        public Uri AvatarMedium { get; set; } = default!;
        public Uri AvatarFull { get; set; } = default!;
        public OnlineStatus PersonaState { get; set; }
        public string RealName { get; set; } = string.Empty;
        public string PrimaryClanId { get; set; } = string.Empty;
        public long TimeCreated { get; set; }
        public int PersonAStateFlags { get; set; }
        public string LocCountryCode { get; set; } = string.Empty;
        public string LocStateCode { get; set; } = string.Empty;
        public long LocCityId { get; set; }
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
