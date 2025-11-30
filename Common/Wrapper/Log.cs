namespace FixedAchievements.Common.Wrapper;

public static class Log
{
    public static void Info(string msg) => FixedAchievements.LoggerInstance.Info(msg);
    public static void Error(string msg) => FixedAchievements.LoggerInstance.Error(msg);
    public static void Debug(string msg) => FixedAchievements.LoggerInstance.Debug(msg);
    public static void Warn(string msg) => FixedAchievements.LoggerInstance.Warn(msg);
}
