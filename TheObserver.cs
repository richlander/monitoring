// using System.Numerics;
// using System.Text.Json.Nodes;
// using Monitoring;

// namespace Monitoring;

// public class TheObserver(HttpClient client)
// {
//     readonly HttpClient _client = client;

//     public async Task<ObservationResult<T>> Observe<T>(ObservationOperation<T> observation) where T : INumber<T>
//     {
//         var value = await CallJsonEndPoint(observation.Endpoint);
        
//         foreach (var operation in observation.Operations)
//         {
//             operation.Load(value);
//         }
//         observation.LastObservation = DateTime.UtcNow;

//         return observation.GetResults();
//     }

//     public async Task<T> CallJsonEndPoint<T>(JsonEndpoint<T> endpoint) where T : INumber<T>
//     {
//         var json = await _client.GetStringAsync(endpoint.Uri);
//         var node = JsonNode.Parse(json) ?? throw new();
//         return endpoint.Func(node) ?? throw new();
//     }
// }
