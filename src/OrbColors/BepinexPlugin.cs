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

        internal static new ManualLogSource Logger = null!;

        private void Awake()
        {
            Logger = base.Logger;

            Logger.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

            new Harmony(LCMPluginInfo.PLUGIN_GUID).PatchAll(Assembly.GetExecutingAssembly());

            InitConfig();

            Config.SaveOnConfigSet = false;

            Settings.OnInitialized.AddListener(AddSettings);
            Settings.OnApplySettings.AddListener(() => { Config.Save(); });

            CodeTalkerNetwork.RegisterListener<OrbColorPacket>(ReceiveOrbColorPacket);
            Logger.LogMessage("Created a packet listener");
        }

        private void InitConfig()
        {
            ConfigDefinition EnabledDef = new("Example Category", "OrbColorsEnabled");
            ConfigDescription EnabledDesc = new("Use Custom Orb Color?");
            _customOrbColorEnabled = Config.Bind(EnabledDef, true, EnabledDesc);

            Color BlockEmissionColor = new();

            Dictionary<string, float> Values = [];
            Values.Add("Red", BlockEmissionColor.r);
            Values.Add("Green", BlockEmissionColor.g);
            Values.Add("Blue", BlockEmissionColor.b);
            Values.Add("Alpha", BlockEmissionColor.a);

            foreach ((string Color, float Value) in Values)
            {
                // ConfigEntry's can be fully initialized in Config.Bind, but I find it more concise to separate out the definitions and descriptions.
                ConfigDefinition ConfigDef = new("Example Category", $"Shield{Color}");
                ConfigDescription ConfigDesc = new(Color, new AcceptableValueRange<float>(0, 1));

                _myOrbColor.Add(Color, Config.Bind(ConfigDef, Value, ConfigDesc));
            }
        }

        private void AddSettings()
        {
            SettingsTab tab = Settings.ModTab;

            tab.AddHeader("Custom Shield Orb Color");

            tab.AddToggle(_customOrbColorEnabled.Description.Description, _customOrbColorEnabled);

            foreach ((string Color, ConfigEntry<float> Value) in _myOrbColor)
            {
                tab.AddAdvancedSlider(Color, Value);
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
                
                if(_playerOrbColors.ContainsKey(key))
                {
                    _playerOrbColors[key] = (orbColor.Enabled, color);
                }
                else
                {
                    _playerOrbColors.Add(key, (orbColor.Enabled, color));
                }
            }
        }

    }
}
