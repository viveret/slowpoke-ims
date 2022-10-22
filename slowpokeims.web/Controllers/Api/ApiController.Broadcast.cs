using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Broadcast;

namespace SlowPokeIMS.Web.Controllers.Api;


public partial class ApiController: ControllerBase
{
    [HttpGet("api/broadcast")]
    public ActionResult BroadcastReceive(CancellationToken cancellationToken)
    {
        var lastGuidReceived = Guid.Empty;
        var msgs = (Guid.Empty == lastGuidReceived ? BroadcastProviderResolver.MemCached.SentMessages : BroadcastProviderResolver.MemCached.SentMessages.SkipWhile(msg => msg.EventGuid != lastGuidReceived)).ToList();
        if (!msgs.Any())
        {
            return NoContent();
        }

        var data = new MemoryStream();
        data.Write(BitConverter.GetBytes(msgs.Count));
        foreach (var msg in msgs)
        {
            (msg.ConvertToRaw() as BroadcastMessageRaw)!.Serialize(data);
        }
        data.Position = 0;
        return File(data, "application/octet-stream");
    }

    [HttpPost("api/broadcast")]
    public ActionResult BroadcastSend(CancellationToken cancellationToken)
    {
        var numToReadBytes = new byte[4];
        Request.Body.Read(numToReadBytes);
        var numToRead = BitConverter.ToInt32(numToReadBytes);

        if (numToRead <= 0)
        {
            // why did you even call?
            return NoContent();
        }

        var msgs = new List<IBroadcastMessage>();
        while (msgs.Count < numToRead && !cancellationToken.IsCancellationRequested)
        {
            var msg = BroadcastMessageRaw.Deserialize(Request.Body);
            if (msg.OriginGuid == BroadcastProviderResolver.HttpKnownHosts.OriginGuid)
            {
                // should be using encryption to make sure this is not a spoofed guid
                // anyone can just edit the guid to any value
                msg.OriginGuid = Guid.Empty;
            }
            msgs.Add(msg);
        }
        BroadcastProviderResolver.MemCached.AddReceivedMessages(msgs);

        return new JsonResult(msgs.Count);
    }
}