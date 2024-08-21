using System.Numerics;
using System.Text.Json.Nodes;

namespace Monitoring;

// public interface IOperationInfo
// {   
//     TimeSpan Frequency { get; }

//     DateTime LastObservation { get; set; }
// }

// public interface IOperation
// {
//     string Name { get; }

//     List<IObservation> Call();
// }

public interface IObservation
{
    string Id { get; }

    string Name { get; }

    object? Value { get; }

    DateTime Timestamp { get; }

    List<IOperation>? Operations { get; }
}

public interface IObservation<T> : IObservation
{
    void Load(T Value);

    new T? Value { get; }
}

public interface IJsonObservation<T> : IObservation<T> where T : INumber<T>
{
    void Load(JsonNode node);
}

public interface IEndpoint
{
    string Name { get; }

    string Uri { get; }

    List<IObservation> Observations { get; }

    List<IOperation>? Operations { get; }

}
