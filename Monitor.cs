using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

public class Monitor(HttpClient client)
{
    readonly HttpClient _client = client;

    public async Task<Observation<T>> Observe<T>(Observation<T> observation) where T : INumber<T>
    {
        var nextObservationTime = observation.LastObservation + observation.Frequency;
        var time = DateTime.UtcNow;

        if (time < nextObservationTime)
        {
            var delayTime = nextObservationTime - time;
            await Task.Delay(delayTime);
        }

        observation.LastObservation = DateTime.UtcNow;
        var value = await CallJsonEndPoint(observation.Endpoint);
            
        foreach (var operation in observation.Operations)
        {
            operation.Load(value);
        }

        return observation;
    }

    public IEnumerable<Task<Observation<T>>> Observe<T>(params List<Observation<T>> observations) where T : INumber<T>
    {
        foreach (var observation in observations)
        {
            yield return Observe(observation);
        }
    }

    public async Task<T> CallJsonEndPoint<T>(Endpoint<T> endpoint) where T : INumber<T>
    {
        var json = await _client.GetStringAsync(endpoint.Uri);
        var node = JsonNode.Parse(json) ?? throw new();
        return endpoint.Func(node) ?? throw new();
    }
}
