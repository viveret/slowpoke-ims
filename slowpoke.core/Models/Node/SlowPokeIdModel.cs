namespace slowpoke.core.Models.Node;


public class SlowPokeIdModel : ISlowPokeId
{
    public string Label { get; set; } = string.Empty;

    public Guid Guid { get; set; } = Guid.Empty;

    public Guid[] GuidAlternatives { get; set; } = Array.Empty<Guid>();

    public Uri Endpoint { get; set; }

    public Uri[] EndpointAlternatives { get; set; } = Array.Empty<Uri>();

    public string RawIdType { get; set; } = string.Empty;

    public string RawId { get; set; } = string.Empty;


    public override string? ToString() => $"{Label} ({Guid}) - {Endpoint} <{RawIdType}-{RawId}>";
}