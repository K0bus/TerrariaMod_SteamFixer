using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;

namespace FixedAchievements
{
    public class SteamFixerPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if (SocialAPI.Mode != SocialMode.Steam)
            {
                string errorReply = Language.GetTextValue("Mods.SteamFixer.SteamInactive");

                Main.NewText($"[SteamFixer] {errorReply}", Color.Red);
                return;
            }

            int pushed = 0;

            foreach (var achievement in Main.Achievements.CreateAchievementsList())
            {
                if (achievement.IsCompleted)
                {
                    try
                    {
                        FixedAchievements.Instance.GrantAchievement(achievement.Name);
                        FixedAchievements.Instance.TryStoreStats();
                        pushed++;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            string successReply = Language.GetTextValue("Mods.SteamFixer.AchievementsPushed", pushed);

            Main.NewText($"[SteamFixer] {successReply}", Color.Green);
        }
    }
}