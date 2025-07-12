using BepInEx.Configuration;
using CodeTalker.Networking;
using HarmonyLib;
using Mirror;

namespace OrbColors
{
    [HarmonyPatch(typeof(Player), "OnStartAuthority")]
    internal class PlayerPatch
    {
        public static void Postfix()
        {
            if(NetworkClient.active)
            {
                CodeTalkerNetwork.SendNetworkPacket(new PlayerJoinPacket());

                if (Plugin._customOrbColorEnabled.Value)
                {
                    //Plugin.Logger.LogMessage("Sending packet due to player joining.");
                    Plugin.SendOrbColorPacket();
                }
            }
        }
    }

    [HarmonyPatch(typeof(ConfigFile), "Save")]
    internal class ConfigFilePatch
    {
        public static bool Prefix(ConfigFile __instance)
        {
            
            if (NetworkClient.active && __instance.ConfigFilePath.EndsWith("OrbColors.cfg"))
            {
                //Plugin.Logger.LogMessage("Sending packet due to config being saved.");
                Plugin.SendOrbColorPacket();
            }

            return true;
        }
    }
}
