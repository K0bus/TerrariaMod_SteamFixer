using System;

namespace FixedAchievements.Common.Steam
{
    public class SteamAchievement
    {
        public string ApiName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; }
        public long? UnlockTimestamp { get; set; }
        public string IconClosedUrl { get; set; }
        public string IconOpenUrl { get; set; }

        public DateTime? UnlockDate =>
            UnlockTimestamp.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(UnlockTimestamp.Value).DateTime
                : null;
    }
}
