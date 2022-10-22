namespace slowpoke.core.Services;



public interface IServingInfoProvider
{
    string Ip { get; }
    short Port { get; }
}