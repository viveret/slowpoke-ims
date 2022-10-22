using System.Text.Json;
using System.Text.Json.Serialization;

namespace slowpoke.core.Models.Broadcast.Messages;


public abstract class BroadcastMessageBase : IBroadcastMessage
{
    [JsonIgnore]
    public Guid OriginGuid { get; set; }

    [JsonIgnore]
    public Guid EventGuid { get; set; }

    [JsonIgnore]
    public string Type { get => GetType().FullName; set => throw new NotSupportedException(); }
    
    [JsonIgnore]
    public DateTime? BroadcastSendDate { get; set; }
    
    [JsonIgnore]
    public DateTime? BroadcastReceiveDate { get; set; }

    public IBroadcastMessage ConvertToRaw()
    {
        return new BroadcastMessageRaw
        {
            OriginGuid = OriginGuid,
            EventGuid = EventGuid,
            Type = Type,
            BroadcastSendDate = BroadcastSendDate,
            BroadcastReceiveDate = BroadcastReceiveDate,
            Value = JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions { WriteIndented = false })
        };
    }

    public IBroadcastMessage ConvertToTrueType() => this;
}