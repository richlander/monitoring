namespace Monitoring;

public interface IObservation
{
    string Name { get; }

    object? Value { get; }

    bool ValueChanged { get; }

    DateTime Timestamp { get; }
}

public interface IObservation<T> : IObservation
{
    void Load(T Value);

    new T? Value { get; }
}
