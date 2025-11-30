using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using FixedAchievements.Common.Service;
using log4net;
using Steamworks;
using Terraria;
using Terraria.Achievements;
using Terraria.ModLoader;
using Terraria.Social;

namespace FixedAchievements;

public class FixedAchievements : Mod
{
    public static FixedAchievements Instance { get; private set; }

    public static Action<string> SendCmdDelegate;
    internal static ILog LoggerInstance;
    
    public static HashSet<string> granted = [];

    public override void Load()
    {
        Instance = this;
        LoggerInstance = Logger;
        try
        {
            InitializeCmd();
            InitializeSocialApi();
            RegisterAchievementHook();
            TryStoreStats();
        }
        catch (Exception e)
        {
            // Défense en profondeur : ne pas planter le chargement du mod si qqchose casse
            Logger.Error($"[SteamFixer] Load() error: {e}");
        }
    }

    private void AchievementCompletedHandler(Achievement achievement)
    {
        try
        {
            AchievementService.PushSteamAchievement(achievement.Name);   
            TryStoreStats();
        }
        catch (Exception e)
        {
            Logger.Warn($"[SteamFixer] AchievementCompletedHandler Exception: {e}");
        }
    }

    public override void Unload()
    {
        try
        {
            // Déhook proprement
            if (Main.Achievements != null)
            {
                Main.Achievements.OnAchievementCompleted -= AchievementCompletedHandler;
            }

            SendCmdDelegate = null;
        }
        catch (Exception e)
        {
            Logger.Warn($"[SteamFixer] Unload() Exception: {e}");
        }
    }

    public void TryStoreStats()
    {
        try { SteamUserStats.StoreStats(); }
        catch (Exception e) { Logger.Warn($"[SteamFixer] StoreStats failed: {e}"); }
    }

    private void InitializeCmd()
    {
        Assembly assembly = typeof(Main).Assembly;

        string[] candidateTypes =
        {
            "Terraria.ModLoader.Engine.TerrariaSteamClient",
            "Terraria.ModLoader.Engine.Steam.TerrariaSteamClient",
            "Terraria.TerrariaSteamClient",
            "Terraria.ModLoader.TerrariaSteamClient"
        };

        MethodInfo sendCmdMethod = null;

        foreach (string tname in candidateTypes)
        {
            Type type = assembly.GetType(tname);

            if (type == null) continue;

            // Cherche une méthode SendCmd qui prend un string (private static)
            sendCmdMethod = type.GetMethod("SendCmd", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, [typeof(string)], null);

            if (sendCmdMethod != null) break;
        }

        if (sendCmdMethod == null)
        {
            // Fallback : chercher par nom méthode n'importe où
            sendCmdMethod = assembly.GetTypes()
                .SelectMany(type =>
                {
                    try { return type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public); }
                    catch { return []; }
                })
                .FirstOrDefault(method => 
                method.Name == "SendCmd" && 
                method.GetParameters().Length == 1 && 
                method.GetParameters()[0].ParameterType == typeof(string));
        }

        if (sendCmdMethod == null)
        {
            Logger.Warn("[SteamFixer] SendCmd method not found via reflection. Steam grant path disabled.");
        }
        else
        {
            SendCmdDelegate = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), sendCmdMethod);
        }
    }

    private void InitializeSocialApi()
    {
        try
        {
            if (SocialAPI.Mode == SocialMode.Steam)
            {
                // Initialize without forcing a mode (if already initialised this is safe)
                SocialAPI.Initialize();
            }
        }
        catch (Exception e)
        {
            Logger.Warn($"[SteamFixer] SocialAPI.Initialize threw: {e}");
        }
    }

    private void RegisterAchievementHook()
    {
        try
        {
            if (Main.Achievements != null)
            {
                Main.Achievements.OnAchievementCompleted += AchievementCompletedHandler;
            }
            else
            {
                Logger.Warn("SteamFixer: Main.Achievements is null — cannot hook achievements");
            }
        }
        catch (Exception e)
        {
            Logger.Warn($"SteamFixer: failed to hook OnAchievementCompleted: {e}");
        }
    }
}
