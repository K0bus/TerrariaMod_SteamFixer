using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FixedAchievements.Common.Steam
{
    public static class SteamStatsService
    {
        private static readonly HttpClient _http = new();

        /// <summary>
        /// Télécharge et parse les stats Steam d’un joueur pour Terraria.
        /// </summary>
        public static async Task<SteamPlayerStats> GetPlayerStatsAsync(string steamId64)
        {
            string url = $"https://steamcommunity.com/profiles/{steamId64}/stats/105600/?xml=1";

            string xmlContent;

            try
            {
                xmlContent = await _http.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Impossible de télécharger les stats Steam : {ex.Message}", ex);
            }

            XDocument doc;

            try
            {
                doc = XDocument.Parse(xmlContent);
            }
            catch (Exception ex)
            {
                throw new Exception("Format XML Steam invalide", ex);
            }

            XElement root = doc.Element("playerstats")
                ?? throw new Exception("Le XML Steam ne contient pas de balise <playerstats>.");

            // Vérifier la confidentialité
            var privacy = root.Element("privacyState")?.Value;
            if (privacy != "public")
                throw new Exception("Le profil Steam est privé : impossible de lire les achievements.");

            var stats = new SteamPlayerStats
            {
                SteamId = steamId64,
                GameName = root.Element("game")?.Element("gameName")?.Value.Trim(),
                FriendlyGameName = root.Element("game")?.Element("gameFriendlyName")?.Value.Trim(),
                HoursPlayed = ParseFloat(root.Element("stats")?.Element("hoursPlayed")?.Value)
            };

            var achievementsNode = root.Element("achievements");
            if (achievementsNode != null)
            {
                foreach (var ach in achievementsNode.Elements("achievement"))
                {
                    stats.Achievements.Add(new SteamAchievement
                    {
                        IsUnlocked = ach.Attribute("closed")?.Value == "1",
                        IconClosedUrl = ach.Element("iconClosed")?.Value.Trim(),
                        IconOpenUrl = ach.Element("iconOpen")?.Value.Trim(),
                        DisplayName = ach.Element("name")?.Value.Trim(),
                        ApiName = ach.Element("apiname")?.Value.Trim(),
                        Description = ach.Element("description")?.Value.Trim(),
                        UnlockTimestamp = ParseLong(ach.Element("unlockTimestamp")?.Value)
                    });
                }
            }

            return stats;
        }

        // Helpers parsing
        private static float? ParseFloat(string s)
            => float.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : null;

        private static long? ParseLong(string s)
            => long.TryParse(s, out var v) ? v : null;
    }
}
