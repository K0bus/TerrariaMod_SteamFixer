using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;

namespace FixedAchievements.Commands
{
    public class PushAllAchievementsCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "pushallachievements";

        public override string Description => Language.GetTextValue("Mods.SteamFixer.AchievementCommandDescription");

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (SocialAPI.Mode != SocialMode.Steam)
            {
                string errorReply = Language.GetTextValue("Mods.SteamFixer.SteamInactive");

                caller.Reply($"[SteamFixer] {errorReply}", Color.Red);
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
                        FixedAchievements.Instance.GrantAchievement(achievement.Name);
                        FixedAchievements.Instance.TryStoreStats();

                        pushed++;
                    }
                    catch
                    {
                        // On ignore les erreurs pour ne pas interrompre la boucle
                        continue;
                    }
                }
            }

            string successReply = Language.GetTextValue("Mods.SteamFixer.AchievementsPushed", pushed);

            caller.Reply($"[SteamFixer] {successReply}", Color.Green);
        }
    }
}
