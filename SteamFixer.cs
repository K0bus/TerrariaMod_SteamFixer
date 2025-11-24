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

		public override void Load()
		{
			granted = new();

			try
			{
				// 1) Récupère l'assembly de Terraria (plus fiable que GetEntryAssembly() sous .NET6+)
				Assembly terrAsm = typeof(Main).Assembly;

				// 2) Deux emplacements possibles pour la classe TerrariaSteamClient selon version tML :
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
						.SelectMany(ty => {
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
					// Create delegate for static method (open static)
					sendCmdDelegate = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), sendCmdMethod);
				}

				// 3) Ensure SocialAPI is initialised for Steam (si we're en mode Steam)
				//    Use SocialAPI.Mode pour vérifier, et appeler Initialize() si nécessaire.
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

				foreach (var a in Main.Achievements.CreateAchievementsList())
				{
					if (a.IsCompleted)
						sendCmdDelegate?.Invoke("grant:" + a.Name);
				}

				try {
					SteamUserStats.StoreStats();
				}
				catch (Exception e)
				{
					Logger.Warn($"SteamFixer: StoreStats failed: {e}");
				}

				// 4) Hook sur CompleteAchievement — on enverra la commande "grant:<name>" au client steam
				//    On_AchievementsSocialModule.CompleteAchievement existe dans l'API tML et est safe à hooker.
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
				// 6) Hook SteamUserStats.SetAchievement -> on appelle StoreStats() après orig
				try
				{
					MethodInfo setAchievement = typeof(SteamUserStats).GetMethod("SetAchievement", new Type[] { typeof(string) });
					if (setAchievement != null)
					{
						MethodInfo detour = typeof(SteamFixer).GetMethod(nameof(StoreStats), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						onSetAchievement = new Hook(setAchievement, detour);
						onSetAchievement.Apply(); // sécurise le hook (Apply est optionnel selon version MonoMod)
					}
					else
					{
						Logger.Warn("SteamFixer: SteamUserStats.SetAchievement not found (class signature changed?).");
					}
				}
				catch (Exception e)
				{
					Logger.Warn($"SteamFixer: error hooking SetAchievement: {e}");
				}
				try
				{
					SteamUserStats.StoreStats();
				}
				catch (Exception e)
				{
					Logger.Warn($"SteamFixer: StoreStats failed: {e}");
				}
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
				string name = achievement.Name;

				// envoyer la commande Steam
				if (sendCmdDelegate != null)
				{
					try
					{
						sendCmdDelegate.Invoke("grant:" + name);
					}
					catch (Exception e)
					{
						Logger.Warn($"SteamFixer: sendCmdDelegate failed for {name}: {e}");
					}
				}
				try
				{
					SteamUserStats.StoreStats();
				}
				catch (Exception e)
				{
					Logger.Warn($"SteamFixer: StoreStats failed: {e}");
				}
			}
			catch (Exception e)
			{
				Logger.Warn($"SteamFixer: AchievementCompletedHandler exception: {e}");
			}
		}

		// MonoMod trampoline-style detour: orig delegate sera fourni par MonoMod
		// Signature : Func<string, bool> orig, string name  -> return bool (same as SteamUserStats.SetAchievement)
		public static bool StoreStats(Func<string, bool> orig, string name)
		{
			bool res = false;
			try
			{
				res = orig(name);
			}
			catch (Exception)
			{
				// On ignore pour garder résilience
			}

			// Force store des stats sur Steam
			try
			{
				SteamUserStats.StoreStats();
			}
			catch (Exception)
			{
				// ignore
			}
			return res;
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
				
				if (onSetAchievement != null)
				{
					try { onSetAchievement.Undo(); } catch { }
					try { onSetAchievement.Dispose(); } catch { }
					onSetAchievement = null;
				}

				sendCmdDelegate = null;
			}
			catch (Exception e)
			{
				Logger.Warn($"SteamFixer Unload() error: {e}");
			}
		}
	}
}
