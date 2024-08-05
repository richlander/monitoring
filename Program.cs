
using System.Dynamic;
using System.Numerics;
using System.Text.Json.Nodes;

HttpClient client = new();
Monitor monitor = new(client);
Endpoint<double> wattsOut = new("Yeti Watts Out", "http://192.168.2.184/state", j => j.AsObject()["wattsOut"]?.GetValue<double>() ?? throw new());
Observation<double> obsWatts = new(TimeSpan.FromSeconds(5), wattsOut, new MinOperation<double>(), new MaxOperation<double>());
Endpoint<double> ampsOut = new("Yeti Amps Out", "http://192.168.2.184/state", j => j.AsObject()["ampsOut"]?.GetValue<double>() ?? throw new());
Observation<double> obsAmps = new(TimeSpan.FromSeconds(10), ampsOut, new MinOperation<double>(), new MaxOperation<double>());
Endpoint<int> temperature = new("Yeti Temperature", "http://192.168.2.184/state", j => j.AsObject()["temperature"]?.GetValue<int>() ?? throw new());
Observation<int> obsTemp = new(TimeSpan.FromSeconds(60), temperature, new MinOperation<int>(), new MaxOperation<int>());

// Good info @ https://github.com/dotnet/runtime/pull/100316

List<IObservation> observations = [obsWatts, obsAmps, obsTemp];
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
    catch (TaskCanceledException)
    {
    }
}

async Task Print<T>(Observation<T> observation) where T: INumber<T>
{
    var result = await monitor.Observe(observation);

    foreach (var op in result.Operations)
    {
        Console.WriteLine($"{observation.Endpoint.Name}; {op.Name}; {op.Value}; {observation.Frequency}; {observation.LastObservation}");
    }
}

public record Observation<T>(TimeSpan Frequency, Endpoint<T> Endpoint, params List<IOperation<T>> Operations) : IObservation where T : INumber<T>
{
    public DateTime LastObservation { get; set; }

    public ObservationResult<T> GetResults()
    {
        List<OperationResult<T>> results = [];

        foreach (var operation in Operations)
        {
            results.Add(operation.GetResult());
        }

        ObservationResult<T> result = new(Endpoint.Name, LastObservation, results);

        return result;
    }
}

public record Endpoint<T>(string Name, string Uri, Func<JsonNode, T?> Func) where T : INumber<T>;

public record ObservationResult<T>(string Name, DateTime Time, params List<OperationResult<T>> Operations) where T : INumber<T>;

public record struct OperationResult<T>(string Name, T Value) where T : INumber<T>;

public interface IOperation<T> where T : INumber<T>
{
    string Name { get; }
    
    void Load(T value);

    T Value { get; }

    bool ValueChanged { get; }

    OperationResult<T> GetResult() => new(Name, Value);
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

    public string Name => "Min operation";

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

    public string Name => "Max operation";

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

// public class MeanOperation<T>(IOperation<T> minOperation, IOperation<T> maxOperation) : IOperation<T> where T : INumber<T>
// {
//     readonly IOperation<T> _minOperation = minOperation;
//     readonly IOperation<T> _maxOperation = maxOperation;

//     public string Name => "Mean operation";

//     public T Value => CalculateMean();

//     private T CalculateMean()
//     {
//         T min = _minOperation.Value;
//         T max = _maxOperation.Value;

//         var value = (min + max) / T.CreateChecked(2);
//         return value;
//     }

//     public void Load(T value)
//     {
//         throw new NotImplementedException();
//     }

//     public bool ValueChanged { get; set; } = false;
// }
