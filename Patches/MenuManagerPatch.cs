using System.Collections;
using System.IO;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace NotezDevStart.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        private static bool firstTimeLoad = true;

        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        static public void OnEnablePatch(MenuManager __instance)
        {
            if (!firstTimeLoad) return;

            if (__instance.menuButtons != null && __instance.menuButtons.name == "MainButtons")
            {
                NotezDevStart.Logger.LogInfo("MenuManager.OnEnablePatch() called - MainButtons");
                JumpInGame(__instance);
            }
            else
            {
                NotezDevStart.Logger.LogInfo("MenuManager.OnEnablePatch() called - not MainButtons");
            }
        }

        static private void JumpInGame(MenuManager __instance)
        {
            if (!Chainloader.PluginInfos.ContainsKey("dev.flero.lethal.FastStartup") && __instance.lanWarningContainer != null)
            {
                GameObject.Destroy(__instance.lanWarningContainer);
            }
            if (NotezDevStart.IsHostInstance)
            {
                __instance.lobbyNameInputField.text = "Placeholder"; // Because the game requires a name to be set to start the game
                __instance.ConfirmHostButton(); //? This is the same as clicking the "Host > Confirm" buttons
                __instance.lobbyNameInputField.text = "";
            }
            else
            {
                __instance.StartCoroutine(WaitForHostAndJoin(__instance));
            }

            firstTimeLoad = false;
        }

        static private IEnumerator WaitForHostAndJoin(MenuManager __instance)
        {
            NotezDevStart.Logger.LogInfo("Waiting for host to be ready...");
            yield return new WaitUntil(() => File.Exists(NotezDevStart.HostReadySignalPath));
            NotezDevStart.Logger.LogInfo("Host is ready, joining the game...");
            __instance.StartAClient();
        }
    }
}
