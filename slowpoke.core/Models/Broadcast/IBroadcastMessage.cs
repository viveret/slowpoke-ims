namespace slowpoke.core.Models.Broadcast;


public interface IBroadcastMessage
{
    Guid OriginGuid { get; set; }

    Guid EventGuid { get; set; }
    
    string Type { get; set; }
    
    DateTime? BroadcastSendDate { get; set; }
    
    DateTime? BroadcastReceiveDate { get; set; }


    IBroadcastMessage ConvertToTrueType();
    
    IBroadcastMessage ConvertToRaw();
}