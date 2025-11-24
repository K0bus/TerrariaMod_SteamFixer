using System.Collections.Generic;
using Steamworks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Social;

namespace SteamFixer
{
    public class SteamFixerPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            var count = 0;
            if (SocialAPI.Mode != SocialMode.Steam)
            {
                Main.NewText("[SteamFixer] Steam non actif ou indisponible.", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            foreach (var achievement in Main.Achievements.CreateAchievementsList())
            {
                if (achievement.IsCompleted)
                {
                    try
                    {
                        SteamFixer.sendCmdDelegate?.Invoke("grant:" + achievement.Name);
                        SteamFixer.GetInstance().GrantAchievement(achievement.Name);
                        SteamFixer.GetInstance().TryStoreStats();
                        count++;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            Main.NewText("[SteamFixer] " + count + " achievement(s) loaded on Steam.", Microsoft.Xna.Framework.Color.Green);
        }
    }
}