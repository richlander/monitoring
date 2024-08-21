using System.Numerics;
using Monitoring.Operations;

namespace Monitoring;

public record YetiDashboard(string Name)
{
    public DateTime LastUpdated { get ; set;}
    public int Temperature { get ; set;}
    public double TemperatureRollingAverage { get ; set;}
    public double WattsOut { get ; set;}
    public double WattsOutRollingAverage { get ; set;}

    public void UpdateDashboard(IObservation<int> observation)
    {
        if (observation.Id == "yeti-temp")
        {
            Temperature = observation.Value;
            var operation = observation.Operations?.Where(o => o.GetType() == typeof(RollingAverageOperation<double>)).FirstOrDefault();

            if (operation is IOperation<double> ops)
            {
                ops.Load(observation.Value);
                TemperatureRollingAverage = ops.Value;
            }
        }
    }

    public void UpdateDashboard(IObservation<double> observation)
    {
    }

    public void UpdateDashboard(List<IObservation> observations, Type type)
    {
        if (type == typeof(int))
        {
            foreach (var observation in observations)
            {
                if (observation is IObservation<int> obs)
                {
                    UpdateDashboard(obs);
                }
            }
        }
        else if (type == typeof(double))
        {
            foreach (var observation in observations)
            {
                if (observation is IObservation<double> obs)
                {
                    UpdateDashboard(obs);
                }
            }
        }
    }
}
