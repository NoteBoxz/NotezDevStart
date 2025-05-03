using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NotezDevStart
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("dev.flero.lethal.FastStartup", BepInDependency.DependencyFlags.SoftDependency)]
    public class NotezDevStart : BaseUnityPlugin
    {
        public static NotezDevStart Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        private static Mutex? AppMutex { get; set; }
        public static bool autoJoinLan;
        public static bool autoPullLever;
        public static bool tpToEntrance;
        public static bool teleportInside;
        public static bool forceLANmode;
        public static int playersRequired;
        internal static bool IsHostInstance;
        public static readonly string TempFolderPath = Path.Combine(Path.GetTempPath(), "NotezDevStart");
        public static readonly string HostReadySignalPath = Path.Combine(TempFolderPath, "HostStarted");

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();
            ConfigFile();
            SetupTempFolder();
            IsHostInstance = !autoJoinLan || !CheckMutex();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void ConfigFile()
        {
            forceLANmode = Config.Bind("General", "ForceLANMode", true, "Force LAN mode on startup. (even if fast startup is installed)").Value;
            autoJoinLan = Config.Bind("General", "AutoJoinLAN", true, "Automatically join LAN lobbies when game is launched more than once.").Value;
            playersRequired = Config.Bind("General", "PlayersRequired", 2, "Number of players required to start the game. (including the host)").Value;
            autoPullLever = Config.Bind("General", "AutoPullLever", false, "Automatically pull the ship's lever on startup.").Value;
            tpToEntrance = Config.Bind("General", "TeleportToEntrance", false, "Automatically teleports you to the main entrance on level load (Requires 'AutoPullLever' enabled).").Value;
            teleportInside = Config.Bind("General", "TeleportInside", false, "Teleports you inside the facility instead (Requires 'TeleportToEntrance' enabled).").Value;
        }

        private void SetupTempFolder()
        {
            try
            {
                // Create or clear the temp folder
                if (Directory.Exists(TempFolderPath))
                {
                    // Delete any existing files
                    foreach (string file in Directory.GetFiles(TempFolderPath))
                    {
                        try { File.Delete(file); } catch { /* Ignore errors */ }
                    }
                }
                else
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(TempFolderPath);
                }

                Logger.LogDebug($"Temp folder setup at: {TempFolderPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to setup temp folder: {ex.Message}");
            }
        }

        internal static bool CheckMutex()
        {
            try
            {
                if (AppMutex == null) AppMutex = new Mutex(true, "LethalCompany-" + MyPluginInfo.PLUGIN_NAME);
                return AppMutex != null && !AppMutex.WaitOne(System.TimeSpan.Zero, true);
            }
            catch
            {
                return false;
            }
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Type[] types = GetTypesWithErrorHandling();

            // Patch everything except FilterEnemyTypesPatch
            foreach (var type in types)
            {
                try
                {
                    Harmony.PatchAll(type);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error patching type {type.FullName}: {e.Message}");
                    if (e.InnerException != null)
                    {
                        Logger.LogError($"Inner exception: {e.InnerException.Message}");
                    }
                }
            }

            Logger.LogDebug("Finished patching!");
        }

        private static Type[] GetTypesWithErrorHandling()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Logger.LogWarning("ReflectionTypeLoadException caught while getting types. Some types will be skipped.");
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Logger.LogWarning($"Loader Exception: {loaderException.Message}");
                    if (loaderException is FileNotFoundException fileNotFound)
                    {
                        Logger.LogWarning($"Could not load file: {fileNotFound.FileName}");
                    }
                }
                return e.Types.Where(t => t != null).ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"Unexpected error while getting types: {e.Message}");
                return new Type[0];
            }
        }


        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
