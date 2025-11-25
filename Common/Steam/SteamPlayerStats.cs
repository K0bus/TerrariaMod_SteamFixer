using System.Collections.Generic;

namespace FixedAchievements.Common.Steam
{
    public class SteamPlayerStats
    {
        public string SteamId { get; set; }
        public string GameName { get; set; }
        public string FriendlyGameName { get; set; }
        public float? HoursPlayed { get; set; }

        public List<SteamAchievement> Achievements { get; set; } = new();
    }
}
