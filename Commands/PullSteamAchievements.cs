using System;
using FixedAchievements.Common.Service;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using static Terraria.Localization.Language;

namespace FixedAchievements.Commands
{
    public class PullSteamAchievements : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "pullsteamachievements";

        public override string Description => GetTextValue("Mods.SteamFixer.AchievementPullCommandDescription");

        public override async void Action(CommandCaller caller, string input, string[] args)
        {
            try
            {
                var pushed = await AchievementService.PullAllSteamAchievements();
                var successReply = GetTextValue("Mods.SteamFixer.AchievementsPulled", pushed);
                caller.Reply($"[SteamFixer] {successReply}", Color.Green);
            }
            catch (Exception e)
            {
                caller.Reply($"[SteamFixer] {e.Message}", Color.Red);
            }
        }
    }
}
