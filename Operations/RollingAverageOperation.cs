using System.Net.NetworkInformation;
using System.Numerics;

namespace Monitoring.Operations;

public class RollingAverageOperation<T>(string name, int count) : IObservation<T> where T : INumber<T>
{
    private readonly List<T> _values = [];

    public string Name { get; } = name;

    public int Count = count;

    public T Value { get; private set; } = T.Zero;

    object? IObservation.Value => Value;

    public bool ValueChanged { get; private set; }

    public DateTime Timestamp { get; private set; }

    public void Load(T value)
    {
        _values.Add(value);
        T average = _values.Average();
        ValueChanged = average != Value;

        if (ValueChanged)
        {
            Value = average;
            Timestamp = DateTime.UtcNow;
        }

        if (_values.Count >= Count)
        {
            _values.RemoveAt(0);
        }
    }
}