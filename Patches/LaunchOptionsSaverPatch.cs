using FastStartup;
using FastStartup.TimeSavers;
using HarmonyLib;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace NotezDevStart.Patches
{
    [HarmonyPatch(typeof(LaunchOptionsSaver))]
    internal class LaunchOptionsSaverPatch
    {
        [HarmonyPrefix, HarmonyPatch("Start")]
        internal static bool Start()
        {
            if (!Plugin.Config.SkipLaunchMode.Value) return true;
            if (LaunchOptionsSaver.HasRan) return true;
            if (!NotezDevStart.forceLANmode) return true;

            SceneManager.LoadScene("InitSceneLANMode");
            return false;
        }
    }
}
