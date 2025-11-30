using FixedAchievements.Common.Service;
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

        public override string Description => Language.GetTextValue("Mods.SteamFixer.AchievementPushCommandDescription");

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var pushed = AchievementService.PushAllSteamAchievements();

            var successReply = Language.GetTextValue("Mods.SteamFixer.AchievementsPushed", pushed);
            caller.Reply($"[SteamFixer] {successReply}", Color.Green);
        }
    }
}
