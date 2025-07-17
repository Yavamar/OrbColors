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
        internal static ConfigEntry<bool> _customOrbColorEnabled;
        internal static Dictionary<string, ConfigEntry<float>> _myOrbColor = [];
        internal static Dictionary<string, (bool, Color)> _playerOrbColors = [];
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

            //Settings.OnInitialized.AddListener(AddSettings);
            Settings.OnApplySettings.AddListener(() => { base.Config.Save(); });

            CodeTalkerNetwork.RegisterListener<OrbColorPacket>(ReceiveOrbColorPacket);
            CodeTalkerNetwork.RegisterListener<PlayerJoinPacket>(ReceivePlayerJoinPacket);
        }

        internal static void InitConfig()
        {
            string headerName = ProfileDataManager._current._characterFile._nickName;

            ConfigDefinition EnabledDef = new(headerName, "OrbColorsEnabled");
            ConfigDescription EnabledDesc = new("Use Custom Orb Color?");
            _customOrbColorEnabled = Config.Bind(EnabledDef, false, EnabledDesc);

            Color BlockEmissionColor = new();

            Dictionary<string, float> Values = [];
            Values.Add("Red", BlockEmissionColor.r);
            Values.Add("Green", BlockEmissionColor.g);
            Values.Add("Blue", BlockEmissionColor.b);
            Values.Add("Alpha", BlockEmissionColor.a);

            foreach ((string Color, float Value) in Values)
            {
                // ConfigEntry's can be fully initialized in Config.Bind, but I find it more concise to separate out the definitions and descriptions.
                ConfigDefinition ConfigDef = new(headerName, $"Shield{Color}");
                ConfigDescription ConfigDesc = new(Color, new AcceptableValueRange<float>(0, 1));

                _myOrbColor.Add(Color, Config.Bind(ConfigDef, Value, ConfigDesc));
            }
        }

        internal static void AddSettings()
        {
            SettingsTab tab = Settings.ModTab;

            _settingsElements.Add(tab.AddHeader($"{ProfileDataManager._current._characterFile._nickName}'s Shield Orb Color"));

            _settingsElements.Add(tab.AddToggle(_customOrbColorEnabled.Description.Description, _customOrbColorEnabled));

            foreach ((string Color, ConfigEntry<float> Value) in _myOrbColor)
            {
                _settingsElements.Add(tab.AddAdvancedSlider(Color, Value));
            }
        }

        internal static void SendOrbColorPacket()
        {
            if (NetworkClient.active && _myOrbColor.Count == 4)
            {
                OrbColorPacket packet = new(
                    _customOrbColorEnabled.Value,
                    _myOrbColor["Red"].Value,
                    _myOrbColor["Green"].Value,
                    _myOrbColor["Blue"].Value,
                    _myOrbColor["Alpha"].Value);

                CodeTalkerNetwork.SendNetworkPacket(packet);
            }
        }

        internal static void ReceiveOrbColorPacket(PacketHeader header, PacketBase packet)
        {
            if (packet is OrbColorPacket orbColor)
            {
                Logger.LogInfo($"Packet Recieved | Plugin: {orbColor.PacketSourceGUID} | Steam ID: {header.SenderID} | PayLoad: {orbColor.Enabled}, {orbColor.Red}, {orbColor.Green}, {orbColor.Blue}, {orbColor.Alpha}");

                Color color = new(orbColor.Red, orbColor.Green, orbColor.Blue, orbColor.Alpha);
                string key = header.SenderID.ToString();

                if (header.SenderIsLobbyOwner)
                {
                    key = "localhost"; // Because the Player object sets the _steamID to "localhost" for the host, for some reason.
                }
                
                if(!_playerOrbColors.TryAdd(key, (orbColor.Enabled, color)))
                {
                    _playerOrbColors[key] = (orbColor.Enabled, color);
                }
            }
        }

        internal static void ReceivePlayerJoinPacket(PacketHeader header, PacketBase packet)
        {
            //Logger.LogInfo("Player with OrbColors joined.");
            if (packet is PlayerJoinPacket && _customOrbColorEnabled.Value)
            {
                //Logger.LogInfo("My custom orb color is enabled, so I should send it to them.");
                SendOrbColorPacket();
            }
        }
    }
}
