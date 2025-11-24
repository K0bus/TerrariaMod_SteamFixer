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

            // Parcours tous les achievements tML
            foreach (var achievement in Main.Achievements.CreateAchievementsList())
            {
                if (achievement.IsCompleted)
                {
                    try
                    {
                        // Pousser vers Steam
                        SteamFixer.GetInstance().GrantAchievement(achievement.Name);
                        SteamFixer.GetInstance().TryStoreStats();
                        pushed++;
                    }
                    catch
                    {
                        // On ignore les erreurs pour ne pas interrompre la boucle
                        continue;
                    }
                }
            }

            caller.Reply($"[SteamFixer] " + pushed + " Steam Achievement refreshed !", Microsoft.Xna.Framework.Color.Green);
        }
    }
}
