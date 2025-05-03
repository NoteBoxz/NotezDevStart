using HarmonyLib;

namespace NotezDevStart.Patches
{
    //? Prevent the game from loading default settings before loading player settings (for example, full screen mode on startup)
    [HarmonyPatch(typeof(IngamePlayerSettings))]
    internal class IngamePlayerSettingsPatch
    {
        [HarmonyPostfix, HarmonyPatch("Awake")]
        private static void AwakePatch(IngamePlayerSettings __instance)
        {
            NotezDevStart.Logger.LogMessage("Loading settings from prefs before the game loads default settings");
            __instance.LoadSettingsFromPrefs();
            __instance.UpdateGameToMatchSettings();
        }
    }
}
