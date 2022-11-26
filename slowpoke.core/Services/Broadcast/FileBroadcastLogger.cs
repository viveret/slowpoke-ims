using System.Text;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Configuration;

namespace SlowPokeIMS.Core.Services.Broadcast;


public class FileBroadcastLogger : IBroadcastLogger
{
    private readonly FileInfo f;
    private FileStream? destination;

    public FileBroadcastLogger(Config config)
    {
        this.f = new FileInfo(Path.Combine(config.Paths.AppRootPath, "broadcast-sent-messages.csv"));
    }

    public void BeginLogging()
    {
        this.destination = f.Open(FileMode.Append);
    }

    public void EndLogging()
    {
        this.destination?.Close();
        this.destination = null;
    }

    public void Log(IBroadcastMessage msg)
    {
        var msgStr = msg.ConvertToRaw().ToString();
        if (destination == null)
        {
            throw new InvalidOperationException("Cannot call log with no destination stream to write to");
        }
        destination.Write(Encoding.ASCII.GetBytes(msgStr!));
        destination.WriteByte((byte)'\n');
        destination.Flush();
    }
}