using System.Numerics;

namespace Monitoring.Operations;

public class MeanOperation<T>(string name) : IObservation<T> where T : INumber<T>, IMinMaxValue<T>
{
    public string Name { get; private set; } = name;

    public T Value { get; private set; } = T.MinValue;

    public bool ValueChanged { get; private set; } = false;

    public void Load(params Span<T> values)
    {
        T average = values.Average();
        ValueChanged = Value != average;

        if (ValueChanged)
        {
            Value = average;
        }
    }

    public void Load(T Value)
    {
        throw new NotImplementedException();
    }

    object? IObservation.Value => throw new NotImplementedException();

    public DateTime Timestamp => throw new NotImplementedException();
}
