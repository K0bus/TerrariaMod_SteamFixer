using System.Collections.Generic;
using System.Reflection;
using Terraria.Achievements;

namespace FixedAchievements.Common.Terraria;

public class TerrariaUtils
{
    public static IEnumerable<string> GetAchievementConditionNames(Achievement achievement)
    {
        var field = typeof(Achievement).GetField("_conditions", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) yield break;

        var dict = field.GetValue(achievement) as System.Collections.IDictionary;
        if (dict == null) yield break;

        foreach (System.Collections.DictionaryEntry entry in dict)
        {
            yield return entry.Key as string;
        }
    }
}