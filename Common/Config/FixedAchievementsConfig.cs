using System.ComponentModel;
using FixedAchievements.Common.Steam;
using FixedAchievements.Common.Wrapper;
using Terraria;
using Terraria.ModLoader.Config;

namespace FixedAchievements.Common.Config;

public class FixedAchievementsConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("Options")]

    [DefaultValue(true)]
    [LabelKey("$Mods.SteamAchievementFixer.Config.EnableAutoSync")]
    public bool EnableAutoSync = true;

    [DefaultValue(false)]
    [LabelKey("$Mods.SteamAchievementFixer.Config.DebugLogs")]
    public bool DebugLogs = false;

    [Header("$Mods.SteamAchievementFixer.Config.ActionsHeader")]
    
    [LabelKey("$Mods.SteamAchievementFixer.Config.PushAchievementsAction")]
    [TooltipKey("$Mods.SteamAchievementFixer.Config.PushAchievementsTooltip")]
    public bool PushAchievementsAction { get; set; }

    [LabelKey("$Mods.SteamAchievementFixer.Config.PullAchievementsAction")]
    [TooltipKey("$Mods.SteamAchievementFixer.Config.PullAchievementsTooltip")]
    public bool ReloadSteamDataAction { get; set; }

    public override void OnChanged()
    {
        base.OnChanged();
        // Push all achievements button clicked
        if (PushAchievementsAction)
        {
            PushAchievementsAction = false; // reset the pseudo-button immediately
            SaveChanges();
            
            Main.NewText("[SteamAchievementFixer] PushAll triggered from config UI.");
            Log.Info("PushAll triggered from Config UI.");
        }

        // Reload steam data button clicked
        if (ReloadSteamDataAction)
        {
            ReloadSteamDataAction = false;
            SaveChanges();

            Main.NewText("[SteamAchievementFixer] Reload Steam Data triggered from config UI.");
            Log.Info("Reload Steam Data triggered from Config UI.");
        }
    }
}