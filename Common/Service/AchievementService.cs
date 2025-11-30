using System;
using System.Threading.Tasks;
using FixedAchievements.Common.Steam;
using FixedAchievements.Common.Terraria;
using FixedAchievements.Common.Wrapper;
using Microsoft.Xna.Framework;
using Steamworks;
using Terraria;
using Terraria.Achievements;
using Terraria.Localization;
using Terraria.Social;

namespace FixedAchievements.Common.Service;

public abstract class AchievementService
{
    // Logic to get achievements from steam and import these in game
    public async static Task<int> PullAllSteamAchievements()
    {
        if (SocialAPI.Mode != SocialMode.Steam)
        {
            string errorReply = Language.GetTextValue("Mods.SteamFixer.SteamInactive");
            throw new Exception($"[SteamFixer] {errorReply}");
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
                                        Log.Warn(
                                            $"[SteamFixer] Can't complete condition `{conditionName}` for {gameAchievement.Name}");
                                    }
                                }

                                Log.Info(
                                    $"[SteamFixer] Complete achievement : {ach.ApiName} ({ach.DisplayName})");
                                pushed++;
                            }
                            catch (Exception e)
                            {
                                Log.Error($"[SteamFixer] {e.Message}");
                            }
                        }
                    }
                    else
                    {
                        Log.Warn($"[SteamFixer] Can't find achievement : {ach.ApiName} ({ach.DisplayName})");
                    }
                }
            }

            if (pushed > 0)
                Main.Achievements.Save();
            return pushed;
        }
        catch (Exception e)
        {
            throw new Exception($"[SteamFixer] {e.Message}");
        }
    }

    // Logic to get achievements from game and import it to Steam
    public static int PushAllSteamAchievements()
    {
        if (SocialAPI.Mode != SocialMode.Steam)
        {
            string errorReply = Language.GetTextValue("Mods.SteamFixer.SteamInactive");

            Main.NewText($"[SteamFixer] {errorReply}", Color.Red);
            return 0;
        }

        int pushed = 0;

        foreach (var achievement in Main.Achievements.CreateAchievementsList())
        {
            if (achievement.IsCompleted)
            {
                try
                {
                    PushSteamAchievement(achievement.Name);
                    FixedAchievements.Instance.TryStoreStats();
                    pushed++;
                }
                catch
                {
                    continue;
                }
            }
        }

        return pushed;
    }

    public static void PushSteamAchievement(string achievementsName)
    {
        if (FixedAchievements.SendCmdDelegate != null && !FixedAchievements.Granted.Contains(achievementsName))
        {
            try
            {
                FixedAchievements.SendCmdDelegate.Invoke("grant:" + achievementsName);
                FixedAchievements.Granted.Add(achievementsName);
            }
            catch (Exception e)
            {
                Log.Warn($"[SteamFixer] SendCmdDelegate failed for {achievementsName}: {e}");
            }
        }
    }
}