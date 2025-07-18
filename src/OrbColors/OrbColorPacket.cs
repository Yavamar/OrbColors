using CodeTalker.Packets;
using Newtonsoft.Json;

namespace OrbColors;

public class OrbColorPacket(bool enabled, float red, float green, float blue, float size) : PacketBase
{
    public override string PacketSourceGUID => LCMPluginInfo.PLUGIN_GUID;

    [JsonProperty]
    public bool Enabled { get; set; } = enabled;
    [JsonProperty]
    public float Red { get; set; } = red;
    [JsonProperty]
    public float Green { get; set; } = green;
    [JsonProperty]
    public float Blue { get; set; } = blue;
    [JsonProperty]
    public float Size { get; set; } = size;
}

public class PlayerJoinPacket() : PacketBase
{
    public override string PacketSourceGUID => LCMPluginInfo.PLUGIN_GUID;
}