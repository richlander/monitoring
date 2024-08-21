
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json.Nodes;
using Monitoring;
using Monitoring.Operations;

/*
Design goals:

- Be able to collect multiple values for each call to the webserver, potentially of different types
- Multiple endpoint implementations should be possible, even though only `JsonEndpoint` is implemented.
- Operations (like `RollingAverageOperation`) should not need to match the type of the observation
- Operations should be very generic (hence why generic math is being used).
- The dashboard and the observations should be tightly coupled so that dashboard -> observation
 interfaction is easy (a little like the "code behind" model).
- The main program should be small, primarily setup, a while loop and visualization.

Initially, I tried to include the operations as an additional `IOperation` property on `IObservation`.
That was possible, but required a lot more type tests and complex/confusing mechanics. Using the dashboard as the
proper home for the observations and the operations vastly simplified all of that, leaving the `Update` method
to control the final operations and to be non-generic. I also wanted to make the operations as `IOservation` which
resulted on each of the operations exposing a nullable `Operations` property. Hardly a disaster, but odd. In any case,
that all got scrapped for the current much simpler model.
*/

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
