
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json.Nodes;
using Monitoring;
using Monitoring.Operations;
// using Monitoring.Operations;

HttpClient client = new();
// TheObserver observer = new(client);
string yetiUrl = "http://192.168.2.184/state";

YetiDashboard dashboard = new("Yeti 1500X");
RollingAverageOperation<double> yetiTempRollingAverage = new("yeti-temp-rolling");
JsonObservation<int> yetiTempObservation = new("yeti-temp", "Yeti Temperature", j => j.AsObject()["temperature"]?.GetValue<int>() ?? default, [ yetiTempRollingAverage ]);
JsonEndpoint yetiEndpoint = new("Yeti", yetiUrl, [yetiTempObservation]);
List<EndPointMaps> endpoints = GetEndPointMaps([yetiEndpoint]);

while (true)
{
    foreach (var endpoint in endpoints)
    {
        // Each endpoint type reqires special handling on invocation
        if (endpoint.Endpoint is JsonEndpoint jsonEndpoint)
        {
            // Call endpoint -- acquire new values
            JsonNode jsonNode = await yetiEndpoint.CallEndpoint(client);

            // Load new values, per type
            JsonObservation<int>.UpdateValue(jsonNode, endpoint.Observations[typeof(IObservation<int>)]);
            JsonObservation<double>.UpdateValue(jsonNode, endpoint.Observations[typeof(IObservation<double>)]);
        }

        // Update dashboard, per type
        dashboard.UpdateDashboard(endpoint.Observations[typeof(IObservation<int>)], typeof(int));
        dashboard.UpdateDashboard(endpoint.Observations[typeof(IObservation<double>)], typeof(double));
    }

    PrintDashboard(dashboard);
    await Task.Delay(TimeSpan.FromSeconds(5));
}

void PrintDashboard(YetiDashboard dashboard)
{
    Console.WriteLine($"Temp: {dashboard.Temperature}; Temp Rolling Average: {dashboard.TemperatureRollingAverage}");
}

// foreach (var observation in yetiEndpoint.Observations)
// {
//     if (observation.Id == "yeti-temp" && observation is IObservation<int> tempObservation)
//     {
//         dashboard.Temperature = tempObservation.Value;
//         double value = tempObservation.Value;
//         IOperation<double>? operation = RunOperation<double>(value, operations, RollingAverageOperation<double>.OperationId);
//         if (operation != null)
//         {
//             dashboard.TemperatureRollingAverage = operation.Value;
//         }
//     }
// }

Dictionary<Type, List<IObservation>> GetObservationMap(List<IObservation> observations)
{
    Dictionary<Type, List<IObservation>> map = new()
    {
        { typeof(IObservation<int>), [] },
        { typeof(IObservation<double>), [] }
    };

    foreach (var observation in observations)
    {
        if (observation is IObservation<int>)
        {
            map[typeof(IObservation<int>)].Add(observation);
        }
        else if (observation is IObservation<double>)
        {
            map[typeof(IObservation<double>)].Add(observation);
        }
    }

    return map;
}

Dictionary<Type, List<IOperation>>? GetOperationsMap(List<IOperation>? operations)
{
    if (operations is null)
    {
        return null;
    }

    Dictionary<Type, List<IOperation>> map = new()
    {
        { typeof(IOperation<int>), [] },
        { typeof(IOperation<double>), [] }
    };

    foreach (var operation in operations)
    {
        if (operation is IOperation<int>)
        {
            map[typeof(IOperation<int>)].Add(operation);
        }
        else if (operation is IOperation<double>)
        {
            map[typeof(IOperation<double>)].Add(operation);
        }
    }

    return map;
}

List<EndPointMaps> GetEndPointMaps(List<IEndpoint> endpoints)
{
    List<EndPointMaps> maps = [];

    foreach (var endpoint in endpoints)
    {
        var obsMap = GetObservationMap(endpoint.Observations);
        var opsMap = GetOperationsMap(endpoint.Operations);
        maps.Add(new(endpoint, obsMap, opsMap));
    }

    return maps;
}

// void UpdateDashboard(YetiDashboard dashboard, Dictionary<Type, List<IObservation>> observarions)
// {
//     foreach (var observation in endpoint.Observations)
//     {
//         if (observation.Id == "yeti-temp" && observation is IObservation<int> tempObservation)
//         {
//             dashboard.Temperature = tempObservation.Value;
//             double value = tempObservation.Value;
//             IOperation<double>? operation = RunOperation<double>(value, operations, RollingAverageOperation<double>.OperationId);
//             if (operation != null)
//             {
//                 dashboard.TemperatureRollingAverage = operation.Value;
//             }
//         }
//     }
// }

// IOperation<T>? RunOperation<T>(T value, List<IOperation> operations, string name) where T : INumber<T>
// {
//     foreach (var operation in operations)
//     {
//         if (operation.Id == name && operation is IOperation<T> op)
//         {
//             op.Load(value);
//             return op;
//         }
//     }

//     return null;
// }




// JsonEndpoint<double> wattsOut = new("Yeti Watts Out", yetiUrl, j => j.AsObject()["wattsOut"]?.GetValue<double>() ?? throw new());
// ObservationOperation<double> obsWatts = new("Watts out, short duration", TimeSpan.FromSeconds(5), wattsOut, new MinOperation<double>(), new MaxOperation<double>());
// ObservationOperation<double> obsWattsLong = new("Watts out, longer duration (rolling average)", TimeSpan.FromSeconds(60), wattsOut, new RollingAverageOperation<double>());
// JsonEndpoint<double> ampsOut = new("Yeti Amps Out", yetiUrl, j => j.AsObject()["ampsOut"]?.GetValue<double>() ?? throw new());
// ObservationOperation<double> obsAmps = new("Amps out, short duration (min/max)", TimeSpan.FromSeconds(10), ampsOut, new MinOperation<double>(), new MaxOperation<double>());
// JsonEndpoint<int> temperature = new("Yeti Temperature", yetiUrl, j => j.AsObject()["temperature"]?.GetValue<int>() ?? throw new());
// ObservationOperation<int> obsTemp = new("Temperatue, long duration", TimeSpan.FromSeconds(60), temperature, new MinOperation<int>(), new MaxOperation<int>());

// List<IOperationInfo> observations = [obsWatts, obsWattsLong, obsAmps, obsTemp];
// TimeSpan delay = observations.MinBy(o => o.Frequency)?.Frequency ?? TimeSpan.FromMinutes(1);

// while (true)
// {
//     foreach (var observation in observations)
//     {
//         if (DateTime.UtcNow < observation.LastObservation + observation.Frequency)
//         {
//             continue;
//         }

//         if (observation is ObservationOperation<int> obsInt)
//         {
//             await Print(obsInt);
//         }
//         else if (observation is ObservationOperation<double> obsDouble)
//         {
//             await Print(obsDouble);
//         }
//     }

//     await Task.Delay(delay);
// }

// async Task Print<T>(ObservationOperation<T> observation) where T: INumber<T>
// {
//     var result = await observer.Observe(observation);

//     foreach (var op in result.Observations)
//     {
//         if (op.Changed)
//         {
//             Console.WriteLine($"{observation.Endpoint.Name}; {op.Name}; {op.Value};");
//         }
//     }
// }

// // void Log<T>(Observation<T> observation) where T: INumber<T>
// // {
// //     string filename = observation.Name.Replace(' ', '-');
// //     File.OpenWrite()
// // }
