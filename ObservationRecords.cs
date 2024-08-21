using System.Net;
using System.Numerics;
using System.Text.Json.Nodes;

namespace Monitoring;

/*
    Terminology:

    JsonEndpoint -- Some JSON endpoint that can produce one or more observable results
    Observation -- A datapoint derived from the endpoint
    Operation -- Some algorithm that can be performed on an observation

*/

public record JsonEndpoint(string Name, string Uri, List<IObservation> Observations) : IEndpoint
{
    public List<IOperation>? Operations { get; set; }

    public async Task<JsonNode> CallEndpoint(HttpClient client)
    {
        string json = await client.GetStringAsync(Uri);
        JsonNode node = JsonNode.Parse(json) ?? throw new();
        return node;
    }
};

public record JsonObservation<T>(string Id, string Name, Func<JsonNode, T?> Func, List<IOperation>? Operations = null) : IJsonObservation<T> where T : INumber<T>
{
    public DateTime Timestamp { get; private set; }

    public T? Value { get; private set;}

    object? IObservation.Value => Value;

    public void Load(JsonNode node)
    {
        Value = Func(node);
        Timestamp = DateTime.UtcNow;
    }

    public static void UpdateValue(JsonNode node, JsonObservation<T> observation)
    {
        observation.Load(node);
    }

    public static void UpdateValue(JsonNode node, List<IObservation> observations)
    {
        foreach (var observation in observations)
        {
            if (observation is JsonObservation<T> obs)
            {
                UpdateValue(node, obs);
            }
        }
    }

    public void Load(T value) => Value = value;
}

// public record ObservationGroup<T>(string Name, TimeSpan Frequency, List<JsonObservation<T>> Observations, List<IOperation<T>>? Operations) where T : INumber<T>;

// public record ObservationResult<T>(string Name, DateTime Time, params List<Observation<T>> Observations) where T : INumber<T>;

public record struct Observation<T>(string Name, T Value, bool Changed) where T : INumber<T>;

public record ObservationOperation(string Name, TimeSpan Frequency, IEndpoint Endpoint, List<IOperation> Operations)
{
    public DateTime LastObservation { get; set; }

    // public ObservationResult<T> GetResults()
    // {
    //     List<Observation<T>> results = [];
    //     List<Observation<T>> derivedResults = [];


    //     foreach (var operation in Operations)
    //     {
    //         results.Add(operation.GetResult());
    //     }

    //     foreach (var scalarOp in DerivedScalarOperations ?? [])
    //     {
    //         foreach (var operation in Operations)
    //         {
    //             scalarOp.Load(operation.Value);
    //             derivedResults.Add(scalarOp.GetResult());
    //         }
    //     }

    //     foreach (var vectorOperation in DerivedVectorOperations ?? [])
    //     {
    //         vectorOperation.Load(Operations);            
    //         derivedResults.Add(vectorOperation.GetResult());
    //     }

    //     results.AddRange(derivedResults);
    //     ObservationResult<T> result = new(Name, LastObservation, results);
    //     return result;
    // }
}
