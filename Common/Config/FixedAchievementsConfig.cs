using System.ComponentModel;
using FixedAchievements.Common.Wrapper;
using Terraria;
using Terraria.ModLoader.Config;

namespace FixedAchievements.Common.Config;

public class FixedAchievementsConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [DefaultValue(true)]
    [LabelKey("$Mods.SteamFixer.Config.EnableAutoSync")]
    [TooltipKey("$Mods.SteamFixer.EnableAutoSyncTooltip")]
    public bool EnableAutoSync = true;
}