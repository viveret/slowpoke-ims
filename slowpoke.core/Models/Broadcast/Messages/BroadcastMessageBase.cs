using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace slowpoke.core.Models.Broadcast.Messages;


public abstract class BroadcastMessageBase : IBroadcastMessage
{
    [JsonIgnore, IgnoreDataMember]
    public Guid OriginGuid { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public Guid EventGuid { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public string Type { get => GetType().FullName!; set => throw new NotSupportedException(); }
    
    [JsonIgnore, IgnoreDataMember]
    public DateTime? BroadcastSendDate { get; set; }
    
    [JsonIgnore, IgnoreDataMember]
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