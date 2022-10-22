namespace slowpoke.core.Models.Node;


public class SlowPokeIdModel : ISlowPokeId
{
    public string Label { get; set; }

    public Guid Guid { get; set; }

    public Guid[] GuidAlternatives { get; set; }

    public Uri Endpoint { get; set; }

    public Uri[] EndpointAlternatives { get; set; }

    public string RawIdType { get; set; }

    public string RawId { get; set; }


    public override string? ToString() => $"{Label} ({Guid}) - {Endpoint} <{RawIdType}-{RawId}>";
}