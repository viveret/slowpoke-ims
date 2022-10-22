namespace slowpoke.core.Models.Node;


public interface ISlowPokeId
{
    string Label { get; } // re-assignable by user
    Guid Guid { get; } // primary id
    Guid[] GuidAlternatives { get; } // nodes, docs, folders, and hosts can be known by multiple GUIDs
    Uri Endpoint { get; } // primary endpoint
    Uri[] EndpointAlternatives { get; } // nodes, docs, folders, and hosts  can be known by multiple endpoints
    string RawIdType { get; } // how the nodes, docs, folders, or hosts are being identified currently
    string RawId { get; } // the current id of the nodes, docs, folders, and hosts as identified by the type
}