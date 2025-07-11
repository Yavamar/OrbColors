using BepInEx.Configuration;
using HarmonyLib;
using Mirror;

namespace OrbColors
{
    [HarmonyPatch(typeof(Player), "Awake")]
    internal class PlayerPatch
    {
        public static bool Prefix(Player __instance)
        {
            Plugin.Logger.LogMessage("Sending packet due to player joining.");
            Plugin.SendOrbColorPacket();

            return true;
        }
    }

    [HarmonyPatch(typeof(ConfigFile), "Save")]
    internal class ConfigFilePatch
    {
        public static bool Prefix(ConfigFile __instance)
        {
            
            if (NetworkClient.active && __instance.ConfigFilePath.EndsWith("OrbColors.cfg"))
            {
                Plugin.Logger.LogMessage("Sending packet due to config being saved.");
                Plugin.SendOrbColorPacket();
            }

            return true;
        }
    }
}
