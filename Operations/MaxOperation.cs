using System.Numerics;

namespace Monitoring.Operations;

public class MaxOperation<T>(string name) : IObservation<T> where T : INumber<T>, IMinMaxValue<T>
{
    public string Name => name;

    public T Value { get; private set; } = T.MinValue;

    object? IObservation.Value => Value;

    public bool ValueChanged { get; private set; } = false;

    public DateTime Timestamp { get; private set; }

    public void Load(T value)
    {
        ValueChanged = value > Value;
        if (ValueChanged)
        {
            Value = value;
            Timestamp = DateTime.UtcNow;
        }
    }
}