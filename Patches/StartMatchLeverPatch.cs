using System.Collections;
using HarmonyLib;
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
            NotezDevStart.Logger.LogInfo("Ship Lever pulled");
            // __instance.LeverAnimation();
            hasPulledLever = true;
            __instance.leverHasBeenPulled = true;
            __instance.leverAnimatorObject.SetBool("pullLever", true);
            __instance.triggerScript.interactable = false;
            __instance.PullLever();
        }
    }
}
