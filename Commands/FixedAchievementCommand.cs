using System;
using FixedAchievements.Common.Service;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FixedAchievements.Commands;

public class FixedAchievementCommand : ModCommand
{
    public override string Command => "saf";
    public override CommandType Type => CommandType.Chat;
    public override string Usage => "/saf <push/pull>";
    public override string Description => "Unified Steam Achievement Fixer commands";

    public override async void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0)
        {
            caller.Reply($"$Usage: {this.Usage}", Color.Red);
            return;
        }
        switch (args[0].ToLower())
        {
            case "pull":
                try
                {
                    var pulled = await AchievementService.PullAllSteamAchievements();
                    var successReplyPull = Language.GetTextValue("Mods.SteamFixer.AchievementsPulled", pulled);
                    caller.Reply($"[SteamFixer] {successReplyPull}", Color.Green);
                }
                catch (Exception e)
                {
                    caller.Reply($"[SteamFixer] {e.Message}", Color.Red);
                }
                break;

            case "push":
                var pushed = AchievementService.PushAllSteamAchievements();

                var successReplyPush = Language.GetTextValue("Mods.SteamFixer.AchievementsPushed", pushed);
                caller.Reply($"[SteamFixer] {successReplyPush}", Color.Green);
                break;

            default:
                caller.Reply($"Unknown subcommand: {args[0]}", Color.Red);
                caller.Reply($"$Usage: {this.Usage}", Color.Red);
                break;
        }
    }
}