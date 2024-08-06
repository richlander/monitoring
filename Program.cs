
using System.Dynamic;
using System.Numerics;
using System.Text.Json.Nodes;

HttpClient client = new();
Monitor monitor = new(client);
Endpoint<double> wattsOut = new("Yeti Watts Out", "http://192.168.2.184/state", j => j.AsObject()["wattsOut"]?.GetValue<double>() ?? throw new());
Observation<double> obsWatts = new("Watts out, short duration", TimeSpan.FromSeconds(5), wattsOut, new MinOperation<double>(), new MaxOperation<double>());
Observation<double> obsWattsLong = new("Watts out, longer duration (rolling average)", TimeSpan.FromSeconds(60), wattsOut, new RollingAverageOperation<double>());
Endpoint<double> ampsOut = new("Yeti Amps Out", "http://192.168.2.184/state", j => j.AsObject()["ampsOut"]?.GetValue<double>() ?? throw new());
Observation<double> obsAmps = new("Amps out, short duration (min/max)", TimeSpan.FromSeconds(10), ampsOut, new MinOperation<double>(), new MaxOperation<double>());
Endpoint<int> temperature = new("Yeti Temperature", "http://192.168.2.184/state", j => j.AsObject()["temperature"]?.GetValue<int>() ?? throw new());
Observation<int> obsTemp = new("Temperatue, long duration", TimeSpan.FromSeconds(60), temperature, new MinOperation<int>(), new MaxOperation<int>());

// Good info @ https://github.com/dotnet/runtime/pull/100316

List<IObservation> observations = [obsWatts, obsWattsLong, obsAmps, obsTemp];
List<Task<IObservation>> work = [];
work.AddRange(monitor.QueueDelay(observations));

while (true)
{
    try
    {
        using var cts = new CancellationTokenSource(5_000);
        await foreach (var task in Task.WhenEach(work).WithCancellation(cts.Token))
        {
            work.Remove(task);
            var observation = await task;

            if (observation is Observation<int> obsInt)
            {
                await Print(obsInt);
            }
            else if (observation is Observation<double> obsDouble)
            {
                await Print(obsDouble);
            }

            work.AddRange(monitor.QueueDelay(observation));
        }
    }
    catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
    {
    }
}

async Task Print<T>(Observation<T> observation) where T: INumber<T>
{
    var result = await monitor.Observe(observation);

    foreach (var op in result.Operations)
    {
        if (op.Changed)
        {
            Console.WriteLine($"{observation.Endpoint.Name}; {op.Name}; {op.Value};");
        }
    }
}

// void Log<T>(Observation<T> observation) where T: INumber<T>
// {
//     string filename = observation.Name.Replace(' ', '-');
//     File.OpenWrite()
// }


public record Observation<T>(string Name, TimeSpan Frequency, Endpoint<T> Endpoint, params List<IOperation<T>> Operations) : IObservation where T : INumber<T>
{
    public DateTime LastObservation { get; set; }

    public ObservationResult<T> GetResults()
    {
        List<OperationResult<T>> results = [];

        foreach (var operation in Operations)
        {
            results.Add(operation.GetResult());
        }

        ObservationResult<T> result = new(Name, LastObservation, results);
        return result;
    }
}

public record Endpoint<T>(string Name, string Uri, Func<JsonNode, T?> Func) where T : INumber<T>;

public record ObservationResult<T>(string Name, DateTime Time, params List<OperationResult<T>> Operations) where T : INumber<T>;

public record struct OperationResult<T>(string Name, T Value, bool Changed) where T : INumber<T>;

public interface IOperation<T> where T : INumber<T>
{
    string Name { get; }
    
    void Load(T value);

    T Value { get; }

    bool ValueChanged { get; }

    OperationResult<T> GetResult() => new(Name, Value, ValueChanged);
}

public interface IObservation
{
    TimeSpan Frequency { get; }

    DateTime LastObservation { get; set; }
}

public class MinOperation<T> : IOperation<T> where T : INumber<T>, IMinMaxValue<T>
{
    T _value = T.MaxValue;
    bool _valueChanged = false;

    public string Name => "Min value";

    public T Value => _value;

    public void Load(T value)
    {
        if (value < _value)
        {
            _valueChanged = true;
            _value = value;
            return;
        }

        if (_valueChanged is true)
        {
            _valueChanged = false;
        }
    }

    public bool ValueChanged => _valueChanged;
}

public class MaxOperation<T> : IOperation<T> where T : INumber<T>, IMinMaxValue<T>
{
    T _value = T.MinValue;
    bool _valueChanged = false;

    public string Name => "Max value";

    public T Value => _value;

    public void Load(T value)
    {
        if (value > _value)
        {
            _valueChanged = true;
            _value = value;
            return;
        }

        if (_valueChanged is true)
        {
            _valueChanged = false;
        }
    }

    public bool ValueChanged => _valueChanged;
}

public class RollingAverageOperation<T> : IOperation<T> where T : INumber<T>
{
    T _value = T.Zero;
    List<T> _values = [];

    bool _valueChanged = false;

    public string Name => "Rolling average";

    public T Value => _value;

    public void Load(T value)
    {
        _values.Add(value);

        if (_values.Count > 10)
        {
            _values.RemoveAt(0);
        }

        T sum = T.Zero;

        for (int i = 0; i < _values.Count; i++)
        {
            sum += _values[i];
        }

        T average = sum / T.CreateChecked(_values.Count);

        _valueChanged = average != _value;
        _value = average;
    }

    public bool ValueChanged => _valueChanged;
}
