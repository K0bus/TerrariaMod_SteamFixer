using System;
using FixedAchievements.Common.Steam;
using FixedAchievements.Common.Terraria;
using Microsoft.Xna.Framework;
using Steamworks;
using Terraria;
using Terraria.Achievements;
using Terraria.ModLoader;
using Terraria.Social;
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
            if (SocialAPI.Mode != SocialMode.Steam)
            {
                string errorReply = GetTextValue("Mods.SteamFixer.SteamInactive");

                caller.Reply($"[SteamFixer] {errorReply}", Color.Red);
                return;
            }

            int pushed = 0;
            try
            {
                var stats = await SteamStatsService.GetPlayerStatsAsync(SteamUser.GetSteamID().ToString());
                foreach (var ach in stats.Achievements)
                {
                    if (ach.IsUnlocked)
                    {
                        Achievement gameAchievement = Main.Achievements.GetAchievement(ach.ApiName.ToUpper());
                        if (gameAchievement != null)
                        {
                            if (!gameAchievement.IsCompleted)
                            {
                                try
                                {
                                    foreach (var conditionName in TerrariaUtils.GetAchievementConditionNames(
                                                 gameAchievement))
                                    {
                                        var condition = gameAchievement.GetCondition(conditionName);
                                        if (condition != null)
                                        {
                                            condition.Complete();
                                        }
                                        else
                                        {
                                            Mod.Logger.Warn(
                                                $"[SteamFixer] Can't complete condition `{conditionName}` for {gameAchievement.Name}");
                                        }
                                    }

                                    Mod.Logger.Info(
                                        $"[SteamFixer] Complete achievement : {ach.ApiName} ({ach.DisplayName})");
                                    pushed++;
                                }
                                catch (Exception e)
                                {
                                    Mod.Logger.Error($"[SteamFixer] {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            Mod.Logger.Warn($"[SteamFixer] Can't find achievement : {ach.ApiName} ({ach.DisplayName})");
                        }
                    }
                }

                if (pushed > 0)
                    Main.Achievements.Save();
                string successReply = GetTextValue("Mods.SteamFixer.AchievementsPulled", pushed);

                caller.Reply($"[SteamFixer] {successReply}", Color.Green);
            }
            catch (Exception e)
            {
                caller.Reply($"[SteamFixer] {e.Message}", Color.Red);
            }
        }
    }
}
