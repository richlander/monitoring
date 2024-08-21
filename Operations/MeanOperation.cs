using System.Numerics;

namespace Monitoring.Operations;

public class MeanOperation<T>(string name) : IObservation<T> where T : INumber<T>, IMinMaxValue<T>
{
    public string Name { get; private set; } = name;

    public T Value { get; private set; } = T.MinValue;

    object? IObservation.Value => Value;

    public bool ValueChanged { get; private set; } = false;

    public DateTime Timestamp { get; private set; }

    public void Load(params Span<T> values)
    {
        T average = values.Average();
        ValueChanged = Value != average;

        if (ValueChanged)
        {
            Value = average;
            Timestamp = DateTime.UtcNow;
        }
    }

    public void Load(T Value)
    {
        throw new NotImplementedException();
    }
}
