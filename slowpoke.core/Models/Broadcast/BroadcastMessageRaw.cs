using System.Reflection;
using System.Text;
using System.Text.Json;

namespace slowpoke.core.Models.Broadcast;


public class BroadcastMessageRaw: IBroadcastMessage
{
    public Guid OriginGuid { get; set; }

    public Guid EventGuid { get; set; }

    public string Type { get; set; }
    
    public DateTime? BroadcastSendDate { get; set; }
    
    public DateTime? BroadcastReceiveDate { get; set; }

    public string Value { get; set; }

    public IBroadcastMessage ConvertToTrueType()
    {
        var type = Assembly.GetExecutingAssembly().GetType(Type);
        return (IBroadcastMessage)System.Text.Json.JsonSerializer.Deserialize(Value, type, new JsonSerializerOptions {  })!;
    }

    public IBroadcastMessage ConvertToRaw() => this;

    public void Serialize(Stream stream)
    {
        BroadcastSendDate ??= DateTime.UtcNow;

        var typeBytes = Encoding.ASCII.GetBytes(Type);
        var valueBytes = Encoding.UTF8.GetBytes(Value);

        stream.Write(OriginGuid.ToByteArray());
        stream.Write(EventGuid.ToByteArray());
        stream.Write(BitConverter.GetBytes(typeBytes.Length));
        stream.Write(typeBytes);
        stream.Write(BitConverter.GetBytes(BroadcastSendDate.Value.Ticks));
        stream.Write(BitConverter.GetBytes(valueBytes.Length));
        stream.Write(valueBytes);
    }

    public static BroadcastMessageRaw Deserialize(Stream stream)
    {
        var ret = new BroadcastMessageRaw();
        ret.BroadcastReceiveDate = DateTime.UtcNow;

        var originGuidBytes = new byte[16];
        stream.Read(originGuidBytes);
        ret.OriginGuid = new Guid(originGuidBytes);

        var guidBytes = new byte[16];
        stream.Read(guidBytes);
        ret.EventGuid = new Guid(guidBytes);

        ret.Type = Encoding.ASCII.GetString(DeserializeVarCharBytes(stream));

        var sendDateBytes = new byte[8];
        stream.Read(sendDateBytes);
        ret.BroadcastSendDate = new DateTime(BitConverter.ToInt64(sendDateBytes));

        ret.Value = Encoding.UTF8.GetString(DeserializeVarCharBytes(stream));

        return ret;
    }

    private static byte[] DeserializeVarCharBytes(Stream stream)
    {
        var lengthBytes = new byte[4];
        stream.Read(lengthBytes);
        var typeLength = BitConverter.ToInt32(lengthBytes);
        var typeBytes = new byte[typeLength];
        stream.Read(typeBytes);
        return typeBytes;
    }

    public override string ToString() => $"{OriginGuid}-{EventGuid}-{Type}-{BroadcastSendDate?.Ticks ?? 0}-{BroadcastReceiveDate?.Ticks ?? 0}-{Value}";

    public static BroadcastMessageRaw Parse(string str)
    {
        const int guidLength = 32 + 4; // 32 digits and 4 dashes
        var parts = str.Substring(guidLength * 2 + 1 * 2).Split('-', 4);
        return new BroadcastMessageRaw
        {
            OriginGuid = Guid.Parse(str.Substring(0, guidLength)),
            EventGuid = Guid.Parse(str.Substring(guidLength + 1, guidLength)),
            Type = parts[0],
            BroadcastSendDate = new DateTime(long.Parse(parts[1])),
            BroadcastReceiveDate = new DateTime(long.Parse(parts[2])),
            Value = parts[3],
        };
    }
}