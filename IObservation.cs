namespace Monitoring;

public interface IObservation
{
    string Name { get; }

    object? Value { get; }

    DateTime Timestamp { get; }
}

public interface IObservation<T> : IObservation
{
    void Load(T Value);

    new T? Value { get; }

    bool ValueChanged { get; }
}
