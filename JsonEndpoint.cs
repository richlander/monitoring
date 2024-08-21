using System.Text.Json.Nodes;

namespace Monitoring;

public record JsonEndpoint(string Name, string Uri, Dictionary<Type, List<IObservation>> Observations) : IEndpoint
{
    public async Task<JsonNode> CallEndpoint(HttpClient client)
    {
        string json = await client.GetStringAsync(Uri);
        JsonNode node = JsonNode.Parse(json) ?? throw new();
        return node;
    }
};
