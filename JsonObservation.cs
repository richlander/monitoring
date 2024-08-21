using System.Numerics;
using System.Text.Json.Nodes;

namespace Monitoring;

public record JsonObservation<T>(string Name, Func<JsonNode, T?> Func) : IObservation<T> where T : INumber<T>
{
    public T? Value { get; private set;}

    object? IObservation.Value => Value;

    public bool ValueChanged { get; private set; } = false;

    public DateTime Timestamp { get; private set; }

    public void Load(T value) => Value = value;

    public void Load(JsonNode node)
    {
        T? value = Func(node);
        ValueChanged = Value != value;
        Value = value;
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
}