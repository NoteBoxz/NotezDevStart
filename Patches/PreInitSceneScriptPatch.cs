using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace NotezDevStart.Patches
{
    [HarmonyPatch(typeof(PreInitSceneScript))]
    public static class PreInitSceneScriptPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SkipToFinalSetting")]
        public static bool SkipToFinalSetting(PreInitSceneScript __instance)
        {
            if (Chainloader.PluginInfos.ContainsKey("dev.flero.lethal.FastStartup"))
            {
                NotezDevStart.Logger.LogInfo("FastStartup is installed, skipping final setting patch.");
                return true;
            }
            if (!NotezDevStart.forceLANmode) return true;

            __instance.launchSettingsPanelsContainer.SetActive(false);
            SceneManager.LoadScene("InitSceneLANMode");
            return false;
        }
    }
}
