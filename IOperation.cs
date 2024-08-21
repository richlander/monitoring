using System.Numerics;

namespace Monitoring;

public interface IOperation : IObservation
{
    bool ValueChanged { get; }
}

public interface IOperation<T> : IOperation, IObservation<T> where T : INumber<T>
{
    Observation<T> GetResult() => new(Name, Value, ValueChanged);

    void LoadObservation(Observation<T> observation);
}
