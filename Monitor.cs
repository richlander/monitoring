using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

public class Monitor(HttpClient client)
{
    readonly HttpClient _client = client;

    public async Task<ObservationResult<T>> Observe<T>(Observation<T> observation) where T : INumber<T>
    {
        var value = await CallJsonEndPoint(observation.Endpoint);
        
        foreach (var operation in observation.Operations)
        {
            operation.Load(value);
        }
        observation.LastObservation = DateTime.UtcNow;

        return observation.GetResults();
    }

    public IEnumerable<Task<ObservationResult<T>>> Observe<T>(params IEnumerable<Observation<T>> observations) where T : INumber<T>
    {
        foreach (var observation in observations)
        {
            yield return Observe(observation);
        }
    }

    public IEnumerable<Task<IObservation>> QueueDelay(params IEnumerable<IObservation> observations)
    {
        foreach (var observation in observations)
        {
            var nextObservationTime = observation.LastObservation + observation.Frequency;
            var delayTime = TimeSpan.Zero;
            var time = DateTime.UtcNow;

            if (time < nextObservationTime)
            {
                delayTime = nextObservationTime - time;
            }

            yield return Task.Run(() => { Task.Delay(delayTime).GetAwaiter().GetResult(); return observation; });
        }
    }

    public async Task<T> CallJsonEndPoint<T>(Endpoint<T> endpoint) where T : INumber<T>
    {
        var json = await _client.GetStringAsync(endpoint.Uri);
        var node = JsonNode.Parse(json) ?? throw new();
        return endpoint.Func(node) ?? throw new();
    }
}
