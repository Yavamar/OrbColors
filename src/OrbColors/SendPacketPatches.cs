using BepInEx.Configuration;
using HarmonyLib;

namespace OrbColors
{
    [HarmonyPatch(typeof(Player), "Awake")]
    internal class PlayerPatch
    {
        public static bool Prefix()
        {
            Plugin.SendOrbColorPacket();
            return true;
        }
    }

    [HarmonyPatch(typeof(ConfigFile), "Save")]
    internal class ConfigFilePatch
    {
        public static bool Prefix()
        {
            Plugin.SendOrbColorPacket();
            return true;
        }
    }
}
