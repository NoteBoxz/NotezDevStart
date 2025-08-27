using System.Collections;
using BepInEx.Bootstrap;
using HarmonyLib;
using LethalLevelLoader;
using UnityEngine;

namespace NotezDevStart.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        private static bool hasPulledLever;

        [HarmonyPatch("Start"), HarmonyPostfix]
        static public void StartPatch(StartMatchLever __instance)
        {
            if (!NotezDevStart.autoPullLever || hasPulledLever) return;
            __instance.StartCoroutine(WaitForPlayersToPullLever(__instance));
        }

        public static IEnumerator WaitForPlayersToPullLever(StartMatchLever __instance)
        {
            NotezDevStart.Logger.LogInfo("Waiting to pull Ship Lever");
            yield return new WaitUntil(() => StartOfRound.Instance.connectedPlayersAmount + 1 >= NotezDevStart.playersRequired);
            if (Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader"))
            {
                NotezDevStart.Logger.LogInfo("Waiting for Lethal Level Loader to be ready...");
                yield return new WaitUntil(() => LLL_ISREADY());
            }
            NotezDevStart.Logger.LogInfo("Ship Lever pulled");
            // __instance.LeverAnimation();
            hasPulledLever = true;
            __instance.leverHasBeenPulled = true;
            __instance.leverAnimatorObject.SetBool("pullLever", true);
            __instance.triggerScript.interactable = false;
            __instance.PullLever();
        }

        public static bool LLL_ISREADY()
        {
            return NetworkBundleManager.Instance.allowedToLoadLevel.Value;
        }
    }
}
