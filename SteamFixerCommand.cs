using System.Collections.Generic;
using Steamworks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Social;

namespace SteamFixer
{
    public class PushAllAchievementsCommand : ModCommand
    {
        // Type de commande : chat
        public override CommandType Type => CommandType.Chat;
        // Nom de la commande
        public override string Command => "pushallachievements";
        // Description
        public override string Description => "Pousse tous les achievements Terraria complétés vers Steam.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (SocialAPI.Mode != SocialMode.Steam)
            {
                caller.Reply("[SteamFixer] Steam non actif ou indisponible.", Microsoft.Xna.Framework.Color.Red);
                return;
            }
            
            int pushed = 0;
            HashSet<string> pushedSet = new HashSet<string>();

            // Parcours tous les achievements tML
            foreach (var achievement in Main.Achievements.CreateAchievementsList())
            {
                if (achievement.IsCompleted && !pushedSet.Contains(achievement.Name))
                {
                    try
                    {
                        // Pousser vers Steam
                        SteamFixer.sendCmdDelegate?.Invoke("grant:" + achievement.Name);
                        SteamUserStats.StoreStats();
                        pushedSet.Add(achievement.Name);
                        pushed++;
                    }
                    catch
                    {
                        // On ignore les erreurs pour ne pas interrompre la boucle
                        continue;
                    }
                }
            }

            foreach (var a in Main.Achievements.CreateAchievementsList())
            {
                bool achieved = false;
                if (SteamUserStats.GetAchievement(a.Name, out achieved))
                {
                    Mod.Logger.Info($"{a.Name} -> {(achieved ? "COMPLETED" : "NOT COMPLETED")}");
                }
            }

            caller.Reply($"[SteamFixer] Steam Achievement refreshed !", Microsoft.Xna.Framework.Color.Green);
        }
    }
}
