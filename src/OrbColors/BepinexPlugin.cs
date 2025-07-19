using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CodeTalker.Networking;
using CodeTalker.Packets;
using HarmonyLib;
using Mirror;
using Nessie.ATLYSS.EasySettings;
using Nessie.ATLYSS.EasySettings.UIElements;
using UnityEngine;

namespace OrbColors
{
    /*
      Here are some basic resources on code style and naming conventions to help
      you in your first CSharp plugin!

      https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
      https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
      https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
    */

    [BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        // We store the ConfigEntry's such that we can retrieve their values as they will be updated when the settings are.
        internal static ConfigEntry<bool> _orbColorEnabled;
        internal static Dictionary<string, ConfigEntry<float>> _color = [];
        internal static ConfigEntry<float> _size;

        internal static Dictionary<string, (bool _orbColorsEnabled, Color _color, float _size)> _playerOrbColors = [];
        internal static List<BaseAtlyssElement> _settingsElements = [];

        internal static new ManualLogSource Logger = null!;
        internal static new ConfigFile Config = null!;

        private void Awake()
        {
            Logger = base.Logger;
            Config = base.Config;

            Logger.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

            new Harmony(LCMPluginInfo.PLUGIN_GUID).PatchAll(Assembly.GetExecutingAssembly());

            base.Config.SaveOnConfigSet = false;

            Settings.OnApplySettings.AddListener(() => { base.Config.Save(); });

            CodeTalkerNetwork.RegisterListener<OrbColorPacket>(ReceiveOrbColorPacket);
            CodeTalkerNetwork.RegisterListener<PlayerJoinPacket>(ReceivePlayerJoinPacket);
        }

        internal static void InitConfig()
        {
            string headerName = $"File {ProfileDataManager._current._selectedFileIndex+1}: {ProfileDataManager._current._characterFile._nickName}";

            ConfigDefinition enabledDef = new(headerName, "OrbColorsEnabled");
            ConfigDescription enabledDesc = new("Use Custom Orb Color?");
            _orbColorEnabled = Config.Bind(enabledDef, false, enabledDesc);

            Dictionary<string, float> values = [];
            values.Add("Red", 0.5f);
            values.Add("Green", 0.5f);
            values.Add("Blue", 0.5f);
            //Values.Add("Alpha", 6.5f);

            foreach ((string color, float value) in values)
            {
                // ConfigEntry's can be fully initialized in Config.Bind, but I find it more concise to separate out the definitions and descriptions.
                ConfigDefinition colorDef = new(headerName, $"Shield{color}");
                ConfigDescription colorDesc = new(color, new AcceptableValueRange<float>(0, 1));

                _color.Add(color, Config.Bind(colorDef, value, colorDesc));
            }

            ConfigDefinition sizeDef = new(headerName, $"ShieldSize");
            ConfigDescription sizeDesc = new("Size", new AcceptableValueRange<float>(0, 100));

            _size = Config.Bind(sizeDef, 6.5f, sizeDesc);
        }

        internal static void AddSettings()
        {
            SettingsTab tab = Settings.ModTab;

            _settingsElements.Add(tab.AddHeader($"{ProfileDataManager._current._characterFile._nickName}'s Shield Orb Color"));

            _settingsElements.Add(tab.AddToggle(_orbColorEnabled.Description.Description, _orbColorEnabled));

            foreach ((string color, ConfigEntry<float> value) in _color)
            {
                _settingsElements.Add(tab.AddAdvancedSlider(color, value));
            }

            _settingsElements.Add(tab.AddAdvancedSlider(_size.Description.Description, _size));
        }

        internal static void SendOrbColorPacket()
        {
            if (NetworkClient.active)
            {
                OrbColorPacket packet = new(
                    _orbColorEnabled.Value,
                    _color["Red"].Value,
                    _color["Green"].Value,
                    _color["Blue"].Value,
                    _size.Value);

                CodeTalkerNetwork.SendNetworkPacket(packet);
            }
        }

        internal static void ReceiveOrbColorPacket(PacketHeader header, PacketBase packet)
        {
            if (packet is OrbColorPacket orbColor)
            {
                Logger.LogInfo($"Packet Recieved | Plugin: {orbColor.PacketSourceGUID} | Steam ID: {header.SenderID} | PayLoad: {orbColor.Enabled}, {orbColor.Red}, {orbColor.Green}, {orbColor.Blue}, {orbColor.Size}");

                string key = header.SenderID.ToString();

                if (orbColor.Enabled)
                {
                    Color color = new(orbColor.Red, orbColor.Green, orbColor.Blue);

                    if (header.SenderIsLobbyOwner)
                    {
                        key = "localhost"; // Because the Player object sets the _steamID to "localhost" for the host, for some reason.
                    }

                    if (!_playerOrbColors.TryAdd(key, (orbColor.Enabled, color, orbColor.Size)))
                    {
                        _playerOrbColors[key] = (orbColor.Enabled, color, orbColor.Size);
                    }
                }
                else if (_playerOrbColors.ContainsKey(key))
                {
                    _playerOrbColors.Remove(key);
                }
            }
        }

        internal static void ReceivePlayerJoinPacket(PacketHeader header, PacketBase packet)
        {
            if (packet is PlayerJoinPacket && _orbColorEnabled.Value)
            {
                // If you rejoin a server, everyone should remove your old entry in case your new one has _orbColorEnabled = false.
                string key = header.SenderID.ToString();
                if (_playerOrbColors.ContainsKey(key))
                {
                    _playerOrbColors.Remove(key);
                }

                SendOrbColorPacket();
            }
        }

        internal static void SetOfflineOrbColor()
        {
            // If you're playing offline, CodeTalker doesn't work. So we have to set the orb color here.
            if (AtlyssNetworkManager._current._soloMode)
            {
                Color color = new(_color["Red"].Value, _color["Green"].Value, _color["Blue"].Value);

                if (!_playerOrbColors.TryAdd("localhost", (_orbColorEnabled.Value, color, _size.Value)))
                {
                    _playerOrbColors["localhost"] = (_orbColorEnabled.Value, color, _size.Value);
                }
            }
        }
    }
}
