using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using Monitoring.Operations;

namespace Monitoring;

public record Dashboard(string Name, List<IEndpoint> Endpoints, JsonObservation<int> TemperatureObservation, JsonObservation<double> WattsOutObservation)
{
    static private int _rollingAverageCount = 10;

    public int Temperature { get; private set; }

    public MinOperation<int> TemperatureMinOperation { get; init; } = new("Yeti Temperature Minimum");

    public MaxOperation<int> TemperatureMaxOperation { get; init; } = new("Yeti Temperature Maximum");

    public MeanOperation<double> TemperatureMeanOperation { get; init; } = new("Yeti Temperature Mean");

    public RollingAverageOperation<double> TemperatureRollingAverage { get; init; } = new("Yeti Temperature Rolling Average", _rollingAverageCount);

    public double WattsOut { get ; private set; }

    public MinOperation<double> WattsOutMinOperation { get; init; } = new("Yeti Watts Out Minimum");

    public MaxOperation<double> WattsOutMaxOperation { get; init; } = new("Yeti Watts Out Maximum");

    public MeanOperation<double> WattsOutMeanOperation { get; init; } = new("Yeti Watts Out Mean");

    public RollingAverageOperation<double> WattsOutRollingAverage { get; init; } = new("Yeti Watts Out Rolling Average", _rollingAverageCount);

    public bool ValueChanged { get; private set; } = true;

    public DateTime LastUpdated { get ; private set; }

    public void Update()
    {
        // Yeti Temperature
        ValueChanged |= Temperature == TemperatureObservation.Value;
        Temperature = TemperatureObservation.Value;
        TemperatureMinOperation.Load(Temperature);
        TemperatureMaxOperation.Load(Temperature);
        TemperatureMeanOperation.Load(TemperatureMinOperation.Value, TemperatureMaxOperation.Value);
        TemperatureRollingAverage.Load(Temperature);

        // Yeti Watts out
        ValueChanged |= WattsOut == WattsOutObservation.Value;
        WattsOut = WattsOutObservation.Value;
        WattsOutMinOperation.Load(WattsOut);
        WattsOutMaxOperation.Load(WattsOut);
        WattsOutMeanOperation.Load(WattsOutMinOperation.Value, WattsOutMaxOperation.Value);
        WattsOutRollingAverage.Load(WattsOut);

        // Update timestamp
        LastUpdated = DateTime.UtcNow;
    }

    public void ResetForNextRead() => ValueChanged = false; 

}
