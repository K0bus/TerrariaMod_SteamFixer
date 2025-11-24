using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using Steamworks;
using Terraria;
using Terraria.Achievements;
using Terraria.ModLoader;
using Terraria.Social;

namespace SteamFixer
{
    public class SteamFixer : Mod
    {
        public static Action<string> sendCmdDelegate;
        private Hook onSetAchievement;
        private HashSet<string> granted = new();
		private static SteamFixer instance;

        public override void Load()
        {
			instance = this;
            granted = new();

            try
            {
                InitializeCMD();
                InitializeSocialAPI();
                RegisterAchievementHook();
                TryStoreStats();
            }
            catch (Exception ex)
            {
                // Défense en profondeur : ne pas planter le chargement du mod si qqchose casse
                Logger.Error($"SteamFixer Load() error: {ex}");
            }
        }

        private void AchievementCompletedHandler(Terraria.Achievements.Achievement achievement)
        {
            try
            {
                GrantAchievement(achievement.Name);   
                TryStoreStats();
            }
            catch (Exception e)
            {
                Logger.Warn($"SteamFixer: AchievementCompletedHandler exception: {e}");
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

                sendCmdDelegate = null;
            }
            catch (Exception e)
            {
                Logger.Warn($"SteamFixer Unload() error: {e}");
            }
        }

		public void GrantAchievement(string name)
		{
			if(!granted.Contains(name))
				if (sendCmdDelegate != null)
					{
						try
						{
							sendCmdDelegate.Invoke("grant:" + name);
							granted.Add(name);
						}
						catch (Exception e)
						{
							Logger.Warn($"SteamFixer: sendCmdDelegate failed for {name}: {e}");
						}
					}
		}

        public void TryStoreStats()
        {
            try { SteamUserStats.StoreStats(); }
            catch (Exception e) { Logger.Warn($"SteamFixer: StoreStats failed: {e}"); }
        }

        private void InitializeCMD()
        {
            Assembly terrAsm = typeof(Main).Assembly;

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
                Type t = terrAsm.GetType(tname);
                if (t == null) continue;
                // Cherche une méthode SendCmd qui prend un string (private static)
                sendCmdMethod = t.GetMethod("SendCmd", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
                if (sendCmdMethod != null) break;
            }

            if (sendCmdMethod == null)
            {
                // Fallback : chercher par nom méthode n'importe où
                sendCmdMethod = terrAsm.GetTypes()
                    .SelectMany(ty =>
                    {
                        try { return ty.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public); }
                        catch { return Array.Empty<MethodInfo>(); }
                    })
                    .FirstOrDefault(mi => mi.Name == "SendCmd" && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(string));
            }

            if (sendCmdMethod == null)
            {
                Logger.Warn("SteamFixer: SendCmd method not found via reflection. Steam grant path disabled.");
            }
            else
            {
                sendCmdDelegate = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), sendCmdMethod);
            }
        }

        private void InitializeSocialAPI()
        {
            try
            {
                if (SocialAPI.Mode == SocialMode.Steam)
                {
                    // Initialize without forcing a mode (if already initialised this est safe)
                    SocialAPI.Initialize();
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"SteamFixer: SocialAPI.Initialize threw: {e}");
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

		public static SteamFixer GetInstance()
		{
			return instance;
		}
    }
}
