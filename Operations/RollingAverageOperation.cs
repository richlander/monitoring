using System.Net.NetworkInformation;
using System.Numerics;

namespace Monitoring.Operations;

public class RollingAverageOperation<T>(string name) : IOperation<T> where T : INumber<T>
{
    private T _value = T.Zero;
    private readonly List<T> _values = [];

    public static string OperationId = "rolling-average";

    bool _valueChanged = false;

    public string Name { get; } = name;

    public T Value => _value;

    object? IObservation.Value => Value;

    public void Load(T value)
    {
        _values.Add(value);
        T average = _values.Average();
        _valueChanged = average != _value;
        _value = average;

        if (_values.Count > 9)
        {
            _values.RemoveAt(0);
        }
    }

    public void LoadObservation(Observation<T> observation) => Load(observation.Value);

    public bool ValueChanged => _valueChanged;

    public string Id => OperationId;

    public DateTime Timestamp => throw new NotImplementedException();

    public List<IOperation>? Operations => throw new NotImplementedException();
}