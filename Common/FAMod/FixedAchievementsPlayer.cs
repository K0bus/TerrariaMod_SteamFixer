using FixedAchievements.Common.Config;
using FixedAchievements.Common.Service;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Social;

namespace FixedAchievements.Common.FAMod
{
    public class FixedAchievementsPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if(!ModContent.GetInstance<FixedAchievementsConfig>().EnableAutoSync)
                return;
            
            var pushed = AchievementService.PushAllSteamAchievements();
            
            string successReply = Language.GetTextValue("Mods.SteamFixer.AchievementsPushed", pushed);
            Main.NewText($"[SteamFixer] {successReply}", Color.Green);
        }
    }
}