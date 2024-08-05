
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
Observation<int> obsTemp = new(TimeSpan.FromSeconds(30), temperature, new MinOperation<int>(), new MaxOperation<int>());

// Good info @ https://github.com/dotnet/runtime/pull/100316
while (true)
{
    var task1 = Observe<double>(obsWatts, obsAmps);
    var task2 = Observe<int>(obsTemp);
    await Task.WhenAny(task1, task2);
}

async Task Observe<T>(params List<Observation<T>> observations) where T : INumber<T>
{
    await foreach (var task in Task.WhenEach(monitor.Observe<T>(observations)))
    {
        var o = await task;
        foreach (var op in o.Operations)
        {
            if (op.ValueChanged)
            {
                Console.WriteLine($"{o.Endpoint.Name}; {op.Name}; {op.GetResult()}");
            }

        }
    }
}

public record Observation<T>(TimeSpan Frequency, Endpoint<T> Endpoint, params IOperation<T>[] Operations) where T : INumber<T>
{
    public DateTime LastObservation { get; set; }
}

public record Endpoint<T>(string Name, string Uri, Func<JsonNode, T?> Func) where T : INumber<T>;

public interface IOperation<T> where T : INumber<T>
{
    string Name { get; }
    
    void Load(T value);

    bool ValueChanged { get; }

    T GetResult();
}

public class MinOperation<T> : IOperation<T> where T : INumber<T>
{
    T _value = T.Zero;
    bool _firstLoad = true;
    bool _valueChanged = false;

    public string Name => "Min operation";

    public T GetResult() => _value;

    public void Load(T value)
    {
        if (_firstLoad)
        {
            _value = value;
            _valueChanged = true;
            _firstLoad = false;
        }
        else if (value < _value)
        {
            _value = value; 
            _valueChanged = true;
        }
        else
        {
            _valueChanged = false;
        }
    }

    public bool ValueChanged => _valueChanged;
}

public class MaxOperation<T> : IOperation<T> where T : INumber<T>
{
    T _value = T.Zero;
    bool _firstLoad = true;
    bool _valueChanged = false;

    public string Name => "Max operation";

    public T GetResult() => _value;

    public void Load(T value)
    {
        if (_firstLoad)
        {
            _value = value;
            _valueChanged = true;
            _firstLoad = false;
        }
        else if (value > _value)
        {
            _valueChanged = true;
            _value = value; 
        }
        else
        {
            _valueChanged = false;
        }
    }

    public bool ValueChanged => _valueChanged;
}

public class MeanOperation<T>(IOperation<T> minOperation, IOperation<T> maxOperation) : IOperation<T> where T : INumber<T>
{
    IOperation<T> _minOperation = minOperation;
    IOperation<T> _maxOperation = maxOperation;

    public string Name => "Mean operation";

    public T GetResult()
    {
        T min = _minOperation.GetResult();
        T max = _maxOperation.GetResult();

        var value = (min + max) / T.CreateChecked(2);
        return value;
    }

    public void Load(T value)
    {
        throw new NotImplementedException();
    }

    public bool ValueChanged { get; set; } = false;
}