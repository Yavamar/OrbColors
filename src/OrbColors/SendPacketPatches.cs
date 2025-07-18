using BepInEx.Configuration;
using CodeTalker.Networking;
using HarmonyLib;
using Mirror;
using Nessie.ATLYSS.EasySettings;
using Nessie.ATLYSS.EasySettings.UIElements;

namespace OrbColors
{
    [HarmonyPatch(typeof(Player), "OnStartAuthority")]
    internal class PlayerPatch
    {
        public static void Postfix()
        {
            if(NetworkClient.active)
            {
                Plugin.InitConfig();
                Plugin.AddSettings();
                CodeTalkerNetwork.SendNetworkPacket(new PlayerJoinPacket());

                Plugin.SetOfflineOrbColor();
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

                Plugin.SetOfflineOrbColor();
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CharacterSelectManager), "Select_CharacterFile")]
    internal class CharacterSelectManagerPatch
    {
        public static void Prefix()
        {
            // Clear my orb color between sessions
            Plugin._color = [];

            // Clear the cache of steamIDs and orb colors between sessions.
            Plugin._playerOrbColors = [];

            // Clear the OrbColors settings off the mod tab. They will be applied again later once the new character is chosen.
            foreach(BaseAtlyssElement element in Plugin._settingsElements)
            {
                Settings.ModTab.ContentElements.Remove(element);
                element.Root.gameObject.SetActive(false);
            }
            Plugin._settingsElements.Clear();
        }
    }
}
