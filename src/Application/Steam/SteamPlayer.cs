using System;
using Crpg.Application.Common.Mappings;

namespace Crpg.Application.Steam
{
    public class SteamPlayer
    {
        public long SteamId { get; set; }
        public CommunityVisibilityState CommunityVisibilityState { get; set; }
        public int ProfileState { get; set; }
        public string PersonaName { get; set; }
        public long LastLogoff { get; set; }
        public bool CommentPermission { get; set; }
        public string ProfileUrl { get; set; }
        public Uri Avatar { get; set; }
        public Uri AvatarMedium { get; set; }
        public Uri AvatarFull { get; set; }
        public OnlineStatus PersonaState { get; set; }
        public string RealName { get; set; }
        public long PrimaryClanId { get; set; }
        public long TimeCreated { get; set; }
        public int PersonAStateFlags { get; set; }
        public string LocCountryCode { get; set; }
        public string LocStateCode { get; set; }
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