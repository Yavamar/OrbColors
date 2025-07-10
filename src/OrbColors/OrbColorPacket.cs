using CodeTalker.Packets;
using Newtonsoft.Json;

namespace OrbColors;

public class OrbColorPacket(bool enabled, float red, float green, float blue, float alpha) : PacketBase
{
    /*
        This example packet contains a simple Payload property,
        but you can include any number of properties of any type.
        However, I strongly recommend sticking to primitive types.

        Remember: Your data is serialized and sent across the network,
        so reference types (like classes) will not point to the same
        object instance on both ends.

        Instead, rely on value types or objects that can be represented
        as values (such as strings, ints, uints, floats, etc) when 
        sending messages.
    */

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