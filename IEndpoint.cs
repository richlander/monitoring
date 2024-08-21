namespace Monitoring;

public interface IEndpoint
{
    string Name { get; }

    string Uri { get; }

    Dictionary<Type, List<IObservation>> Observations { get; }
}
