
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json.Nodes;
using Monitoring;
using Monitoring.Operations;

HttpClient client = new();
string yetiUrl = "http://192.168.2.184/state";

JsonObservation<int> yetiTempObservation = new("Yeti Temperature", j => j.AsObject()["temperature"]?.GetValue<int>() ?? default);
JsonObservation<double> yetiWattsObservation = new("Yeti Watts Out", j => j.AsObject()["wattsOut"]?.GetValue<double>() ?? default);
JsonEndpoint yetiEndpoint = new("Yeti 1500X", yetiUrl, new()    
    {
        { typeof(int), [ yetiTempObservation ]},
        { typeof(double), [ yetiWattsObservation ]}
    });

Dashboard dashboard = new(
    "Rich's Home Dashboard",
    [ yetiEndpoint ],
    yetiTempObservation,
    yetiWattsObservation
    );

List<IObservation> observations = dashboard.Observations;

Console.WriteLine(dashboard.Name);

while (true)
{
    foreach (var endpoint in dashboard.Endpoints)
    {
        // Each endpoint type reqires special handling on invocation
        if (endpoint is JsonEndpoint jsonEndpoint)
        {
            // Call endpoint -- acquire new values
            Console.WriteLine($"Querying {jsonEndpoint.Name}");
            JsonNode jsonNode = await jsonEndpoint.CallEndpoint(client);

            // Load new values, per type
            JsonObservation<int>.UpdateValue(jsonNode, endpoint.Observations[typeof(int)]);
            JsonObservation<double>.UpdateValue(jsonNode, endpoint.Observations[typeof(double)]);
        }

        dashboard.Update();
    }

    if (dashboard.ValueChanged)
    {
        PrintDashboard(dashboard, observations);
        dashboard.ResetForNextRead();
    }
    else
    {
        Console.WriteLine($"No change. Time: {DateTime.UtcNow}; Last update: {dashboard.LastUpdated}");
    }

    await Task.Delay(TimeSpan.FromSeconds(10));
}

static void PrintDashboard(Dashboard dashboard, List<IObservation> observations)
{
    Console.WriteLine($"{nameof(dashboard.LastUpdated)}: {dashboard.LastUpdated}");
    PrintObservations(observations);
    Console.WriteLine();
}

static void PrintObservations(List<IObservation> observations)
{
    foreach (var observation in observations)
    {
        Console.WriteLine($"{observation.Name}: {observation.Value}"); 
    }
}
