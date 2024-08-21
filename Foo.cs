namespace Monitoring;

public record EndPointMaps(IEndpoint Endpoint, Dictionary<Type, List<IObservation>> Observations, Dictionary<Type, List<IOperation>>? Operations);
