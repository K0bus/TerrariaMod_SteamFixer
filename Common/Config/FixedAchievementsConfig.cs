using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace FixedAchievements.Common.Config;

public class FixedAchievementsConfig : ModConfig
{
    
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public override LocalizedText DisplayName => Language.GetText("Mods.SteamFixer.Config.DisplayName");
    
    [DefaultValue(true)]
    [LabelKey("$Mods.SteamFixer.Config.EnableAutoSync")]
    [TooltipKey("$Mods.SteamFixer.EnableAutoSyncTooltip")]
    public bool EnableAutoSync = true;
}