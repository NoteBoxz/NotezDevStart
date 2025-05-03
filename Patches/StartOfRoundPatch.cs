using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace NotezDevStart.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static bool attemptedTp;

        [HarmonyPatch("TeleportPlayerInShipIfOutOfRoomBounds"), HarmonyPostfix]
        static public void TeleportPlayerInShipIfOutOfRoomBoundsStats(StartOfRound __instance)
        {
            if (!NotezDevStart.tpToEntrance || attemptedTp || !__instance.shipDoorsEnabled) return;

            NotezDevStart.Logger.LogInfo("Teleporting to entrance...");

            MethodInfo findEntranceMethod = AccessTools.Method(typeof(RoundManager), "FindMainEntranceScript");
            EntranceTeleport entrance = (EntranceTeleport)findEntranceMethod.Invoke(null, new object[] { NotezDevStart.teleportInside });

            if (entrance != null)
            {
                entrance.TeleportPlayer();
                NotezDevStart.Logger.LogInfo("Player teleported to entrance");
            }
            else
            {
                NotezDevStart.Logger.LogError("Failed to find entrance");
            }

            attemptedTp = true;
        }

        [HarmonyPatch("Start"), HarmonyPostfix]
        static public void StartPostfix(StartOfRound __instance)
        {
            if (NotezDevStart.IsHostInstance)
            {
                NotezDevStart.Logger.LogInfo("Host instance is ready, creating signal file...");
                try
                {
                    // Create the host ready signal file
                    File.WriteAllText(NotezDevStart.HostReadySignalPath, DateTime.Now.ToString());
                    NotezDevStart.Logger.LogInfo("Host signal file created successfully");
                }
                catch (Exception ex)
                {
                    NotezDevStart.Logger.LogError($"Failed to create host signal file: {ex.Message}");
                }
            }
        }
    }
}
