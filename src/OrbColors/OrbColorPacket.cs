using CodeTalker.Packets;
using Newtonsoft.Json;

namespace OrbColors;

public class OrbColorPacket(bool enabled, float red, float green, float blue, float alpha) : PacketBase
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
    public float Alpha { get; set; } = alpha;
}