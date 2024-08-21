// using System.Numerics;

// namespace Monitoring.Operations;

// public class MeanOperation<T> : IVectorOperation<T> where T : INumber<T>, IMinMaxValue<T>
// {
//     T _value = T.MinValue;
//     bool _valueChanged = false;

//     public string Name => "Mean value";

//     public T Value => _value;

//     public void Load(params Span<T> values)
//     {
//         T average = values.Average();
//         _valueChanged = average != _value;
//         _value = average;
//     }

//     public void Load(params List<IOperation<T>> values)
//     {
//         T average = values.Average();
//         _valueChanged = average != _value;
//         _value = average;
//     }

//     public void Load(params List<Observation<T>> values)
//     {
//         T average = values.Average();
//         _valueChanged = average != _value;
//         _value = average;
//     }

//     public bool ValueChanged => _valueChanged;
// }
